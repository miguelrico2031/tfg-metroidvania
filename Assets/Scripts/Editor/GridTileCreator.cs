using UnityEngine;
using UnityEditor;

public static class GridTileCreator
{
    [MenuItem("Tools/Create Grid Tile Sprite")]
    public static void CreateGridTile()
    {
        int size = 32;
        int border = 1;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Color transparent = Color.clear;
        Color line = new Color(1f, 1f, 1f, 0.3f);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                if (x >= size - border || y >= size - border)
                    tex.SetPixel(x, y, line);

        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        string path = "Assets/Sprites/GridTile.png";
        System.IO.Directory.CreateDirectory("Assets/Sprites");
        System.IO.File.WriteAllBytes(path, png);
        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        Debug.Log("Grid tile creado en " + path);
    }

    [MenuItem("Tools/Create Floor Grid Tile Sprite")]
    public static void CreateFloorGridTile()
    {
        int size = 32;
        int border = 1;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Color transparent = Color.white;
        Color line = new Color(1f, 1f, 1f, 0.3f);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                if (x >= size - border || y >= size - border)
                    tex.SetPixel(x, y, line);

        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        string path = "Assets/Sprites/FloorGridTile.png";
        System.IO.Directory.CreateDirectory("Assets/Sprites");
        System.IO.File.WriteAllBytes(path, png);
        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        Debug.Log("Grid tile creado en " + path);
    }
}