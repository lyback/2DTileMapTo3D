using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestMove : MonoBehaviour
{
    public int moveSpeed = 1;
    public TileMapGen gen;
    public int size_W = 5;
    public int size_H = 5;
    void Start()
    {
        var terrainInfo = AssetDatabase.LoadAssetAtPath<TerrainInfo>(string.Format("Assets/TileMap/TerrainInfo.asset"));
        string tilemapInfoPath = "Assets/TileMap/MapInfo/MapInfo_{0}_{1}.asset";
        string itemInfoPath = "Assets/TileMap/ItemInfo/ItemInfo_{0}_{1}.asset";
        string tilemapObjPath = "Assets/TileMap/TileMapObj/Prefab/Terrain/Terrain@{0}.prefab";
        string itemObjPath = "Assets/TileMap/TileMapObj/Prefab/item/{0}.prefab";
        gen.Init(terrainInfo, size_W, size_H , -size_W/2, -size_H/2, tilemapInfoPath,itemInfoPath,tilemapObjPath,itemObjPath);
        // gen.Init(terrainInfo, terrainInfo.MapSize.x, terrainInfo.MapSize.y, terrainInfo.SpiltMapSize.x, terrainInfo.SpiltMapSize.y, size, Vector2.zero);
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
            dirVector3.z += 0.01f*moveSpeed;
            dirVector3.x += 0.01f*moveSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            dirVector3.z -= 0.01f*moveSpeed;
            dirVector3.x -= 0.01f*moveSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            dirVector3.z += 0.01f*moveSpeed;
            dirVector3.x -= 0.01f*moveSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            dirVector3.z -= 0.01f*moveSpeed;
            dirVector3.x += 0.01f*moveSpeed;
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