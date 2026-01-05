using UnityEngine;
using UnityEngine.UI;
using TMPro; // Se você usar TextMeshPro

/// <summary>
/// Gerencia a UI do Pet - barras, textos e botões.
/// </summary>
public class PetUIManager : MonoBehaviour
{
    [Header("Referência ao Pet")]
    [SerializeField] private PetControllerSimple petController;
    
    [Header("Botões Circulares de Status")]
    [SerializeField] private CircularStatusButton hungerStatusButton;
    [SerializeField] private CircularStatusButton energyStatusButton;
    [SerializeField] private CircularStatusButton confidenceStatusButton; // Opcional
    
    [Header("Textos - Valores (Opcional)")]
    [SerializeField] private TextMeshProUGUI hungerText;  // OU use "Text" se não tiver TextMeshPro
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI confidenceText; // Opcional
    
    [Header("Barras (opcional)")]
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider energySlider;
    
    [Header("Botões de Ação")]
    [SerializeField] private Button feedButton;
    [SerializeField] private Button sleepButton;

    private void Start()
    {
        // Busca o PetController se não foi atribuído
        if (petController == null)
        {
            petController = FindObjectOfType<PetControllerSimple>();
        }
        
        // Configura os botões
        if (feedButton != null)
        {
            feedButton.onClick.AddListener(OnFeedButtonClicked);
        }
        
        if (sleepButton != null)
        {
            sleepButton.onClick.AddListener(OnSleepButtonClicked);
        }
        
        // Configura sliders se existirem
        if (hungerSlider != null)
        {
            hungerSlider.minValue = 0;
            hungerSlider.maxValue = 100;
        }
        
        if (energySlider != null)
        {
            energySlider.minValue = 0;
            energySlider.maxValue = 100;
        }
    }

    private void Update()
    {
        if (petController == null) return;
        
        // Atualiza botões circulares de status
        UpdateCircularStatusButtons();
        
        // Atualiza textos (se existirem)
        UpdateTexts();
        
        // Atualiza sliders/barras (se existirem)
        UpdateSliders();
    }
    
    private void UpdateCircularStatusButtons()
    {
        if (hungerStatusButton != null)
        {
            hungerStatusButton.SetValue(petController.hunger);
        }
        
        if (energyStatusButton != null)
        {
            energyStatusButton.SetValue(petController.energy);
        }
        
        if (confidenceStatusButton != null)
        {
            confidenceStatusButton.SetValue(petController.confidence);
        }
    }

    private void UpdateTexts()
    {
        if (hungerText != null)
        {
            hungerText.text = $"Fome: {petController.hunger:F0}%";
        }
        
        if (energyText != null)
        {
            energyText.text = $"Energia: {petController.energy:F0}%";
        }
        
        if (confidenceText != null)
        {
            confidenceText.text = $"Confiança: {petController.confidence:F0}%";
        }
    }

    private void UpdateSliders()
    {
        if (hungerSlider != null)
        {
            hungerSlider.value = petController.hunger;
        }
        
        if (energySlider != null)
        {
            energySlider.value = petController.energy;
        }
    }

    private void OnFeedButtonClicked()
    {
        if (petController != null)
        {
            petController.Feed();
        }
    }

    private void OnSleepButtonClicked()
    {
        if (petController != null)
        {
            petController.Sleep();
        }
    }
}


