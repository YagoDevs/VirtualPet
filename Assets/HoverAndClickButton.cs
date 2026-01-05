using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Adiciona efeito hover (levantar botão) e executa ações ao clicar.
/// Adicione este componente aos botões circulares de status.
/// </summary>
[RequireComponent(typeof(Button))]
public class HoverAndClickButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Configuração da Ação")]
    [Tooltip("Tipo de ação ao clicar")]
    [SerializeField] private ActionType actionType = ActionType.Feed;
    
    [Tooltip("Referência ao PetController")]
    [SerializeField] private PetControllerSimple petController;
    
    [Header("Efeito Hover")]
    [Tooltip("Distância que o botão sobe no hover (em unidades UI)")]
    [SerializeField] private float hoverLiftAmount = 10f;
    
    [Tooltip("Velocidade da animação do hover")]
    [SerializeField] private float hoverSpeed = 10f;
    
    [Tooltip("Escala do botão no hover (1.1 = 10% maior)")]
    [SerializeField] private float hoverScale = 1.1f;
    
    [Header("Efeito Click")]
    [Tooltip("Escala durante o click")]
    [SerializeField] private float clickScale = 0.9f;
    
    [Tooltip("Duração da animação de click")]
    [SerializeField] private float clickDuration = 0.1f;
    
    [Header("Feedback Visual")]
    [Tooltip("Cor do botão no hover (opcional)")]
    [SerializeField] private bool changeColorOnHover = false;
    
    [SerializeField] private Color hoverColor = Color.white;
    
    [Header("Sons")]
    [Tooltip("Som ao passar o mouse (hover)")]
    [SerializeField] private AudioClip hoverSound;
    
    [Tooltip("Som ao clicar")]
    [SerializeField] private AudioClip clickSound;
    
    [Tooltip("Volume dos sons (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 0.5f;
    
    public enum ActionType
    {
        Feed,    // Alimentar (restaura fome)
        Sleep    // Dormir (restaura energia)
    }
    
    private Button button;
    private RectTransform rectTransform;
    private Image buttonImage;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Vector3 targetPosition;
    private Vector3 targetScale;
    private Color originalColor;
    private bool isHovering = false;
    private bool isAnimating = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();
        
        originalPosition = rectTransform.localPosition;
        originalScale = rectTransform.localScale;
        targetPosition = originalPosition;
        targetScale = originalScale;
        
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        
        // Busca o PetController se não foi atribuído
        if (petController == null)
        {
            petController = FindObjectOfType<PetControllerSimple>();
            if (petController == null)
            {
                Debug.LogWarning($"HoverAndClickButton em {gameObject.name}: PetController não encontrado!");
            }
        }
    }

    private void Update()
    {
        // Anima suavemente para a posição e escala alvo
        rectTransform.localPosition = Vector3.Lerp(
            rectTransform.localPosition, 
            targetPosition, 
            hoverSpeed * Time.deltaTime
        );
        
        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale, 
            targetScale, 
            hoverSpeed * Time.deltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        
        // Define a posição alvo (sobe o botão)
        targetPosition = originalPosition + new Vector3(0, hoverLiftAmount, 0);
        
        // Define a escala alvo (aumenta ligeiramente)
        targetScale = originalScale * hoverScale;
        
        // Muda a cor se configurado
        if (changeColorOnHover && buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }
        
        // Toca som de hover
        PlaySound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        
        // Volta para a posição original
        targetPosition = originalPosition;
        targetScale = originalScale;
        
        // Restaura a cor original
        if (changeColorOnHover && buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (petController == null)
        {
            Debug.LogWarning($"Botão clicado mas PetController não está atribuído em {gameObject.name}!");
            return;
        }
        
        // Toca som de click
        PlaySound(clickSound);
        
        // Executa a ação
        ExecuteAction();
        
        // Animação de click
        if (!isAnimating)
        {
            StartCoroutine(AnimateClick());
        }
    }

    private void ExecuteAction()
    {
        switch (actionType)
        {
            case ActionType.Feed:
                petController.Feed();
                Debug.Log("🍖 Pet alimentado!");
                break;
                
            case ActionType.Sleep:
                petController.Sleep();
                Debug.Log("💤 Pet dormindo!");
                break;
        }
    }

    private System.Collections.IEnumerator AnimateClick()
    {
        isAnimating = true;
        
        Vector3 clickedScale = originalScale * clickScale;
        
        // Diminui (scale down)
        float elapsed = 0f;
        while (elapsed < clickDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / clickDuration;
            
            // Se ainda estiver em hover, usa a escala de hover como base
            Vector3 currentTargetScale = isHovering ? originalScale * hoverScale : originalScale;
            rectTransform.localScale = Vector3.Lerp(currentTargetScale, clickedScale, t);
            
            yield return null;
        }
        
        // Volta ao normal (ou ao hover se ainda estiver com mouse em cima)
        elapsed = 0f;
        while (elapsed < clickDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / clickDuration;
            
            Vector3 finalScale = isHovering ? originalScale * hoverScale : originalScale;
            rectTransform.localScale = Vector3.Lerp(clickedScale, finalScale, t);
            
            yield return null;
        }
        
        // Define a escala final correta
        targetScale = isHovering ? originalScale * hoverScale : originalScale;
        
        isAnimating = false;
    }

    /// <summary>
    /// Configura o tipo de ação via script
    /// </summary>
    public void SetActionType(ActionType type)
    {
        actionType = type;
    }

    /// <summary>
    /// Configura a referência ao PetController via script
    /// </summary>
    public void SetPetController(PetControllerSimple controller)
    {
        petController = controller;
    }
    
    /// <summary>
    /// Toca um som se ele estiver configurado
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            // Toca o som na posição da câmera para que seja audível
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, soundVolume);
        }
    }
}

