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
        base.Init(terrainInfo, v_count, h_count, itemEx_x, itemEx_y, offset_x, offset_y);
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
        
        int visiable_x_count = m_visiableCount_h;
        int visiable_z_count = m_visiableCount_v;
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
