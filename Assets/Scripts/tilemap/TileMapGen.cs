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
    int m_visiableCount = 0;
    int m_visiablehalfCount = 0;
    VisiableObjEx[,] m_needVisibleObj;
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

    static TileMapObjInfo s_TileMapObjInfo = default(TileMapObjInfo);
    static ItemMapObjInfo s_ItemMapObjInfo = default(ItemMapObjInfo);
    public void Init(TerrainInfo terrainInfo, int view_w, int view_h, float offset_x, float offset_y)
    {
        m_terrainInfoAsset = terrainInfo;
        m_offset = new Vector2(offset_x, offset_y);
        m_visibleObj = new VisiableObj[view_w, view_h];
        m_needVisibleObj = new VisiableObjEx[view_w, view_h];
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
                    m_needVisibleObj[i, j].isShow = false;
                    SetVisibleObjNUll(i, j);
                    continue;
                }
                int _index_x = _pos_x / m_tilemap_W;
                int _index_z = _pos_z / m_tilemap_H;

                TileMapInfo _mapInfo = GetTileMapInfo(_index_x, _index_z);
                ItemMapInfo _itemInfo = GetItemMapInfo(_index_x, _index_z);
                int map_index = _pos_z * m_map_W + _pos_x;
                TileMapObjInfo mapObjInfo = s_TileMapObjInfo;
                ItemMapObjInfo itemObjInfo = s_ItemMapObjInfo;
                bool hasMapObj = false;
                bool hasItemObj = false;
                if (!System.Object.ReferenceEquals(_mapInfo, null))
                {
                    hasMapObj = _mapInfo.mapInfoDic.TryGetValue(map_index, out mapObjInfo);
                }
                if (!System.Object.ReferenceEquals(_itemInfo, null))
                {
                    hasItemObj = _itemInfo.mapInfoDic.TryGetValue(map_index, out itemObjInfo);
                }
                if (hasMapObj || hasItemObj)
                {
                    int objIndex = 0;
                    int objRotY = 0;
                    if (hasMapObj)
                    {
                        objIndex = mapObjInfo.terrainIndex;
                        objRotY = mapObjInfo.terrainRotY;
                    }
                    string itemName = "";
                    int itemRotY = 0;
                    if (hasItemObj)
                    {
                        itemName = itemObjInfo.itemName;
                        itemRotY = itemObjInfo.itemRotY;
                    }
                    m_needVisibleObj[i, j].isShow = true;
                    m_needVisibleObj[i, j].terIndex = objIndex;
                    m_needVisibleObj[i, j].terRotY = objRotY;
                    m_needVisibleObj[i, j].itemName = itemName;
                    m_needVisibleObj[i, j].itemRotY = itemRotY;
                    // SetVisibleObj(i, j, objIndex, objRotY, itemName, itemRotY);
                }
                else
                {
                    m_needVisibleObj[i, j].isShow = false;
                    SetVisibleObjNUll(i, j);
                }
            }
        }
        for (int i = 0; i < visiable_x_count; i++)
        {
            for (int j = 0; j < visiable_z_count; j++)
            {
                if (m_needVisibleObj[i, j].isShow)
                {
                    SetVisibleObj(i, j);
                }
            }
        }
        pos_temp.x = start_x;
        pos_temp.z = start_z;
        Root.position = pos_temp;
    }
    Vector3 pos_temp = Vector3.zero;
    Vector3 rot_temp = Vector3.zero;
    Vector3 hide_temp = new Vector3(0f, 1000f, 0f);
    Transform tra_temp;
    void SetVisibleObj(int i, int j)
    {
        var _visibleObj = m_visibleObj[i, j];
        int terIndex = m_needVisibleObj[i, j].terIndex;
        int terRotY = m_needVisibleObj[i, j].terRotY;
        string itemName = m_needVisibleObj[i, j].itemName;
        int itemRotY = m_needVisibleObj[i, j].itemRotY;
        //地形
        var _terObject = _visibleObj.ter;
        pos_temp.x = i + m_offset.x;
        pos_temp.z = j + m_offset.y;
        rot_temp.y = terRotY - 135;

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

        //物件
        var _itemObject = _visibleObj.item;
        rot_temp.y = itemRotY;

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
            tra_temp.localEulerAngles = rot_temp;
        }

    }
    void SetVisibleObjNUll(int i, int j)
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
    TileMapInfo GetTileMapInfo(int w, int h)
    {
        if (!m_tilemapInfoIsInit[w, h] && m_tilemapInfo[w, h] == null)
        {
            if (!m_terrainInfoAsset.MapInfoList[w*m_splitMap_X + h])
            {
                m_tilemapInfoIsInit[w, h] = true;
                return null;
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
        return m_tilemapInfo[w, h];
    }
    ItemMapInfo GetItemMapInfo(int w, int h)
    {
        if (!m_itemmapInfoIsInit[w, h] && m_itemmapInfo[w, h] == null)
        {
            if (!m_terrainInfoAsset.ItemInfoList[w*m_splitMap_X + h])
            {
                m_itemmapInfoIsInit[w, h] = true;
                return null;
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
        return m_itemmapInfo[w, h];
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
    struct VisiableObjEx
    {
        public bool isShow;
        public int terIndex;
        public int terRotY;
        public string itemName;
        public int itemRotY;
    }

}
