using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestMove_Diamond : MonoBehaviour
{
    public int moveSpeed = 1;
    public TileMapGenBase gen;
    public int size_W = 100;
    public int size_H = 100;
    void Start()
    {
#if UNITY_EDITOR
        var terrainInfo = AssetDatabase.LoadAssetAtPath<TerrainInfo>(string.Format("Assets/TileMap/TerrainInfo.asset"));
#else
        var terrainInfo = new TerrainInfo();
#endif
        string tilemapInfoPath = "Assets/TileMap/MapInfo";
        string tilemapInfoName = "MapInfo_{0}_{1}";
        string itemInfoPath = "Assets/TileMap/ItemInfo";
        string itemInfoName = "ItemInfo_{0}_{1}";
        string alphaTexInfoPath = "Assets/TileMap/TerrainAlpha";
        string alphaTexInfoName = "TerrainAlpha_{0}_{1}";
        string tilemapObjPath = "Assets/TileMap/TileMapObj/Prefab/Terrain";
        string tilemapObjName = "Terrain@{0}";
        string itemObjPath = "Assets/TileMap/TileMapObj/Prefab/Item";
        string alphaTexObjPath = "Assets/TileMap/TileMapObj/Prefab/TerrainAlpha";
        gen.Init(terrainInfo, size_W, size_H, 8, 8, 2.7f, -2.9f);
        gen.SetResPath(false, tilemapInfoPath, tilemapInfoName, itemInfoPath, itemInfoName, alphaTexInfoPath, alphaTexInfoName, tilemapObjPath, tilemapObjName, itemObjPath, alphaTexObjPath);
    }
    void Update()
    {
        gen.MoveTo(transform.position.x, transform.position.z);
    }
    private void FixedUpdate()
    {
#if UNITY_EDITOR
        Vector3 dirVector3 = transform.position;
        if (Input.GetKey(KeyCode.W))
        {
            dirVector3.z += 0.01f * moveSpeed;
            dirVector3.x += 0.01f * moveSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            dirVector3.z -= 0.01f * moveSpeed;
            dirVector3.x -= 0.01f * moveSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            dirVector3.z += 0.01f * moveSpeed;
            dirVector3.x -= 0.01f * moveSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            dirVector3.z -= 0.01f * moveSpeed;
            dirVector3.x += 0.01f * moveSpeed;
        }
        transform.position = dirVector3;
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 pos = touch.position;
                transform.position = pos;
            }
        }
#endif

    }
}