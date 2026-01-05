using UnityEngine;

/// <summary>
/// Controlador do Pet que usa apenas floats no Animator (Hunger, Energy, Confidence).
/// Não usa triggers. As transições entre estados são automáticas baseadas nos valores.
/// </summary>
public class PetControllerSimple : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    
    [Header("Stats do Pet (0-100)")]
    [Range(0, 100)]
    public float hunger = 100f;      // 100 = cheio, 0 = faminto
    
    [Range(0, 100)]
    public float energy = 100f;      // 100 = descansado, 0 = exausto
    
    [Range(0, 100)]
    public float confidence = 50f;   // 100 = feliz, 0 = tímido
    
    [Header("Decay Rates (por segundo)")]
    [SerializeField] private float hungerDecayRate = 1f;      // Fome diminui ao longo do tempo
    [SerializeField] private float energyDecayRate = 0.5f;    // Energia diminui ao longo do tempo (quando > 30)
    [SerializeField] private float energyDecayWhenLow = 0.2f; // Energia diminui mais devagar quando < 30 (pet descansando)
    [SerializeField] private float confidenceDecayRate = 0.2f; // Confiança diminui se não interagir
    
    [Header("Valores dos Botões")]
    [SerializeField] private float feedHungerAmount = 25f;      // Quanto alimentar restaura
    [SerializeField] private float feedConfidenceBonus = 10f;   // Bonus de confiança ao alimentar
    [SerializeField] private float feedEnergyCost = 5f;         // Comer gasta um pouco de energia
    
    [SerializeField] private float sleepEnergyAmount = 30f;     // Quanto descansar restaura
    [SerializeField] private float sleepHungerCost = 10f;       // Dormir gasta fome
    [SerializeField] private float sleepConfidenceBonus = 5f;   // Bonus de confiança ao descansar
    
    // Hash IDs para performance
    private int hungerHash;
    private int energyHash;
    private int confidenceHash;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        
        // Cache dos hashes dos parâmetros (melhora performance)
        hungerHash = Animator.StringToHash("Hunger");
        energyHash = Animator.StringToHash("Energy");
        confidenceHash = Animator.StringToHash("Confidence");
    }

    private void Start()
    {
        // Inicializa os parâmetros do Animator
        UpdateAnimatorParameters();
    }

    private void Update()
    {
        // Atualiza necessidades ao longo do tempo
        UpdateNeeds();
        
        // Atualiza parâmetros do Animator (sempre!)
        UpdateAnimatorParameters();
    }

    private void UpdateNeeds()
    {
        // Fome diminui sempre
        hunger -= hungerDecayRate * Time.deltaTime;
        
        // Sistema de Energia (SEM regeneração automática!):
        if (energy < 30f)
        {
            // Quando Energy < 30, o Animator entra em RestBlend (pet descansando)
            // Energia continua caindo, mas mais devagar (pet está descansando)
            energy -= energyDecayWhenLow * Time.deltaTime;
        }
        else
        {
            // Energia diminui normalmente quando acordado (Energy > 30)
            energy -= energyDecayRate * Time.deltaTime;
        }
        
        // Confiança diminui lentamente se não houver interação
        confidence -= confidenceDecayRate * Time.deltaTime;
        
        // Clamp todos os valores entre 0 e 100
        hunger = Mathf.Clamp(hunger, 0f, 100f);
        energy = Mathf.Clamp(energy, 0f, 100f);
        confidence = Mathf.Clamp(confidence, 0f, 100f);
    }

    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;
        
        // Atualiza os três floats do Animator
        // O Animator FSM faz as transições automaticamente baseado nestes valores
        animator.SetFloat(hungerHash, hunger);
        animator.SetFloat(energyHash, energy);
        animator.SetFloat(confidenceHash, confidence);
    }

    /// <summary>
    /// Alimenta o pet (chamado pelo botão de Feed)
    /// </summary>
    public void Feed()
    {
        // Aumenta fome (pet come e fica satisfeito)
        hunger += feedHungerAmount;
        
        // Aumenta confiança (pet se sente cuidado)
        confidence += feedConfidenceBonus;
        
        // Comer gasta um pouco de energia
        energy -= feedEnergyCost;
        
        // Clamp todos os valores
        hunger = Mathf.Clamp(hunger, 0f, 100f);
        energy = Mathf.Clamp(energy, 0f, 100f);
        confidence = Mathf.Clamp(confidence, 0f, 100f);
        
        // Atualiza Animator imediatamente
        UpdateAnimatorParameters();
        
        Debug.Log($"🍖 Pet alimentado! Hunger: {hunger:F0}, Confidence: {confidence:F0}");
    }
    
    /// <summary>
    /// Método alternativo para compatibilidade (chama Feed)
    /// </summary>
    public void OnFeedButton()
    {
        Feed();
    }

    /// <summary>
    /// Faz o pet descansar (chamado pelo botão de Sleep/Rest)
    /// </summary>
    public void Sleep()
    {
        // Aumenta energia (pet descansa)
        energy += sleepEnergyAmount;
        
        // Dormir gasta fome (tempo passa)
        hunger -= sleepHungerCost;
        
        // Aumenta confiança (pet se sente seguro)
        confidence += sleepConfidenceBonus;
        
        // Clamp todos os valores
        hunger = Mathf.Clamp(hunger, 0f, 100f);
        energy = Mathf.Clamp(energy, 0f, 100f);
        confidence = Mathf.Clamp(confidence, 0f, 100f);
        
        // Atualiza Animator imediatamente
        UpdateAnimatorParameters();
        
        Debug.Log($"💤 Pet descansando! Energy: {energy:F0}, Confidence: {confidence:F0}");
    }
    
    /// <summary>
    /// Método alternativo para compatibilidade (chama Sleep)
    /// </summary>
    public void OnSleepButton()
    {
        Sleep();
    }

    // Mostra informações na tela para debug
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Box("=== Pet Status ===");
        GUILayout.Label($"Fome (Hunger): {hunger:F1}");
        GUILayout.Label($"Energia (Energy): {energy:F1}");
        GUILayout.Label($"Confiança (Confidence): {confidence:F1}");
        GUILayout.Space(10);
        GUILayout.Label("=== Info ===");
        GUILayout.Label("Use os botões UI para interagir");
        GUILayout.Label("Hunger < 40 → EatBlend");
        GUILayout.Label("Energy < 30 → RestBlend");
        GUILayout.EndArea();
    }
}

