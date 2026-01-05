using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script utilitário para configurar iluminação automaticamente.
/// Use no Editor para aplicar configurações pré-definidas.
/// </summary>
public class LightingSetupHelper
{
#if UNITY_EDITOR
    [MenuItem("Tools/Iluminação/Aplicar Setup Básico")]
    public static void ApplyBasicLightingSetup()
    {
        // Procura pela Directional Light principal
        Light mainLight = FindMainDirectionalLight();
        
        if (mainLight != null)
        {
            ConfigureMainLight(mainLight);
            Debug.Log("✓ Directional Light configurada!");
        }
        else
        {
            Debug.LogWarning("⚠️ Nenhuma Directional Light encontrada. Crie uma: GameObject → Light → Directional Light");
        }
        
        // Configura ambiente
        ConfigureEnvironment();
        Debug.Log("✓ Ambiente configurado!");
        
        // Configura câmera para post-processing
        ConfigureCamera();
        Debug.Log("✓ Câmera configurada!");
        
        Debug.Log("🎉 Setup de iluminação básico aplicado! Agora adicione Post-Processing no Volume Profile.");
    }
    
    [MenuItem("Tools/Iluminação/Adicionar Fill Light")]
    public static void AddFillLight()
    {
        GameObject fillLightObj = new GameObject("Fill Light");
        Light fillLight = fillLightObj.AddComponent<Light>();
        
        fillLight.type = LightType.Directional;
        fillLight.color = new Color(0.68f, 0.85f, 0.9f); // Azul suave
        fillLight.intensity = 0.3f;
        fillLight.shadows = LightShadows.None;
        
        fillLightObj.transform.rotation = Quaternion.Euler(-20f, 150f, 0f);
        
        Debug.Log("✓ Fill Light adicionada! Ajuste a posição conforme necessário.");
        Selection.activeGameObject = fillLightObj;
    }
    
    [MenuItem("Tools/Iluminação/Adicionar Luz no Pet")]
    public static void AddPetSpotlight()
    {
        GameObject spotlightObj = new GameObject("Pet Spotlight");
        Light spotlight = spotlightObj.AddComponent<Light>();
        
        spotlight.type = LightType.Point;
        spotlight.color = new Color(1f, 0.96f, 0.84f); // Branco quente
        spotlight.intensity = 2f;
        spotlight.range = 10f;
        spotlight.shadows = LightShadows.None;
        spotlight.renderMode = LightRenderMode.ForcePixel;
        
        spotlightObj.transform.position = new Vector3(0, 5, 3);
        
        Debug.Log("✓ Luz do Pet adicionada! Posicione perto do seu pet.");
        Selection.activeGameObject = spotlightObj;
    }
    
    private static Light FindMainDirectionalLight()
    {
        Light[] lights = Object.FindObjectsOfType<Light>();
        
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional && light.gameObject.name.Contains("Directional"))
            {
                return light;
            }
        }
        
        // Se não encontrou, retorna a primeira Directional
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                return light;
            }
        }
        
        return null;
    }
    
    private static void ConfigureMainLight(Light light)
    {
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.96f, 0.84f); // Amarelo suave
        light.intensity = 1.5f;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.7f;
        light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.High;
        
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }
    
    private static void ConfigureEnvironment()
    {
        // Configura iluminação ambiente
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.53f, 0.81f, 0.92f); // Azul claro
        RenderSettings.ambientEquatorColor = new Color(1f, 0.78f, 0.59f); // Laranja suave
        RenderSettings.ambientGroundColor = new Color(0.31f, 0.39f, 0.24f); // Verde escuro
        RenderSettings.ambientIntensity = 1.2f;
        
        // Reflexões do ambiente
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
        RenderSettings.reflectionIntensity = 1f;
        RenderSettings.reflectionBounces = 1;
        
        Debug.Log("✓ Ambiente configurado com cores!");
    }
    
    private static void ConfigureCamera()
    {
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogWarning("⚠️ Main Camera não encontrada!");
            return;
        }
        
        // Procura pelo Universal Additional Camera Data
        var cameraData = mainCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        
        if (cameraData != null)
        {
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraData.antialiasingQuality = UnityEngine.Rendering.Universal.AntialiasingQuality.High;
            
            Debug.Log("✓ Post-Processing ativado na câmera!");
        }
        else
        {
            Debug.LogWarning("⚠️ Camera não tem Universal Additional Camera Data. Certifique-se de estar usando URP.");
        }
    }
#endif
}

