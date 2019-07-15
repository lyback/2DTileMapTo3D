using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileMapGen : MonoBehaviour
{
    public Transform Root;                      // 场景根节点
    TerrainInfo m_terrainInfoAsset;
    TileMapInfo[,] m_tilemapInfo;
    bool[,] m_tilemapInfoIsInit;
    ItemMapInfo[,] m_itemmapInfo;
    bool[,] m_itemmapInfoIsInit;
    int m_tilemap_W;
    int m_tilemap_H;
    int m_map_W;
    int m_map_H;
    int m_splitMap_X;
    int m_splitMap_Y;
    Vector2 m_offset;

    VisiableObj[,] m_visibleObj;    
    VisiableTerEx[,] m_needVisibleTer;
    VisiableItemEx[,] m_needVisibleItem;
    int m_visiableCount = 0;
    int m_visiablehalfCount = 0;
    Dictionary<int, TileMapObjPool> m_terPoolDic = new Dictionary<int, TileMapObjPool>();
    Dictionary<int, GameObject> m_terrainObjRef = new Dictionary<int, GameObject>();
    Dictionary<string, TileMapObjPool> m_itemPoolDic = new Dictionary<string, TileMapObjPool>();
    Dictionary<string, GameObject> m_itemObjRef = new Dictionary<string, GameObject>();

    int m_lastPos_x = int.MaxValue;
    int m_lastPos_z = int.MaxValue;

    string m_tilemapInfoPath = "";
    string m_tilemapInfoName = "";
    string m_itemInfoPath = "";
    string m_itemInfoName = "";
    string m_terrainObjPath = "";
    string m_terrainObjName = "";
    string m_itemObjPath = "";

    public void Init(TerrainInfo terrainInfo, int view_w, int view_h, float offset_x, float offset_y)
    {
        m_terrainInfoAsset = terrainInfo;
        m_offset = new Vector2(offset_x, offset_y);
        m_visibleObj = new VisiableObj[view_w, view_h];
        m_needVisibleTer = new VisiableTerEx[view_w, view_h];
        m_needVisibleItem = new VisiableItemEx[view_w, view_h];
        m_visiableCount = view_w * view_h;
        m_visiablehalfCount = m_visiableCount / 2;


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
    public void InitEx(TerrainInfo terrainInfo, int v_count, int h_count, float offset_x, float offset_y)
    {
        m_terrainInfoAsset = terrainInfo;
        m_offset = new Vector2(offset_x, offset_y);
        m_visibleObj = new VisiableObj[h_count, v_count];
        m_needVisibleTer = new VisiableTerEx[h_count, v_count];
        m_needVisibleItem = new VisiableItemEx[h_count, v_count];
        m_visiableCount = v_count * h_count;
        m_visiablehalfCount = m_visiableCount / 2;


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
    public void SetResPath(string tileMapInfoPath, string tileMapInfoName, string itemInfoPath, string itemInfoName, string terrainObjPath, string terrainObjName, string itemObjPath)
    {
        m_tilemapInfoPath = tileMapInfoPath;
        m_tilemapInfoName = tileMapInfoName;
        m_itemInfoPath = itemInfoPath;
        m_itemInfoName = itemInfoName;
        m_terrainObjPath = terrainObjPath;
        m_terrainObjName = terrainObjName;
        m_itemObjPath = itemObjPath;
    }
    public void MoveToEx(float x, float z)
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
                if (_pos_x >= m_map_W || _pos_x < 0 || _pos_z >= m_map_H || _pos_z < 0)
                {
                    m_needVisibleTer[i, j].isShow = false;
                    m_needVisibleItem[i, j].isShow = false;
                    SetVisibleTerNUll(i, j);
                    SetVisibleItemNUll(i, j);
                    continue;
                }
                int _index_x = _pos_x / m_tilemap_W;
                int _index_z = _pos_z / m_tilemap_H;

                TileMapInfo _mapInfo = null;
                ItemMapInfo _itemInfo = null;
                bool hasTileMapInfo = GetTileMapInfo(_index_x, _index_z, out _mapInfo);
                bool hasItemMapInfo = GetItemMapInfo(_index_x, _index_z, out _itemInfo);

                int map_index = _pos_z * m_map_W + _pos_x;

                //地形
                int terrainIndex = 0;
                bool hasMapObj = false;
                if (hasTileMapInfo)
                {
                    hasMapObj = _mapInfo.mapInfoDic.TryGetValue(map_index, out terrainIndex);
                }
                if (hasMapObj)
                {
                    m_needVisibleTer[i, j].isShow = true;
                    m_needVisibleTer[i, j].terIndex = terrainIndex;
                }
                else
                {
                    m_needVisibleTer[i, j].isShow = false;
                    SetVisibleTerNUll(i, j);
                }
                //物件
                string itemName = null;
                bool hasItemObj = false;
                if (hasItemMapInfo)
                {
                    hasItemObj = _itemInfo.mapInfoDic.TryGetValue(map_index, out itemName);
                }
                if (hasItemObj)
                {
                    m_needVisibleItem[i, j].isShow = true;
                    m_needVisibleItem[i, j].itemName = itemName;
                }
                else
                {
                    m_needVisibleItem[i, j].isShow = false;
                    SetVisibleItemNUll(i, j);
                }
                
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
                    SetVisibleItem(i, j, _pos_x, _pos_z);
                }
            }
            b = !b;
        }
        pos_temp.x = start_x;
        pos_temp.z = start_z;
        Root.position = pos_temp;
    }
    public void MoveTo(float x, float z)
    {
        int start_x = Mathf.FloorToInt(x);
        int start_z = Mathf.FloorToInt(z);
        if (start_x == m_lastPos_x && start_z == m_lastPos_z)
        {
            return;
        }
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
                // int _pos_z = start_z + j;
                // int _pos_x = start_x + i;
                if (_pos_x >= m_map_W || _pos_x < 0 || _pos_z >= m_map_H || _pos_z < 0)
                {
                    m_needVisibleTer[i, j].isShow = false;
                    m_needVisibleItem[i, j].isShow = false;
                    SetVisibleTerNUll(i, j);
                    SetVisibleItemNUll(i, j);
                    continue;
                }
                int _index_x = _pos_x / m_tilemap_W;
                int _index_z = _pos_z / m_tilemap_H;

                TileMapInfo _mapInfo = null;
                ItemMapInfo _itemInfo = null;
                bool hasTileMapInfo = GetTileMapInfo(_index_x, _index_z, out _mapInfo);
                bool hasItemMapInfo = GetItemMapInfo(_index_x, _index_z, out _itemInfo);

                int map_index = _pos_z * m_map_W + _pos_x;

                //地形
                int terrainIndex = 0;
                bool hasMapObj = false;
                if (hasTileMapInfo)
                {
                    hasMapObj = _mapInfo.mapInfoDic.TryGetValue(map_index, out terrainIndex);
                }
                if (hasMapObj)
                {
                    m_needVisibleTer[i, j].isShow = true;
                    m_needVisibleTer[i, j].terIndex = terrainIndex;
                }
                else
                {
                    m_needVisibleTer[i, j].isShow = false;
                    SetVisibleTerNUll(i, j);
                }
                //物件
                string itemName = null;
                bool hasItemObj = false;
                if (hasItemMapInfo)
                {
                    hasItemObj = _itemInfo.mapInfoDic.TryGetValue(map_index, out itemName);
                }
                if (hasItemObj)
                {
                    m_needVisibleItem[i, j].isShow = true;
                    m_needVisibleItem[i, j].itemName = itemName;
                }
                else
                {
                    m_needVisibleItem[i, j].isShow = false;
                    SetVisibleItemNUll(i, j);
                }
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
                    SetVisibleItem(i, j, i, j);
                }
            }
        }
        pos_temp.x = start_x;
        pos_temp.z = start_z;
        Root.position = pos_temp;
    }
    Vector3 pos_temp = Vector3.zero;
    Vector3 rot_temp = new Vector3(0,-135,0);
    Vector3 hide_temp = new Vector3(0f, 1000f, 0f);
    Transform tra_temp;
    void SetVisibleTer(int i, int j, int posx, int posz)
    {
        var _visibleObj = m_visibleObj[i, j];
        int terIndex = m_needVisibleTer[i, j].terIndex;
        // int terRotY = m_needVisibleObj[i, j].terRotY;
        //地形
        var _terObject = _visibleObj.ter;
        pos_temp.x = posx + m_offset.x;
        pos_temp.z = posz + m_offset.y;
        // rot_temp.y = terRotY - 135;

        if (!System.Object.ReferenceEquals(_terObject, null))
        {
            tra_temp = _terObject.transform;
            if (_visibleObj.terIndex == terIndex)
            {
                tra_temp.localPosition = pos_temp;
                tra_temp.localEulerAngles = rot_temp;
                return;
            }
            tra_temp.localPosition = hide_temp;
            RecycleMapObj(_visibleObj.terIndex, _terObject);
        }

        m_visibleObj[i, j].terIndex = terIndex;
        m_visibleObj[i, j].ter = GetMapObjFromPool(terIndex);
        _visibleObj = m_visibleObj[i, j];
        _terObject = _visibleObj.ter;
        if (!System.Object.ReferenceEquals(_terObject, null))
        {
            tra_temp = _terObject.transform;
            tra_temp.localPosition = pos_temp;
            tra_temp.localEulerAngles = rot_temp;
        }
    }
     void SetVisibleItem(int i, int j, int posx, int posz)
    {
        var _visibleObj = m_visibleObj[i, j];
        string itemName = m_needVisibleItem[i, j].itemName;
        // int itemRotY = m_needVisibleObj[i, j].itemRotY;

        pos_temp.x = posx + m_offset.x;
        pos_temp.z = posz + m_offset.y;
        //物件
        var _itemObject = _visibleObj.item;

        if (!System.Object.ReferenceEquals(_itemObject, null))
        {
            tra_temp = _itemObject.transform;
            if (_visibleObj.itemName == itemName)
            {
                tra_temp.localPosition = pos_temp;
                tra_temp.localEulerAngles = rot_temp;
                return;
            }
            tra_temp.localPosition = hide_temp;
            RecycleItemObj(_visibleObj.itemName, _itemObject);
        }

        m_visibleObj[i, j].itemName = itemName;
        m_visibleObj[i, j].item = GetItemObjFromPool(itemName);
        _visibleObj = m_visibleObj[i, j];
        _itemObject = _visibleObj.item;
        if (!System.Object.ReferenceEquals(_itemObject, null))
        {
            tra_temp = _itemObject.transform;
            tra_temp.localPosition = pos_temp;
            // tra_temp.localEulerAngles = rot_temp;
        }
    }
    void SetVisibleTerNUll(int i, int j)
    {
        var obj = m_visibleObj[i, j];
        //地形
        if (obj.ter)
        {
            obj.ter.transform.localPosition = hide_temp;
            RecycleMapObj(obj.terIndex, obj.ter);
            m_visibleObj[i, j].ter = null;
            m_visibleObj[i, j].terIndex = -1;
        }
        //物件
        if (obj.item)
        {
            obj.item.transform.localPosition = hide_temp;
            RecycleItemObj(obj.itemName, obj.item);
            m_visibleObj[i, j].item = null;
            m_visibleObj[i, j].itemName = "";
        }
    }
    void SetVisibleItemNUll(int i, int j)
    {
        var obj = m_visibleObj[i, j];
        //物件
        if (obj.item)
        {
            obj.item.transform.localPosition = hide_temp;
            RecycleItemObj(obj.itemName, obj.item);
            m_visibleObj[i, j].item = null;
            m_visibleObj[i, j].itemName = "";
        }
    }
    GameObject GetMapObjFromPool(int index)
    {
        if (index == 0)
        {
            return null;
        }
        TileMapObjPool pool;
        if (m_terPoolDic.TryGetValue(index, out pool))
        {
            return pool.Get();
        }
        pool = new TileMapObjPool(GetTerrainObj(index), Root, m_visiablehalfCount);
        m_terPoolDic.Add(index, pool);
        return pool.Get();
    }
    void RecycleMapObj(int index, GameObject obj)
    {
        m_terPoolDic[index].Recycle(obj);
    }
    GameObject GetItemObjFromPool(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        TileMapObjPool pool;
        if (m_itemPoolDic.TryGetValue(name, out pool))
        {
            return pool.Get();
        }
        pool = new TileMapObjPool(GetItemObj(name), Root, m_visiablehalfCount);
        m_itemPoolDic.Add(name, pool);
        return pool.Get();
    }
    void RecycleItemObj(string name, GameObject obj)
    {
        m_itemPoolDic[name].Recycle(obj);
    }
    bool GetTileMapInfo(int w, int h, out TileMapInfo info)
    {
        if (!m_tilemapInfoIsInit[w, h])
        {
            if (!m_terrainInfoAsset.MapInfoList[w * m_splitMap_X + h])
            {
                m_tilemapInfoIsInit[w, h] = true;
                info = null;
                return false;
            }
            string name = string.Format(m_tilemapInfoName, w, h);
#if TILEMAP_TEST
            var mapInfo = AssetDatabase.LoadAssetAtPath<TileMapInfo>(string.Format("{0}/{1}.asset", m_tilemapInfoPath, name));
            if (mapInfo != null)
            {
                mapInfo.Init();
                m_tilemapInfo[w, h] = mapInfo;
            }
            m_tilemapInfoIsInit[w, h] = true;
#else
            LoadManager.Instance.LoadAsset(m_tilemapInfoPath, name, "asset", typeof(TileMapInfo), (data) =>
            {
                var mapInfo = data as TileMapInfo;
                if (mapInfo != null)
                {
                    mapInfo.Init();
                    m_tilemapInfo[w, h] = mapInfo;
                }
                m_tilemapInfoIsInit[w, h] = true;
            }, false);
#endif
        }
        info = m_tilemapInfo[w, h];
        return m_terrainInfoAsset.MapInfoList[w * m_splitMap_X + h];
    }
    bool GetItemMapInfo(int w, int h, out ItemMapInfo info)
    {
        if (!m_itemmapInfoIsInit[w, h])
        {
            if (!m_terrainInfoAsset.ItemInfoList[w * m_splitMap_X + h])
            {
                m_itemmapInfoIsInit[w, h] = true;
                info = null;
                return false;
            }
            string name = string.Format(m_itemInfoName, w, h);
#if TILEMAP_TEST
            var itemInfo = AssetDatabase.LoadAssetAtPath<ItemMapInfo>(string.Format("{0}/{1}.asset", m_itemInfoPath, name));
            if (itemInfo != null)
            {
                itemInfo.Init();
                m_itemmapInfo[w, h] = itemInfo;
            }
            m_itemmapInfoIsInit[w, h] = true;
#else
            LoadManager.Instance.LoadAsset(m_itemInfoPath, name, "asset", typeof(ItemMapInfo), (data) =>
            {
                var itemInfo = data as ItemMapInfo;
                if (itemInfo != null)
                {
                    itemInfo.Init();
                    m_itemmapInfo[w, h] = itemInfo;
                }
                m_itemmapInfoIsInit[w, h] = true;
            }, false);
#endif
        }
        info = m_itemmapInfo[w, h];
        return m_terrainInfoAsset.ItemInfoList[w * m_splitMap_X + h];
    }
    GameObject GetTerrainObj(int index)
    {
        GameObject obj = null;
        if (!m_terrainObjRef.TryGetValue(index, out obj))
        {
            string name = string.Format(m_terrainObjName, index);
#if TILEMAP_TEST
            obj = AssetDatabase.LoadAssetAtPath<GameObject>(string.Format("{0}/{1}.prefab", m_terrainObjPath, name));
            m_terrainObjRef.Add(index, obj);
#else
            LoadManager.Instance.LoadAsset(m_terrainObjPath, name, "prefab", typeof(GameObject), (data) =>
            {
                obj = data as GameObject;
                m_terrainObjRef.Add(index, obj);
            }, false);
#endif
        }
        return obj;
    }

    GameObject GetItemObj(string name)
    {
        GameObject obj = null;
        if (!m_itemObjRef.TryGetValue(name, out obj))
        {
#if TILEMAP_TEST
            obj = AssetDatabase.LoadAssetAtPath<GameObject>(string.Format("{0}/{1}.prefab", m_itemObjPath, name));
            m_itemObjRef.Add(name, obj);
#else
            LoadManager.Instance.LoadAsset(m_itemObjPath, name, "prefab", typeof(GameObject), (data) =>
            {
                obj = data as GameObject;
                m_itemObjRef.Add(name, obj);
            }, false);
#endif
        }
        return obj;
    }
    struct VisiableObj
    {
        public int terIndex;
        public GameObject ter;
        public string itemName;
        public GameObject item;
    }
    struct VisiableTerEx
    {
        public bool isShow;
        public int terIndex;
        // public int terRotY;
    }
    struct VisiableItemEx
    {
        public bool isShow;
        public string itemName;
        // public int itemRotY;
    }

}
