using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEditor;

public class SavePrefab
{
    static string terPrefabPath = "Assets/TileMap/TileMapObj/Prefab/Terrain/";
    static string itemPrefabPath = "Assets/TileMap/TileMapObj/Prefab/Item/";
    static string alphaPrefabPath = "Assets/TileMap/TileMapObj/Prefab/TerrainAlpha/";
    [MenuItem("Assets/Map/CreatePrefab")]
    public static void CreatePrefab()
    {

        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            var obj = Selection.gameObjects[i];
            PrefabUtility.CreatePrefab(string.Format("{0}{1}.prefab", terPrefabPath, obj.name), obj);
        }
    }
    [MenuItem("Assets/Map/CreateItemPrefab_Item")]
    public static void CreateItemPrefab_Item()
    {
        Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < objs.Length; i++)
        {
            var obj = objs[i] as Texture2D;
            string assetPath = AssetDatabase.GetAssetPath(obj);
            TextureImporter texImp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            texImp.spritePackingTag = "TileMap_Item";
            texImp.spritePixelsPerUnit = 256;
            TextureImporterSettings settings = new TextureImporterSettings();
            texImp.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            texImp.SetTextureSettings(settings);
            if (obj != null)
            {
                string prefaPath = string.Format("{0}{1}.prefab", itemPrefabPath, obj.name);
                var prefab = AssetDatabase.LoadAssetAtPath<Object>(prefaPath);
                if (prefab == null)
                {
                    var item = CreateItemPrefab(obj, 2);
					
                    var spr = item.GetComponent<SpriteRenderer>();
                    var spHeight = spr.sprite.texture.height;
                    var ppu = spr.sprite.pixelsPerUnit;
					var h = spHeight / ppu;

                    item.transform.localPosition = new Vector3(0f, h/2f/Mathf.Sqrt(2), 0f);
                    item.transform.localEulerAngles = new Vector3(45, 45, 0);
                    PrefabUtility.CreatePrefab(prefaPath, item);
                    GameObject.DestroyImmediate(item);
                }
            }
        }
    }
    [MenuItem("Assets/Map/CreateItemPrefab_Item_Shan")]
    public static void CreateItemPrefab_Item_Shan()
    {
        Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < objs.Length; i++)
        {
            
            var obj = objs[i] as Texture2D;
            string assetPath = AssetDatabase.GetAssetPath(obj);
            TextureImporter texImp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            texImp.spritePackingTag = "TileMap_Item";
            texImp.spritePixelsPerUnit = 256;
            TextureImporterSettings settings = new TextureImporterSettings();
            texImp.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.TopRight;
            texImp.SetTextureSettings(settings);
            if (obj != null)
            {
                string prefaPath = string.Format("{0}{1}.prefab", itemPrefabPath, obj.name);
                var prefab = AssetDatabase.LoadAssetAtPath<Object>(prefaPath);
                if (prefab == null)
                {
                    var item = CreateItemPrefab(obj, 1);
                    var parent = new GameObject(obj.name);
                    item.transform.SetParent(parent.transform);
                    var spr = item.GetComponent<SpriteRenderer>();
                    var spHeight = spr.sprite.texture.height;
                    var ppu = spr.sprite.pixelsPerUnit;
					var h = spHeight / ppu;
                    item.transform.localPosition = new Vector3(0f, h / Mathf.Sqrt(2f), -(h*Mathf.Sqrt(2f) - h / Mathf.Sqrt(2f))*Mathf.Sqrt(2f));
                    item.transform.localEulerAngles = new Vector3(45, 45, 0);
                    PrefabUtility.CreatePrefab(prefaPath, parent);
                    GameObject.DestroyImmediate(parent);
                }
            }
        }
    }
    [MenuItem("Assets/Map/CreateItemPrefab_Item_TextureAlpha")]
    public static void CreateItemPrefab_Item_TextureAlpha()
    {
        Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < objs.Length; i++)
        {
            var obj = objs[i] as Texture2D;
            string assetPath = AssetDatabase.GetAssetPath(obj);
            TextureImporter texImp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            texImp.spritePackingTag = "TileMap_Item";
            texImp.spritePixelsPerUnit = 256;
            TextureImporterSettings settings = new TextureImporterSettings();
            texImp.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomLeft;
            texImp.SetTextureSettings(settings);
            if (obj != null)
            {
                string prefaPath = string.Format("{0}{1}.prefab", alphaPrefabPath, obj.name);
                var prefab = AssetDatabase.LoadAssetAtPath<Object>(prefaPath);
                if (prefab == null)
                {
                    var item = CreateItemPrefab(obj, 0);
                    item.transform.localPosition = new Vector3(0f, 0.01f, 0f);
					item.transform.localScale = new Vector3(1f, Mathf.Sqrt(2), 1f);
                    item.transform.localEulerAngles = new Vector3(90, 45, 0);
                    PrefabUtility.CreatePrefab(prefaPath, item);
                    GameObject.DestroyImmediate(item);
                }
            }
        }
    }
    static GameObject CreateItemPrefab(Object obj, int order)
    {
        string assetPath = AssetDatabase.GetAssetPath(obj);
        Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        var item = new GameObject("sprite");
        var sp = item.AddComponent<SpriteRenderer>();
        sp.sprite = spr;
        sp.sortingOrder = order;

        return item;
    }

}
