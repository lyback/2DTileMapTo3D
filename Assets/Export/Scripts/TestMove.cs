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
    public Vector2Int size = new Vector2Int(5, 5);
    void Start()
    {
        var terrainInfo = AssetDatabase.LoadAssetAtPath<TerrainInfo>(string.Format("Assets/TileMap/TerrainInfo.asset"));
        gen.Init(terrainInfo, terrainInfo.MapSize.x, terrainInfo.MapSize.y, terrainInfo.SpiltMapSize.x, terrainInfo.SpiltMapSize.y, size, new Vector2(-size.x / 2f, -size.y / 2f));
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