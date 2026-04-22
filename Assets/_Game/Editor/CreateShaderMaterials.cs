using UnityEditor;
using UnityEngine;
using System.IO;

public static class CreateShaderMaterials
{
    public static void Create()
    {
        Debug.Log("[CreateShaderMaterials] Creating dummy materials to force shader inclusion...");
        string resourcesFolder = "Assets/Resources";
        if (!Directory.Exists(resourcesFolder))
        {
            Directory.CreateDirectory(resourcesFolder);
        }

        CreateDummyMaterial(resourcesFolder, "IncludedShader_URPLit", "Universal Render Pipeline/Lit");
        CreateDummyMaterial(resourcesFolder, "IncludedShader_URPUnlit", "Universal Render Pipeline/Unlit");
        CreateDummyMaterial(resourcesFolder, "IncludedShader_UnlitTransparent", "Unlit/Transparent");
        CreateDummyMaterial(resourcesFolder, "IncludedShader_UnlitColor", "Unlit/Color");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateShaderMaterials] Done.");
    }

    private static void CreateDummyMaterial(string folder, string name, string shaderName)
    {
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debug.LogWarning($"[CreateShaderMaterials] Could not find shader: {shaderName}");
            return;
        }

        string path = $"{folder}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
            Debug.Log($"[CreateShaderMaterials] Created {path}");
        }
        else
        {
            mat.shader = shader;
            EditorUtility.SetDirty(mat);
            Debug.Log($"[CreateShaderMaterials] Updated {path}");
        }
    }
}
