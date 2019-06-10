using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestMove : MonoBehaviour
{
    public TileMapGen gen;
    public Vector2Int size = new Vector2Int(5, 5);
    void Start()
    {
        var terrainInfo = AssetDatabase.LoadAssetAtPath<TerrainInfo>(string.Format("Assets/Map/TerrainInfo.asset"));
        gen.Init(terrainInfo, terrainInfo.MapSize.x, terrainInfo.MapSize.y, terrainInfo.SpiltMapSize.x, terrainInfo.SpiltMapSize.y, size, new Vector2(-size.x / 2f, -size.y / 2f));
    }
    void Update()
    {
        gen.MoveTo(transform.position.x, transform.position.z);
    }
}