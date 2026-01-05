using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Botão circular que mostra status com preenchimento colorido (0-100%).
/// A cor muda gradualmente entre duas cores baseado no valor.
/// Exemplo: 100% = cor1 (verde), 0% = cor2 (vermelho)
/// </summary>
public class CircularStatusButton : MonoBehaviour
{
    [Header("Referências UI")]
    [Tooltip("Imagem que será preenchida (Fill Image)")]
    [SerializeField] private Image fillImage;
    
    [Tooltip("Ícone central (opcional) - pode deixar vazio")]
    [SerializeField] private Image iconImage;
    
    [Tooltip("Texto para mostrar a porcentagem (opcional)")]
    [SerializeField] private TextMeshProUGUI percentageText;
    
    [Header("Configuração de Cores")]
    [Tooltip("Cor quando o valor está em 100% (cheio)")]
    [SerializeField] private Color fullColor = Color.yellow;
    
    [Tooltip("Cor quando o valor está em 0% (vazio)")]
    [SerializeField] private Color emptyColor = Color.red;
    
    [Header("Configuração de Preenchimento")]
    [Tooltip("Valor atual (0-100)")]
    [SerializeField] [Range(0, 100)] private float currentValue = 100f;
    
    [Tooltip("Tipo de preenchimento")]
    [SerializeField] private FillType fillType = FillType.VerticalBottomToTop;
    
    public enum FillType
    {
        VerticalBottomToTop,    // De baixo para cima
        VerticalTopToBottom,    // De cima para baixo
        HorizontalLeftToRight,  // Da esquerda para direita
        HorizontalRightToLeft,  // Da direita para esquerda
        Radial360               // Circular (como relógio)
    }
    
    private void Start()
    {
        // Configura o tipo de preenchimento da imagem
        ConfigureFillType();
        
        // Atualiza visual inicial
        UpdateVisual();
    }
    
    private void OnValidate()
    {
        // Atualiza no Editor quando valores mudam
        if (fillImage != null)
        {
            ConfigureFillType();
            UpdateVisual();
        }
    }
    
    /// <summary>
    /// Configura o tipo de preenchimento da Image baseado no FillType selecionado
    /// </summary>
    private void ConfigureFillType()
    {
        if (fillImage == null) return;
        
        fillImage.type = Image.Type.Filled;
        
        switch (fillType)
        {
            case FillType.VerticalBottomToTop:
                fillImage.fillMethod = Image.FillMethod.Vertical;
                fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
                break;
                
            case FillType.VerticalTopToBottom:
                fillImage.fillMethod = Image.FillMethod.Vertical;
                fillImage.fillOrigin = (int)Image.OriginVertical.Top;
                break;
                
            case FillType.HorizontalLeftToRight:
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                break;
                
            case FillType.HorizontalRightToLeft:
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Right;
                break;
                
            case FillType.Radial360:
                fillImage.fillMethod = Image.FillMethod.Radial360;
                fillImage.fillOrigin = (int)Image.Origin360.Bottom;
                fillImage.fillClockwise = true;
                break;
        }
    }
    
    /// <summary>
    /// Atualiza o valor do status (0-100)
    /// </summary>
    public void SetValue(float newValue)
    {
        currentValue = Mathf.Clamp(newValue, 0f, 100f);
        UpdateVisual();
    }
    
    /// <summary>
    /// Obtém o valor atual
    /// </summary>
    public float GetValue()
    {
        return currentValue;
    }
    
    /// <summary>
    /// Define a cor para quando está cheio (100%)
    /// </summary>
    public void SetFullColor(Color color)
    {
        fullColor = color;
        UpdateVisual();
    }
    
    /// <summary>
    /// Define a cor para quando está vazio (0%)
    /// </summary>
    public void SetEmptyColor(Color color)
    {
        emptyColor = color;
        UpdateVisual();
    }
    
    /// <summary>
    /// Define o sprite do ícone central
    /// </summary>
    public void SetIcon(Sprite icon)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }
    }
    
    /// <summary>
    /// Atualiza o visual do botão (fillAmount e cor)
    /// </summary>
    private void UpdateVisual()
    {
        if (fillImage == null) return;
        
        // Calcula a porcentagem (0.0 a 1.0)
        float fillPercentage = currentValue / 100f;
        
        // Atualiza o preenchimento
        fillImage.fillAmount = fillPercentage;
        
        // Interpola a cor entre emptyColor e fullColor
        // Quando valor = 0, usa emptyColor
        // Quando valor = 100, usa fullColor
        fillImage.color = Color.Lerp(emptyColor, fullColor, fillPercentage);
        
        // Atualiza o texto de porcentagem (se existir)
        UpdatePercentageText();
    }
    
    /// <summary>
    /// Atualiza o texto de porcentagem
    /// </summary>
    private void UpdatePercentageText()
    {
        if (percentageText != null)
        {
            percentageText.text = $"{currentValue:F0}%";
        }
    }
    
    /// <summary>
    /// Define o componente de texto para a porcentagem
    /// </summary>
    public void SetPercentageText(TextMeshProUGUI text)
    {
        percentageText = text;
        UpdatePercentageText();
    }
}

