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
        base.Init(terrainInfo, v_count, h_count, itemEx_x, itemEx_y, offset_x, offset_y);
    }
    public override void MoveTo(float x, float z)
    {
        int start_x = Mathf.FloorToInt(x);
        int start_z = Mathf.FloorToInt(z);
        if (!m_forceMoveTo && (start_x == m_lastPos_x && start_z == m_lastPos_z))
        {
            return;
        }
        m_lastPos_x = start_x;
        m_lastPos_z = start_z;
        m_forceMoveTo = false;

        int h_count = m_visiableCount_h;
        int v_count = m_visiableCount_v;
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
                CheckVisibleAlphaTex(_pos_x + m_itemSizeEx_x_Half + m_itemSizeEx_y_Quarter, _pos_z - m_itemSizeEx_x_Half + m_itemSizeEx_y_Quarter, i, j);
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
                if (m_needVisibleAlphaTex[i, j].isShow)
                {
                    SetVisibleAlphaTex(i, j, _pos_x + m_itemSizeEx_x_Half - m_itemSizeEx_y_Quarter, _pos_z - m_itemSizeEx_x_Half - m_itemSizeEx_y_Quarter);
                }
            }
            b = !b;
        }
        pos_temp.x = start_x;
        pos_temp.y = 0f;
        pos_temp.z = start_z;
        Root.position = pos_temp;
    }
    public override void HideItemAtPos(int x, int z)
    {
        x = m_map_W - x;
        z = m_map_H - z;
        int temp = x;
        x = z;
        z = temp;
        int id = x * 10000 + z;
        // Debug.Log("TileMap:HideItemAtPos:" + x + "," + z);
        if (!m_hideItem.Contains(id))
        {
            m_hideItem.Add(id);
        }
    }
    public override void ShowItemAtPos(int x, int z)
    {
        x = m_map_W - x;
        z = m_map_H - z;
        int temp = x;
        x = z;
        z = temp;
        // Debug.Log("TileMap:ShowItemAtPos:" + x + "," + z);
        int id = x * 10000 + z;
        if (m_hideItem.Contains(id))
        {
            m_hideItem.Remove(id);
        }
    }
    // protected override void GetItemPosByIndex(int i, int j, out int x, out int z){
    //     int v_center_x = (i)/2 + 1;
    //     int v_center_z = (i+1)/2;
    //     x = v_center_x - j + m_itemSizeEx_x_Half - m_itemSizeEx_y_Quarter;
    //     z = v_center_z + j - m_itemSizeEx_x_Half - m_itemSizeEx_y_Quarter;
    // }
}
