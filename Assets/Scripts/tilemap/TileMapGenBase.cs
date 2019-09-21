using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileMapGenBase : MonoBehaviour
{
    public Transform Root;                      // 场景根节点
    public Transform TerRoot;
    public Transform ItemRoot;
    protected uint m_level = uint.MaxValue;
    protected TerrainInfo m_terrainInfoAsset;
    protected TileMapInfo[,] m_tilemapInfo;
    protected bool[,] m_tilemapInfoIsInit;
    protected ItemMapInfo[,] m_itemmapInfo;
    protected bool[,] m_itemmapInfoIsInit;
    protected AlphaTexInfo[,] m_alphaTexInfo;
    protected bool[,] m_alphaTexInfoIsInit;
    protected int m_tilemap_W;
    protected int m_tilemap_H;
    protected int m_map_W;
    protected int m_map_H;
    protected int m_splitMap_X;
    protected int m_splitMap_Y;
    protected Vector2 m_offset;
    protected bool m_forceMoveTo = false;

    protected VisiableObj[,] m_visibleObj;
    protected VisiableTerEx[,] m_needVisibleTer;
    protected VisiableItemEx[,] m_needVisibleItem;
    protected VisiableItemEx[,] m_needVisibleAlphaTex;
    // protected HashSet<int> m_hideItem;
    protected bool[,] m_hideItemList;
    protected int m_itemSizeEx_x = 0;
    protected int m_itemSizeEx_x_Half = 0;
    protected int m_itemSizeEx_y = 0;
    protected int m_itemSizeEx_y_Half = 0;
    protected int m_itemSizeEx_y_Quarter = 0;
    protected int m_visiableCount_v;
    protected int m_visiableCount_h;
    protected Dictionary<int, TileMapObjPool> m_terPoolDic = new Dictionary<int, TileMapObjPool>();
    protected Dictionary<int, GameObject> m_terrainObjRef = new Dictionary<int, GameObject>();
    protected Dictionary<string, TileMapObjPool> m_itemPoolDic = new Dictionary<string, TileMapObjPool>();
    protected Dictionary<string, GameObject> m_itemObjRef = new Dictionary<string, GameObject>();
    protected Dictionary<string, TileMapObjPool> m_alphaTexPoolDic = new Dictionary<string, TileMapObjPool>();
    protected Dictionary<string, GameObject> m_alphaTexObjRef = new Dictionary<string, GameObject>();

    protected int m_lastPos_x = int.MaxValue;
    protected int m_lastPos_z = int.MaxValue;

    protected Vector3 pos_temp = Vector3.zero;
    protected Vector3 rot_temp = new Vector3(0, -135, 0);
    protected Transform tra_temp;

    bool m_fullPath = false;
    string m_tilemapInfoPath = "";
    string m_tilemapInfoName = "";
    string m_itemInfoPath = "";
    string m_itemInfoName = "";
    string m_alphaTexPath = "";
    string m_alphaTexName = "";
    string m_terrainObjPath = "";
    string m_terrainObjName = "";
    string m_itemObjPath = "";
    string m_alphaTexObjPath = "";

    public virtual void Init(TerrainInfo terrainInfo, int v_count, int h_count, int itemEx_x, int itemEx_y, float offset_x, float offset_y)
    {
        m_terrainInfoAsset = terrainInfo;
        m_offset = new Vector2(offset_x, offset_y);
        m_visibleObj = new VisiableObj[h_count + itemEx_y, v_count + itemEx_x];
        m_needVisibleTer = new VisiableTerEx[h_count + itemEx_y, v_count + itemEx_x];
        m_needVisibleItem = new VisiableItemEx[h_count + itemEx_y, v_count + itemEx_x];
        m_needVisibleAlphaTex = new VisiableItemEx[h_count + itemEx_y, v_count + itemEx_x];

        m_itemSizeEx_x = itemEx_x;
        m_itemSizeEx_x_Half = itemEx_x / 2;
        m_itemSizeEx_y = itemEx_y;
        m_itemSizeEx_y_Half = itemEx_y / 2;
        m_itemSizeEx_y_Quarter = itemEx_y / 4;

        m_visiableCount_v = v_count + itemEx_x;
        m_visiableCount_h = h_count + itemEx_y;

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
        m_alphaTexInfo = new AlphaTexInfo[x, y];
        m_tilemapInfoIsInit = new bool[x, y];
        m_itemmapInfoIsInit = new bool[x, y];
        m_alphaTexInfoIsInit = new bool[x, y];

        // m_hideItem = new HashSet<int>();
        m_hideItemList = new bool[m_map_W,m_map_H];
    }
    public virtual void SetLevel(uint level)
    {
        m_level = level;
    }
    public virtual void MoveTo(float x, float z)
    {
    }
    public virtual void HideItemAtPos(int x, int z)
    {
    }
    public virtual void ShowItemAtPos(int x, int z)
    {
    }
    public virtual void SetForceMoveTo()
    {
        m_forceMoveTo = true;
    }
    public void SetResPath(bool fullPath, string tileMapInfoPath, string tileMapInfoName,
     string itemInfoPath, string itemInfoName,
     string alphaTexPath, string alphaTexName,
     string terrainObjPath, string terrainObjName,
     string itemObjPath,
     string alphaTexObjPath)
    {
        m_fullPath = fullPath;
        m_tilemapInfoPath = tileMapInfoPath;
        m_tilemapInfoName = tileMapInfoName;
        m_itemInfoPath = itemInfoPath;
        m_itemInfoName = itemInfoName;
        m_alphaTexPath = alphaTexPath;
        m_alphaTexName = alphaTexName;
        m_terrainObjPath = terrainObjPath;
        m_terrainObjName = terrainObjName;
        m_itemObjPath = itemObjPath;
        m_alphaTexObjPath = alphaTexObjPath;
    }
    protected void CheckVisibleTer(int _pos_x, int _pos_z, int i, int j)
    {
        if (_pos_x >= m_map_W || _pos_x < 0 || _pos_z >= m_map_H || _pos_z < 0)
        {
            m_needVisibleTer[i, j].isShow = false;
            SetVisibleTerNUll(i, j);
            return;
        }
        int _index_x = _pos_x / m_tilemap_W;
        int _index_z = _pos_z / m_tilemap_H;

        TileMapInfo _mapInfo = null;
        bool hasTileMapInfo = GetTileMapInfo(_index_x, _index_z, out _mapInfo);

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
    }
    protected void CheckVisibleItem(int _pos_x, int _pos_z, int i, int j)
    {
        int id = _pos_x * 10000 + _pos_z;
        // if (m_hideItem.Contains(id))
        if (_pos_x >= m_map_W || _pos_x < 0 || _pos_z >= m_map_H || _pos_z < 0)
        {
            m_needVisibleItem[i, j].isShow = false;
            SetVisibleItemNUll(i, j);
            return;
        }
        if (m_hideItemList[_pos_x,_pos_z])
        {
            m_needVisibleItem[i, j].isShow = false;
            SetVisibleItemNUll(i, j);
            return;
        }
        int _index_x = _pos_x / m_tilemap_W;
        int _index_z = _pos_z / m_tilemap_H;

        ItemMapInfo _itemInfo = null;
        bool hasItemMapInfo = GetItemMapInfo(_index_x, _index_z, out _itemInfo);

        int map_index = _pos_z * m_map_W + _pos_x;

        //物件
        string itemName = null;
        bool hasItemObj = false;
        if (hasItemMapInfo)
        {
            hasItemObj = _itemInfo.itemInfoDic.TryGetValue(map_index, out itemName);
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
    static AlphaTexInfo_S alphaTex_s;
    protected void CheckVisibleAlphaTex(int _pos_x, int _pos_z, int i, int j)
    {
        if (_pos_x >= m_map_W || _pos_x < 0 || _pos_z >= m_map_H || _pos_z < 0)
        {
            m_needVisibleAlphaTex[i, j].isShow = false;
            SetVisibleAlphaTexNUll(i, j);
            return;
        }
        int _index_x = _pos_x / m_tilemap_W;
        int _index_z = _pos_z / m_tilemap_H;

        AlphaTexInfo _AlphaTexInfo = null;
        bool hasAlphaTexMapInfo = GetAlphaTexInfo(_index_x, _index_z, out _AlphaTexInfo);

        int map_index = _pos_z * m_map_W + _pos_x;

        //透贴
        bool hasAlphaTexObj = false;
        if (hasAlphaTexMapInfo)
        {
            hasAlphaTexObj = _AlphaTexInfo.terrainAlphaDic.TryGetValue(map_index, out alphaTex_s);
            hasAlphaTexObj = hasAlphaTexObj ? alphaTex_s.level <= m_level : hasAlphaTexObj;
        }
        if (hasAlphaTexObj)
        {
            m_needVisibleAlphaTex[i, j].isShow = true;
            m_needVisibleAlphaTex[i, j].itemName = alphaTex_s.objName;
        }
        else
        {
            m_needVisibleAlphaTex[i, j].isShow = false;
            SetVisibleAlphaTexNUll(i, j);
        }
    }

    protected void SetVisibleTer(int i, int j, int posx, int posz)
    {
        var _visibleObj = m_visibleObj[i, j];
        int terIndex = m_needVisibleTer[i, j].terIndex;

        //地形
        var _terObject = _visibleObj.ter;
        if (!System.Object.ReferenceEquals(_terObject, null))
        {
            if (_visibleObj.terIndex == terIndex)
            {
                return;
            }
            RecycleMapObj(_visibleObj.terIndex, _terObject);
        }

        m_visibleObj[i, j].terIndex = terIndex;
        m_visibleObj[i, j].ter = GetMapObjFromPool(terIndex, posx, posz);
        _visibleObj = m_visibleObj[i, j];
        _terObject = _visibleObj.ter;
        if (!System.Object.ReferenceEquals(_terObject, null))
        {
            tra_temp = _terObject.transform;
            tra_temp.localEulerAngles = rot_temp;
        }
    }
    protected void SetVisibleItem(int i, int j, int posx, int posz)
    {
        var _visibleObj = m_visibleObj[i, j];
        string itemName = m_needVisibleItem[i, j].itemName;

        //物件
        var _itemObject = _visibleObj.item;
        if (!System.Object.ReferenceEquals(_itemObject, null))
        {
            if (_visibleObj.itemName == itemName)
            {
                return;
            }
            RecycleItemObj(_visibleObj.itemName, _itemObject);
        }

        m_visibleObj[i, j].itemName = itemName;
        m_visibleObj[i, j].item = GetItemObjFromPool(itemName, posx, posz);
    }
    protected void SetVisibleAlphaTex(int i, int j, int posx, int posz)
    {
        var _visibleObj = m_visibleObj[i, j];
        string alphaTexName = m_needVisibleAlphaTex[i, j].itemName;

        //透贴
        var _alphaTexObject = _visibleObj.alphaTex;
        if (!System.Object.ReferenceEquals(_alphaTexObject, null))
        {
            if (_visibleObj.alphaTexName == alphaTexName)
            {
                return;
            }
            RecycleAlphaTexObj(_visibleObj.alphaTexName, _alphaTexObject);
        }

        m_visibleObj[i, j].alphaTexName = alphaTexName;
        m_visibleObj[i, j].alphaTex = GetAlphaTexObjFromPool(alphaTexName, posx, posz);

    }
    protected void SetVisibleTerNUll(int i, int j)
    {
        var obj = m_visibleObj[i, j];
        //地形
        if (!System.Object.ReferenceEquals(obj.ter, null))
        {
            RecycleMapObj(obj.terIndex, obj.ter);
            m_visibleObj[i, j].ter = null;
            m_visibleObj[i, j].terIndex = -1;
        }
    }
    protected void SetVisibleItemNUll(int i, int j)
    {
        var obj = m_visibleObj[i, j];
        //物件
        if (!System.Object.ReferenceEquals(obj.item, null))
        {
            RecycleItemObj(obj.itemName, obj.item);
            m_visibleObj[i, j].item = null;
            m_visibleObj[i, j].itemName = "";
        }
    }
    protected void SetVisibleAlphaTexNUll(int i, int j)
    {
        var obj = m_visibleObj[i, j];
        //透贴
        if (!System.Object.ReferenceEquals(obj.alphaTex, null))
        {
            RecycleAlphaTexObj(obj.alphaTexName, obj.alphaTex);
            m_visibleObj[i, j].alphaTex = null;
            m_visibleObj[i, j].alphaTexName = "";
        }
    }
    GameObject GetMapObjFromPool(int index, int x, int z)
    {
        if (index == 0)
        {
            return null;
        }
        TileMapObjPool pool;
        if (m_terPoolDic.TryGetValue(index, out pool))
        {
            return pool.Get(x, z);
        }
        pool = new TileMapObjPool(GetTerrainObj(index), TerRoot, m_visiableCount_h * m_visiableCount_v);
        m_terPoolDic.Add(index, pool);
        return pool.Get(x, z);
    }
    void RecycleMapObj(int index, GameObject obj)
    {
        m_terPoolDic[index].Recycle(obj);
    }
    GameObject GetItemObjFromPool(string name, int x, int z)
    {
        float _x = x - 0.5f;
        float _z = z - 0.5f;
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        TileMapObjPool pool;
        if (m_itemPoolDic.TryGetValue(name, out pool))
        {
            return pool.Get(_x, _z);
        }
        pool = new TileMapObjPool(GetItemObj(name), ItemRoot, m_visiableCount_h * m_visiableCount_v);
        m_itemPoolDic.Add(name, pool);
        return pool.Get(_x, _z);
    }
    void RecycleItemObj(string name, GameObject obj)
    {
        m_itemPoolDic[name].Recycle(obj);
    }
    GameObject GetAlphaTexObjFromPool(string name, int x, int z)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        TileMapObjPool pool;
        if (m_alphaTexPoolDic.TryGetValue(name, out pool))
        {
            return pool.Get(x, z);
        }
        pool = new TileMapObjPool(GetAlphaTexObj(name), ItemRoot, m_visiableCount_h * m_visiableCount_v);
        m_alphaTexPoolDic.Add(name, pool);
        return pool.Get(x, z);
    }
    void RecycleAlphaTexObj(string name, GameObject obj)
    {
        m_alphaTexPoolDic[name].Recycle(obj);
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
            }, false, false, false, m_fullPath);
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
            }, false, false, false, m_fullPath);
#endif
        }
        info = m_itemmapInfo[w, h];
        return m_terrainInfoAsset.ItemInfoList[w * m_splitMap_X + h];
    }
    bool GetAlphaTexInfo(int w, int h, out AlphaTexInfo info)
    {
        if (!m_alphaTexInfoIsInit[w, h])
        {
            if (!m_terrainInfoAsset.TerrainAlphaList[w * m_splitMap_X + h])
            {
                m_alphaTexInfoIsInit[w, h] = true;
                info = null;
                return false;
            }
            string name = string.Format(m_alphaTexName, w, h);
#if TILEMAP_TEST
            var alphaTex = AssetDatabase.LoadAssetAtPath<AlphaTexInfo>(string.Format("{0}/{1}.asset", m_alphaTexPath, name));
            if (alphaTex != null)
            {
                alphaTex.Init();
                m_alphaTexInfo[w, h] = alphaTex;
            }
            m_alphaTexInfoIsInit[w, h] = true;
#else
            LoadManager.Instance.LoadAsset(m_alphaTexPath, name, "asset", typeof(AlphaTexInfo), (data) =>
            {
                var alphaTex = data as AlphaTexInfo;
                if (alphaTex != null)
                {
                    alphaTex.Init();
                    m_alphaTexInfo[w, h] = alphaTex;
                }
                m_alphaTexInfoIsInit[w, h] = true;
            }, false, false, false, m_fullPath);
#endif
        }
        info = m_alphaTexInfo[w, h];
        return m_terrainInfoAsset.TerrainAlphaList[w * m_splitMap_X + h];
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
            if (obj == null)
            {
                obj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TileMap/TileMapObj/Prefab/Error.prefab");
                Debug.LogError("GetTerrainObj:null:" + name);
            }
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
            if (obj == null)
            {
                obj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TileMap/TileMapObj/Prefab/Error.prefab");
                Debug.LogError("GetItemObj:null:" + name);
            }
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
    GameObject GetAlphaTexObj(string name)
    {
        GameObject obj = null;
        if (!m_alphaTexObjRef.TryGetValue(name, out obj))
        {
#if TILEMAP_TEST
            obj = AssetDatabase.LoadAssetAtPath<GameObject>(string.Format("{0}/{1}.prefab", m_alphaTexObjPath, name));
            m_alphaTexObjRef.Add(name, obj);
            if (obj == null)
            {
                obj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TileMap/TileMapObj/Prefab/Error.prefab");
                Debug.LogError("GetAlphaTexObj:null:" + name);
            }
#else
            LoadManager.Instance.LoadAsset(m_alphaTexObjPath, name, "prefab", typeof(GameObject), (data) =>
            {
                obj = data as GameObject;
                m_alphaTexObjRef.Add(name, obj);
            }, false);
#endif
        }
        return obj;
    }

    protected struct VisiableObj
    {
        public int terIndex;
        public GameObject ter;
        public string itemName;
        public GameObject item;
        public string alphaTexName;
        public GameObject alphaTex;
    }
    protected struct VisiableTerEx
    {
        public bool isShow;
        public int terIndex;
        // public int terRotY;
    }
    protected struct VisiableItemEx
    {
        public bool isShow;
        public string itemName;
        // public int itemRotY;
    }

}
