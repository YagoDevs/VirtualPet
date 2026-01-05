using UnityEngine;
using System.Collections;

public class DayNightCycleController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PetControllerSimple petController;
    [SerializeField] private Light mainLight;
    [SerializeField] private GameObject sunObject;
    [SerializeField] private GameObject moonObject;
    
    [Header("Áudio")]
    [SerializeField] private AudioClip dayAmbientSound;
    [SerializeField] private AudioClip nightAmbientSound;
    [Range(0f, 1f)] [SerializeField] private float maxVolume = 0.5f;
    [SerializeField] private float audioTransitionSpeed = 0.5f;
    
    [Header("Skybox")]
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material nightSkybox;
    
    [Header("Configurações")]
    [SerializeField] private float nightEnergyThreshold = 80f;
    [SerializeField] private float dayEnergyThreshold = 20f;
    [SerializeField] private bool invertLogic = true;
    [SerializeField] private float transitionSpeed = 1f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float scaleDuration = 1.5f;
    [SerializeField] private float minScale = 0.3f;
    
    [Header("Luz")]
    [SerializeField] private Color dayLightColor = new Color(1f, 0.96f, 0.84f);
    [SerializeField] private float dayLightIntensity = 1.5f;
    [SerializeField] private Vector3 dayLightRotation = new Vector3(50f, -30f, 0f);
    [SerializeField] private Color nightLightColor = new Color(0.6f, 0.7f, 1f);
    [SerializeField] private float nightLightIntensity = 0.5f;
    [SerializeField] private Vector3 nightLightRotation = new Vector3(-10f, 45f, 0f);
    
    [Header("Ambiente")]
    [SerializeField] private Color dayAmbientSky = new Color(0.53f, 0.81f, 0.92f);
    [SerializeField] private Color dayAmbientEquator = new Color(1f, 0.78f, 0.59f);
    [SerializeField] private Color dayAmbientGround = new Color(0.31f, 0.39f, 0.24f);
    [SerializeField] private Color nightAmbientSky = new Color(0.1f, 0.15f, 0.3f);
    [SerializeField] private Color nightAmbientEquator = new Color(0.2f, 0.2f, 0.4f);
    [SerializeField] private Color nightAmbientGround = new Color(0.05f, 0.05f, 0.1f);
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    private float currentBlend = 1f;
    private float targetBlend = 1f;
    private bool wasDay = true;
    
    private AudioSource dayAudioSource, nightAudioSource;
    private Renderer[] sunRenderers, moonRenderers;
    private Vector3 sunOriginalScale, moonOriginalScale;
    private Coroutine sunFadeCoroutine, moonFadeCoroutine, sunScaleCoroutine, moonScaleCoroutine;
    
    private void Start()
    {
        petController = petController ?? FindObjectOfType<PetControllerSimple>();
        mainLight = mainLight ?? FindObjectOfType<Light>();
        
        if (daySkybox != null) RenderSettings.skybox = daySkybox;
        
        SetupDayNightObjects();
        SetupAudio();
    }
    
    private void Update()
    {
        if (petController == null) return;
        
        CalculateTargetBlend();
        currentBlend = Mathf.Lerp(currentBlend, targetBlend, Time.deltaTime * transitionSpeed);
        
        ApplySkyboxTransition();
        ApplyLightTransition();
        ApplyAmbientTransition();
        ApplyAudioTransition();
        UpdateDayNightObjects();
        
        if (showDebugInfo) ShowDebugInfo();
    }
    
    private void CalculateTargetBlend()
    {
        float energy = petController.energy;
        float range = nightEnergyThreshold - dayEnergyThreshold;
        
        if (energy >= nightEnergyThreshold)
            targetBlend = invertLogic ? 0f : 1f;
        else if (energy <= dayEnergyThreshold)
            targetBlend = invertLogic ? 1f : 0f;
        else
        {
            float normalizedEnergy = (energy - dayEnergyThreshold) / range;
            targetBlend = invertLogic ? 1f - normalizedEnergy : normalizedEnergy;
        }
    }
    
    private void ApplySkyboxTransition()
    {
        Material targetSkybox = currentBlend > 0.5f ? daySkybox : nightSkybox;
        if (targetSkybox != null && RenderSettings.skybox != targetSkybox)
        {
            RenderSettings.skybox = targetSkybox;
            DynamicGI.UpdateEnvironment();
        }
        RenderSettings.ambientIntensity = Mathf.Lerp(0.6f, 1.2f, currentBlend);
    }
    
    private void ApplyLightTransition()
    {
        if (mainLight == null) return;
        
        mainLight.color = Color.Lerp(nightLightColor, dayLightColor, currentBlend);
        mainLight.intensity = Mathf.Lerp(nightLightIntensity, dayLightIntensity, currentBlend);
        mainLight.transform.rotation = Quaternion.Euler(Vector3.Lerp(nightLightRotation, dayLightRotation, currentBlend));
    }
    
    private void ApplyAmbientTransition()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Color.Lerp(nightAmbientSky, dayAmbientSky, currentBlend);
        RenderSettings.ambientEquatorColor = Color.Lerp(nightAmbientEquator, dayAmbientEquator, currentBlend);
        RenderSettings.ambientGroundColor = Color.Lerp(nightAmbientGround, dayAmbientGround, currentBlend);
    }
    
    private void SetupDayNightObjects()
    {
        if (sunObject != null)
        {
            sunObject.SetActive(true);
            sunOriginalScale = sunObject.transform.localScale;
            sunRenderers = sunObject.GetComponentsInChildren<Renderer>();
            SetObjectAlpha(sunRenderers, 1f);
        }
        
        if (moonObject != null)
        {
            moonObject.SetActive(true);
            moonOriginalScale = moonObject.transform.localScale;
            moonRenderers = moonObject.GetComponentsInChildren<Renderer>();
            SetObjectAlpha(moonRenderers, 0f);
            moonObject.transform.localScale = moonOriginalScale * minScale;
        }
    }
    
    private void UpdateDayNightObjects()
    {
        bool isCurrentlyDay = currentBlend > 0.5f;
        
        if (isCurrentlyDay != wasDay)
        {
            if (isCurrentlyDay)
            {
                AnimateObject(sunObject, sunRenderers, ref sunFadeCoroutine, ref sunScaleCoroutine, sunOriginalScale, true);
                AnimateObject(moonObject, moonRenderers, ref moonFadeCoroutine, ref moonScaleCoroutine, moonOriginalScale, false);
                if (showDebugInfo) Debug.Log("☀️ Amanheceu!");
            }
            else
            {
                AnimateObject(sunObject, sunRenderers, ref sunFadeCoroutine, ref sunScaleCoroutine, sunOriginalScale, false);
                AnimateObject(moonObject, moonRenderers, ref moonFadeCoroutine, ref moonScaleCoroutine, moonOriginalScale, true);
                if (showDebugInfo) Debug.Log("🌙 Anoiteceu!");
            }
            
            wasDay = isCurrentlyDay;
        }
    }
    
    private void AnimateObject(GameObject obj, Renderer[] renderers, ref Coroutine fadeCoroutine, 
                               ref Coroutine scaleCoroutine, Vector3 originalScale, bool fadeIn)
    {
        if (obj == null) return;
        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        
        float targetAlpha = fadeIn ? 1f : 0f;
        Vector3 targetScale = fadeIn ? originalScale : originalScale * minScale;
        
        fadeCoroutine = StartCoroutine(AnimateAlpha(renderers, GetObjectAlpha(renderers), targetAlpha, fadeDuration));
        scaleCoroutine = StartCoroutine(AnimateScale(obj.transform, obj.transform.localScale, targetScale, scaleDuration));
    }
    
    private void SetupAudio()
    {
        if (dayAmbientSound != null)
        {
            dayAudioSource = gameObject.AddComponent<AudioSource>();
            dayAudioSource.clip = dayAmbientSound;
            dayAudioSource.loop = true;
            dayAudioSource.volume = maxVolume;
            dayAudioSource.Play();
        }
        
        if (nightAmbientSound != null)
        {
            nightAudioSource = gameObject.AddComponent<AudioSource>();
            nightAudioSource.clip = nightAmbientSound;
            nightAudioSource.loop = true;
            nightAudioSource.volume = 0f;
            nightAudioSource.Play();
        }
    }
    
    private void ApplyAudioTransition()
    {
        if (dayAudioSource != null)
            dayAudioSource.volume = Mathf.Lerp(dayAudioSource.volume, currentBlend * maxVolume, Time.deltaTime * audioTransitionSpeed);
        
        if (nightAudioSource != null)
            nightAudioSource.volume = Mathf.Lerp(nightAudioSource.volume, (1f - currentBlend) * maxVolume, Time.deltaTime * audioTransitionSpeed);
    }
    
    private IEnumerator AnimateAlpha(Renderer[] renderers, float startAlpha, float targetAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t; // Ease In-Out
            
            SetObjectAlpha(renderers, Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }
        SetObjectAlpha(renderers, targetAlpha);
    }
    
    private IEnumerator AnimateScale(Transform target, Vector3 startScale, Vector3 targetScale, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // Ease Out Back
            const float overshoot = 1.70158f;
            t = 1f + (overshoot + 1f) * Mathf.Pow(t - 1f, 3f) + overshoot * Mathf.Pow(t - 1f, 2f);
            
            target.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        target.localScale = targetScale;
    }
    
    private void SetObjectAlpha(Renderer[] renderers, float alpha)
    {
        if (renderers == null) return;
        
        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;
            
            foreach (Material mat in rend.materials)
            {
                if (mat == null || !mat.HasProperty("_Color")) continue;
                
                Color color = mat.color;
                color.a = alpha;
                mat.color = color;
                
                if (alpha < 1f && mat.renderQueue < 3000)
                {
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.renderQueue = 3000;
                }
            }
        }
    }
    
    private float GetObjectAlpha(Renderer[] renderers)
    {
        if (renderers == null || renderers.Length == 0) return 1f;
        
        float totalAlpha = 0f;
        int count = 0;
        
        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;
            foreach (Material mat in rend.materials)
            {
                if (mat != null && mat.HasProperty("_Color"))
                {
                    totalAlpha += mat.color.a;
                    count++;
                }
            }
        }
        
        return count > 0 ? totalAlpha / count : 1f;
    }
    
    private void ShowDebugInfo()
    {
        string timeOfDay = currentBlend > 0.5f ? "DIA ☀️" : "NOITE 🌙";
        string petState = invertLogic ? 
            (currentBlend > 0.5f ? "CANSADA 😴" : "ATIVA 🦉") : 
            (currentBlend > 0.5f ? "ATIVA ☀️" : "CANSADA 😴");
        
        Debug.Log($"[Day/Night] Energia: {petController.energy:F0} | Blend: {currentBlend:F2} | {timeOfDay} | Pet: {petState}");
    }
    
    // Métodos Públicos
    public void ForceDay()
    {
        targetBlend = currentBlend = 1f;
        ApplySkyboxTransition();
        ApplyLightTransition();
        ApplyAmbientTransition();
        AnimateObject(sunObject, sunRenderers, ref sunFadeCoroutine, ref sunScaleCoroutine, sunOriginalScale, true);
        AnimateObject(moonObject, moonRenderers, ref moonFadeCoroutine, ref moonScaleCoroutine, moonOriginalScale, false);
        wasDay = true;
    }
    
    public void ForceNight()
    {
        targetBlend = currentBlend = 0f;
        ApplySkyboxTransition();
        ApplyLightTransition();
        ApplyAmbientTransition();
        AnimateObject(sunObject, sunRenderers, ref sunFadeCoroutine, ref sunScaleCoroutine, sunOriginalScale, false);
        AnimateObject(moonObject, moonRenderers, ref moonFadeCoroutine, ref moonScaleCoroutine, moonOriginalScale, true);
        wasDay = false;
    }
    
    public float GetCurrentBlend() => currentBlend;
    public bool IsDay() => currentBlend > 0.5f;
    public void StopAllAudio()
    {
        if (dayAudioSource != null) dayAudioSource.Stop();
        if (nightAudioSource != null) nightAudioSource.Stop();
    }
    public void SetMaxVolume(float volume) => maxVolume = Mathf.Clamp01(volume);
    
    private void OnDestroy()
    {
        if (sunFadeCoroutine != null) StopCoroutine(sunFadeCoroutine);
        if (moonFadeCoroutine != null) StopCoroutine(moonFadeCoroutine);
        if (sunScaleCoroutine != null) StopCoroutine(sunScaleCoroutine);
        if (moonScaleCoroutine != null) StopCoroutine(moonScaleCoroutine);
    }
    
    private void OnValidate()
    {
        if (invertLogic && nightEnergyThreshold <= dayEnergyThreshold)
            nightEnergyThreshold = dayEnergyThreshold + 10f;
        else if (!invertLogic && dayEnergyThreshold <= nightEnergyThreshold)
            dayEnergyThreshold = nightEnergyThreshold + 10f;
    }
}
