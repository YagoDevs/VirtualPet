using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reproduz sons específicos quando determinadas animações entram em execução.
/// Configure a lista no Inspector para escolher qual áudio toca em cada estado.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class PetAnimationAudioPlayer : MonoBehaviour
{
    [System.Serializable]
    private class AnimationSoundConfig
    {
        public enum MatchMode
        {
            StateName,
            ClipName
        }

        [Tooltip("Modo de correspondência: pelo nome do State (default) ou pelo nome do Clip dentro do BlendTree.")]
        public MatchMode matchBy = MatchMode.StateName;

        [Tooltip("Nome do estado no Animator OU nome do clip, dependendo do modo selecionado.")]
        public string stateName;

        [Tooltip("Som que será tocado quando o estado começar")]
        public AudioClip clip;

        [Tooltip("Volume individual deste som")]
        [Range(0f, 1f)]
        public float volume = 1f;

        [Tooltip("Repete o áudio enquanto o estado continuar ativo")]
        public bool loopWhileActive = true;

        [Tooltip("Tempo de espera entre repetições quando em loop (0 = sem pausa).")]
        [Min(0f)] public float loopCooldownSeconds = 0f;

        [Tooltip("Limite máximo em segundos para o loop (0 = enquanto o estado estiver ativo)")]
        public float maxLoopDuration = 0f;

        [HideInInspector] public int stateHash;

        public void RefreshHash()
        {
            // Só gera hash quando comparando por StateName.
            if (matchBy == MatchMode.StateName)
            {
                stateHash = string.IsNullOrWhiteSpace(stateName)
                    ? 0
                    : Animator.StringToHash(stateName);
            }
            else
            {
                stateHash = 0;
            }
        }
    }

    [Header("Referências")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    [Header("Configurações de Som por Animação")]
    [SerializeField] private List<AnimationSoundConfig> animationSounds = new List<AnimationSoundConfig>();

    private int currentStateHash = -1;
    private string currentClipName = null;
    private AnimationSoundConfig currentConfig;
    private Coroutine stopLoopRoutine;
    private Coroutine manualLoopRoutine;

    private void Awake()
    {
        CacheComponents();
        CacheStateHashes();
    }

    private void Update()
    {
        if (animator == null) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        int shortHash = stateInfo.shortNameHash;
        string clipName = null;

        // Pega o primeiro clip ativo na layer 0 (útil para BlendTrees).
        // Usa GetCurrentAnimatorClipInfoWeighted para pegar o clip mais pesado em BlendTrees
        AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
        if (clips != null && clips.Length > 0)
        {
            // Se há múltiplos clips (BlendTree), pega o que tem maior peso
            AnimatorClipInfo bestClip = clips[0];
            float maxWeight = clips[0].weight;
            for (int i = 1; i < clips.Length; i++)
            {
                if (clips[i].weight > maxWeight && clips[i].clip != null)
                {
                    maxWeight = clips[i].weight;
                    bestClip = clips[i];
                }
            }
            if (bestClip.clip != null)
            {
                clipName = bestClip.clip.name;
            }
        }

        // Sempre verifica mudança de clip, mesmo que o stateHash seja o mesmo
        // Isso é importante para BlendTrees onde Idle e Happy compartilham o mesmo stateHash
        bool clipChanged = !string.Equals(clipName, currentClipName, StringComparison.OrdinalIgnoreCase);
        bool stateChanged = shortHash != currentStateHash;

        if (stateChanged || clipChanged)
        {
            OnAnimationChanged(shortHash, clipName);
        }

        ApplyLiveAdjustments();
    }

    private void OnAnimationChanged(int nextStateHash, string clipName)
    {
        Log($"[Audio] Change detected: stateHash={nextStateHash}, clip='{clipName ?? "null"}' (anterior: '{currentClipName ?? "null"}')");
        currentStateHash = nextStateHash;
        currentClipName = clipName;
        StopLoopTimer();
        StopManualLoop();

        AnimationSoundConfig config = FindConfig(nextStateHash, clipName);
        currentConfig = config;

        if (config == null || config.clip == null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            Log($"[Audio] Nenhuma config encontrada para stateHash={nextStateHash} clip='{clipName}'. Verifique se o nome está correto e se Match By está configurado corretamente.");
            return;
        }

        PlayClipWithConfig(config);

        if (config.loopWhileActive && config.maxLoopDuration > 0f)
        {
            stopLoopRoutine = StartCoroutine(StopAfterDelay(config.maxLoopDuration));
        }
    }

    private AnimationSoundConfig FindConfig(int targetStateHash, string clipName)
    {
        for (int i = 0; i < animationSounds.Count; i++)
        {
            AnimationSoundConfig config = animationSounds[i];
            if (config.matchBy == AnimationSoundConfig.MatchMode.StateName)
            {
                if (config.stateHash == targetStateHash)
                {
                    Log($"[Audio] Config encontrada por StateName: '{config.stateName}' (hash={config.stateHash})");
                    return config;
                }
            }
            else // ClipName
            {
                if (!string.IsNullOrWhiteSpace(clipName) &&
                    !string.IsNullOrWhiteSpace(config.stateName) &&
                    string.Equals(config.stateName, clipName, StringComparison.OrdinalIgnoreCase))
                {
                    Log($"[Audio] Config encontrada por ClipName: '{config.stateName}' (clip atual: '{clipName}')");
                    return config;
                }
            }
        }

        // Log detalhado para debug
        if (debugLogs)
        {
            string availableConfigs = "";
            for (int i = 0; i < animationSounds.Count; i++)
            {
                var cfg = animationSounds[i];
                availableConfigs += $"\n  [{i}] MatchBy={cfg.matchBy}, Name='{cfg.stateName}', Hash={cfg.stateHash}, HasClip={cfg.clip != null}";
            }
            Log($"[Audio] Nenhuma config correspondeu. Procurando: stateHash={targetStateHash}, clipName='{clipName}'. Configs disponíveis:{availableConfigs}");
        }

        return null;
    }

    private void PlayClipWithConfig(AnimationSoundConfig config)
    {
        audioSource.clip = config.clip;
        audioSource.volume = config.volume;
        audioSource.loop = config.loopWhileActive && config.loopCooldownSeconds <= 0f;
        audioSource.time = 0f;
        audioSource.Play();
        Log($"[Audio] Tocando '{config.clip.name}' por {(config.matchBy == AnimationSoundConfig.MatchMode.StateName ? "state" : "clip")}='{config.stateName}', loop={audioSource.loop}, cooldown={config.loopCooldownSeconds}");

        if (config.loopWhileActive && config.loopCooldownSeconds > 0f)
        {
            manualLoopRoutine = StartCoroutine(ManualLoop(config));
        }
    }

    private IEnumerator ManualLoop(AnimationSoundConfig config)
    {
        while (currentConfig == config && audioSource.clip == config.clip)
        {
            float wait = (config.clip != null ? config.clip.length : 0f) + config.loopCooldownSeconds;
            if (wait <= 0f) wait = 0.01f;
            yield return new WaitForSeconds(wait);

            // se mudou de estado/clip, sai
            if (currentConfig != config || audioSource == null) yield break;

            audioSource.time = 0f;
            audioSource.Play();
            Log($"[Audio] Repetindo '{config.clip.name}' com cooldown={config.loopCooldownSeconds}");
        }
    }

    private void StopManualLoop()
    {
        if (manualLoopRoutine == null) return;
        StopCoroutine(manualLoopRoutine);
        manualLoopRoutine = null;
    }

    /// <summary>
    /// Permite ajustar volume/loop em tempo real no Inspector enquanto o áudio toca.
    /// </summary>
    private void ApplyLiveAdjustments()
    {
        if (currentConfig == null || audioSource == null) return;
        if (audioSource.clip != currentConfig.clip) return;

        audioSource.volume = currentConfig.volume;
        // Se não há cooldown, loop direto; se há cooldown, mantemos loop off e o manual cuida.
        audioSource.loop = currentConfig.loopWhileActive && currentConfig.loopCooldownSeconds <= 0f;
    }

    private IEnumerator StopAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        audioSource.Stop();
        stopLoopRoutine = null;
    }

    private void CacheComponents()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    private void CacheStateHashes()
    {
        if (animationSounds == null) return;

        for (int i = 0; i < animationSounds.Count; i++)
        {
            animationSounds[i].RefreshHash();
        }
    }

    private void StopLoopTimer()
    {
        if (stopLoopRoutine == null) return;

        StopCoroutine(stopLoopRoutine);
        stopLoopRoutine = null;
    }

    private void Log(string message)
    {
        if (debugLogs)
        {
            Debug.Log(message, this);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheComponents();
        CacheStateHashes();
    }
#endif
}


