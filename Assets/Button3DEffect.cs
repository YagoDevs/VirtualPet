using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adiciona efeito 3D visual ao botão com sombra, brilho e profundidade.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class Button3DEffect : MonoBehaviour
{
    [Header("Configuração do Efeito 3D")]
    [Tooltip("Intensidade da sombra (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float shadowIntensity = 0.5f;
    
    [Tooltip("Distância da sombra em pixels")]
    [SerializeField] private Vector2 shadowDistance = new Vector2(6f, -6f);
    
    [Tooltip("Adicionar contorno branco?")]
    [SerializeField] private bool addOutline = true;
    
    [Tooltip("Cor do contorno")]
    [SerializeField] private Color outlineColor = Color.white;
    
    [Tooltip("Distância do contorno em pixels")]
    [SerializeField] private Vector2 outlineDistance = new Vector2(3f, -3f);
    
    [Header("Brilho no Topo (Opcional)")]
    [Tooltip("Adicionar brilho no topo do botão?")]
    [SerializeField] private bool addTopShine = true;
    
    [Tooltip("Intensidade do brilho (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float shineIntensity = 0.3f;
    
    private Shadow shadowComponent;
    private Outline outlineComponent;
    private GameObject shineObject;
    
    private void Start()
    {
        ApplyEffect();
    }
    
    private void OnValidate()
    {
        // Atualiza em tempo real no Editor
        if (Application.isPlaying)
        {
            ApplyEffect();
        }
    }
    
    /// <summary>
    /// Aplica o efeito 3D ao botão
    /// </summary>
    public void ApplyEffect()
    {
        AddShadow();
        
        if (addOutline)
        {
            AddOutline();
        }
        else
        {
            RemoveOutline();
        }
        
        if (addTopShine)
        {
            AddShineEffect();
        }
        else
        {
            RemoveShine();
        }
    }
    
    private void AddShadow()
    {
        shadowComponent = GetComponent<Shadow>();
        if (shadowComponent == null)
        {
            shadowComponent = gameObject.AddComponent<Shadow>();
        }
        
        Color shadowColor = new Color(0, 0, 0, shadowIntensity);
        shadowComponent.effectColor = shadowColor;
        shadowComponent.effectDistance = shadowDistance;
        shadowComponent.useGraphicAlpha = true;
    }
    
    private void AddOutline()
    {
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent == null)
        {
            outlineComponent = gameObject.AddComponent<Outline>();
        }
        
        outlineComponent.effectColor = outlineColor;
        outlineComponent.effectDistance = outlineDistance;
        outlineComponent.useGraphicAlpha = true;
    }
    
    private void RemoveOutline()
    {
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent != null)
        {
            DestroyImmediate(outlineComponent);
        }
    }
    
    private void AddShineEffect()
    {
        // Remove o brilho existente se houver
        RemoveShine();
        
        // Cria um objeto filho para o brilho
        shineObject = new GameObject("Shine");
        shineObject.transform.SetParent(transform, false);
        
        // Configura o RectTransform
        RectTransform shineRect = shineObject.AddComponent<RectTransform>();
        shineRect.anchorMin = new Vector2(0, 0.6f);
        shineRect.anchorMax = new Vector2(1, 1);
        shineRect.offsetMin = new Vector2(10, 0);
        shineRect.offsetMax = new Vector2(-10, -10);
        
        // Adiciona Image com gradiente (simulado com cor branca semi-transparente)
        Image shineImage = shineObject.AddComponent<Image>();
        shineImage.color = new Color(1, 1, 1, shineIntensity);
        shineImage.raycastTarget = false;
        
        // Move para o início da hierarquia (renderiza primeiro, atrás do conteúdo)
        shineObject.transform.SetAsFirstSibling();
    }
    
    private void RemoveShine()
    {
        Transform existingShine = transform.Find("Shine");
        if (existingShine != null)
        {
            DestroyImmediate(existingShine.gameObject);
        }
    }
    
    /// <summary>
    /// Remove todos os efeitos
    /// </summary>
    public void RemoveAllEffects()
    {
        RemoveOutline();
        RemoveShine();
        
        if (shadowComponent != null)
        {
            DestroyImmediate(shadowComponent);
        }
    }
}










