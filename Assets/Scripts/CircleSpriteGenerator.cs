using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script utilitário para gerar sprites circulares.
/// Use apenas no Editor da Unity.
/// </summary>
public class CircleSpriteGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Gerar Sprite Circular")]
    public static void GenerateCircleSprite()
    {
        int size = 512;
        Texture2D texture = new Texture2D(size, size);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        // Desenha um círculo branco com anti-aliasing
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                // Anti-aliasing na borda
                float alpha = 1f - Mathf.Clamp01((distance - radius + 2f) / 2f);
                
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        
        texture.Apply();
        
        // Salva como PNG
        byte[] bytes = texture.EncodeToPNG();
        string path = "Assets/textures/CircleSprite.png";
        
        // Cria a pasta se não existir
        if (!System.IO.Directory.Exists("Assets/textures"))
        {
            System.IO.Directory.CreateDirectory("Assets/textures");
        }
        
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();
        
        // Configura como sprite
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        
        Debug.Log($"✓ Sprite circular criado em: {path}");
        
        // Seleciona o arquivo criado
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);
    }
#endif
}


