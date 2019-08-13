using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileMapGen_Square : TileMapGenBase
{
    public override void Init(TerrainInfo terrainInfo, int v_count, int h_count, int itemEx_x, int itemEx_y, float offset_x, float offset_y)
    {
        m_terrainInfoAsset = terrainInfo;
        m_offset = new Vector2(offset_x + itemEx_x, offset_y + itemEx_y);
        m_visibleObj = new VisiableObj[v_count + itemEx_x, h_count + itemEx_y];
        m_needVisibleTer = new VisiableTerEx[v_count + itemEx_x, h_count + itemEx_y];
        m_needVisibleItem = new VisiableItemEx[v_count + itemEx_x, h_count + itemEx_y];

        m_itemSizeEx_x = itemEx_x;
        m_itemSizeEx_x_Half = itemEx_x / 2;
        m_itemSizeEx_y = itemEx_y;
        m_itemSizeEx_y_Half = itemEx_y / 2;
        m_itemSizeEx_y_Quarter = itemEx_y / 4;

        m_visiableCount = v_count * h_count;
        m_visiablehalfCount = m_visiableCount / 2;

        TerRoot.localPosition = new Vector3(-offset_x, 0, -offset_y);
        ItemRoot.localPosition = new Vector3(-offset_x, 0, -offset_y);

        m_map_W = terrainInfo.MapSize_W;
        m_map_H = terrainInfo.MapSize_H;
        m_splitMap_X = terrainInfo.SpiltMap_X;
        m_splitMap_Y = terrainInfo.SpiltMap_Y;

        m_tilemap_W = terrainInfo.SpiltMapSize_W;
        m_tilemap_H = terrainInfo.SpiltMapSize_H;
        int x = Mathf.CeilToInt(m_map_W * 1f / m_tilemap_W);
        int y = Mathf.CeilToInt(m_map_H * 1f / m_tilemap_H);
        m_tilemapInfo = new TileMapInfo[x, y];
        m_itemmapInfo = new ItemMapInfo[x, y];
        m_tilemapInfoIsInit = new bool[x, y];
        m_itemmapInfoIsInit = new bool[x, y];
        m_alphaTexInfoIsInit = new bool[x, y];
    }
    public override void MoveTo(float x, float z)
    {
        int start_x = Mathf.FloorToInt(x);
        int start_z = Mathf.FloorToInt(z);
        // if (start_x == m_lastPos_x && start_z == m_lastPos_z)
        // {
        //     return;
        // }
        m_lastPos_x = start_x;
        m_lastPos_z = start_z;
        int visiable_x_count = m_visibleObj.GetLength(0);
        int visiable_z_count = m_visibleObj.GetLength(1);
        for (int i = 0; i < visiable_x_count; i++)
        {
            for (int j = 0; j < visiable_z_count; j++)
            {
                int _pos_x = m_map_W - (start_x + i);
                int _pos_z = m_map_H - (start_z + j);
                int temp = _pos_x;
                _pos_x = _pos_z;
                _pos_z = temp;
                if (i < visiable_x_count - m_itemSizeEx_x && j < visiable_z_count - m_itemSizeEx_y)
                {
                    CheckVisibleTer(_pos_x, _pos_z, i, j);
                }
                else
                {
                    m_needVisibleTer[i, j].isShow = false;
                    SetVisibleTerNUll(i, j);
                }
                CheckVisibleItem(_pos_x + m_itemSizeEx_x_Half, _pos_z + m_itemSizeEx_y_Half, i, j);
            }
        }
        for (int i = 0; i < visiable_x_count; i++)
        {
            for (int j = 0; j < visiable_z_count; j++)
            {
                if (m_needVisibleTer[i, j].isShow)
                {
                    SetVisibleTer(i, j, i, j);
                }
                if (m_needVisibleItem[i, j].isShow)
                {
                    SetVisibleItem(i, j, i - m_itemSizeEx_x_Half, j - m_itemSizeEx_y_Half);
                }
            }
        }
        pos_temp.x = start_x;
        pos_temp.y = 0f;
        pos_temp.z = start_z;
        Root.position = pos_temp;
    }

}
