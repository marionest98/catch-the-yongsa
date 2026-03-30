using UnityEditor;
using UnityEngine;

public class RobinImportFixer : EditorWindow
{
    [MenuItem("Tools/Fix Robin Sprites")]
    public static void FixRobinSprites()
    {
        // Robin 폴더 안 모든 텍스처를 찾아서 Sprite로 변경
        string[] guids = AssetDatabase.FindAssets(
            "t:Texture2D", new[] { "Assets/Sprites/Robin" });

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null) { continue; }

            // Sprite로 설정, Bilinear 필터
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
            count++;
        }

        Debug.Log("Robin 스프라이트 설정 완료: " + count + "개");
    }
}