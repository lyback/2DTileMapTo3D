using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileMapGen_Diamond : TileMapGenBase
{
    public override void Init(TerrainInfo terrainInfo, int v_count, int h_count, int itemEx_x, int itemEx_y, float offset_x, float offset_y)
    {
        m_terrainInfoAsset = terrainInfo;
        m_offset = new Vector2(offset_x, offset_y);
        m_visibleObj = new VisiableObj[h_count + itemEx_y, v_count + itemEx_x];
        m_needVisibleTer = new VisiableTerEx[h_count + itemEx_y, v_count + itemEx_x];
        m_needVisibleItem = new VisiableItemEx[h_count + itemEx_y, v_count + itemEx_x];

        m_itemSizeEx_x = itemEx_x;
        m_itemSizeEx_x_Half = itemEx_x / 2;
        m_itemSizeEx_y = itemEx_y;
        m_itemSizeEx_y_Half = itemEx_y / 2;
        m_itemSizeEx_y_Quarter = itemEx_y / 4;

        m_visiableCount = v_count * h_count;
        m_visiablehalfCount = m_visiableCount / 2;

        TerRoot.localPosition = new Vector3(offset_x, 0, offset_y);
        ItemRoot.localPosition = new Vector3(offset_x, 0, offset_y);


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
    }
    public override void MoveTo(float x, float z)
    {
        int start_x = Mathf.FloorToInt(x);
        int start_z = Mathf.FloorToInt(z);
        if (start_x == m_lastPos_x && start_z == m_lastPos_z)
        {
            return;
        }
        m_lastPos_x = start_x;
        m_lastPos_z = start_z;

        int h_count = m_visibleObj.GetLength(0);
        int v_count = m_visibleObj.GetLength(1);
        bool b = true;
        int v_center_x = start_x;
        int v_center_z = start_z;
        for (int i = 0; i < h_count; i++)
        {
            int dx = b ? 1 : 0;
            int dz = !b ? 1 : 0;
            v_center_x = v_center_x + dx;
            v_center_z = v_center_z + dz;
            for (int j = 0; j < v_count; j++)
            {
                int _pos_x = m_map_W - (v_center_x - j);
                int _pos_z = m_map_H - (v_center_z + j);
                int temp = _pos_x;
                _pos_x = _pos_z;
                _pos_z = temp;
                if (i < h_count - m_itemSizeEx_y && j < v_count - m_itemSizeEx_x)
                {
                    CheckVisibleTer(_pos_x, _pos_z, i, j);
                }
                else
                {
                    m_needVisibleTer[i, j].isShow = false;
                    SetVisibleTerNUll(i, j);
                }
                CheckVisibleItem(_pos_x + m_itemSizeEx_x_Half + m_itemSizeEx_y_Quarter, _pos_z - m_itemSizeEx_x_Half + m_itemSizeEx_y_Quarter, i, j);
            }
            b = !b;
        }
        b = true;
        v_center_x = 0;
        v_center_z = 0;
        for (int i = 0; i < h_count; i++)
        {
            int dx = b ? 1 : 0;
            int dz = !b ? 1 : 0;
            v_center_x = v_center_x + dx;
            v_center_z = v_center_z + dz;
            for (int j = 0; j < v_count; j++)
            {
                int _pos_x = v_center_x - j;
                int _pos_z = v_center_z + j;
                if (m_needVisibleTer[i, j].isShow)
                {
                    SetVisibleTer(i, j, _pos_x, _pos_z);
                }
                if (m_needVisibleItem[i, j].isShow)
                {
                    SetVisibleItem(i, j, _pos_x + m_itemSizeEx_x_Half - m_itemSizeEx_y_Quarter, _pos_z - m_itemSizeEx_x_Half - m_itemSizeEx_y_Quarter);
                }
            }
            b = !b;
        }
        pos_temp.x = start_x;
        pos_temp.z = start_z;
        Root.position = pos_temp;
    }
}
