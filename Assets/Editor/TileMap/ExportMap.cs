using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using TiledJsonUtility;

public class ExportMap : Editor
{
    static string m_MapName = "Map_KOW";
    static string TerrainLayerName = "Terrain";
    static string RemovableItemLayerName = "RemovableItem";
    static string UnRemovableItemLayerName = "UnRemovableItem";
    static string UnRemovableItemLayer_HName = "UnRemovableItemH";
    static string TerrainAlphaLayer1Name = "TerrainAlpha1";
    static string TerrainAlphaLayer2Name = "TerrainAlpha2";
    static string TerrainAlphaLayer3Name = "TerrainAlpha3";
    static int SeaID = 8;
    static HashSet<int> UnSetTerrain = new HashSet<int> { SeaID, };
    static int m_MapSpiltSize_x = 80;
    static int m_MapSpiltSize_y = 80;

    [MenuItem("Map/ExportTerrain")]
    public static void ExportTerrain()
    {
        CleanSOCache();
        CleanGidTilesetCache();

        System.DateTime beforDT = System.DateTime.Now;
        if (Directory.Exists(Application.dataPath + "/TileMap/MapInfo/"))
        {
            Directory.Delete(Application.dataPath + "/TileMap/MapInfo/", true);
        }
        Directory.CreateDirectory(Application.dataPath + "/TileMap/MapInfo/");
        //加载Map
        TiledMap map = LoadMap(m_MapName);

        var globalGridSizeX = map.width;
        var globalGridSizeZ = map.height;

        //计算map分块大小,创建分块map配置
        int spiltMap_w = Mathf.CeilToInt(globalGridSizeX * 1f / m_MapSpiltSize_x);
        int spiltMap_h = Mathf.CeilToInt(globalGridSizeZ * 1f / m_MapSpiltSize_y);

        //创建地形obj配置
        TerrainInfo terrainInfo = GetStriptableObject<TerrainInfo>("Assets/TileMap/TerrainInfo.asset");
        terrainInfo.MapSize_W = globalGridSizeX;
        terrainInfo.MapSize_H = globalGridSizeZ;
        terrainInfo.SpiltMapSize_W = spiltMap_w;
        terrainInfo.SpiltMapSize_H = spiltMap_h;
        terrainInfo.SpiltMap_X = m_MapSpiltSize_x;
        terrainInfo.SpiltMap_Y = m_MapSpiltSize_y;
        terrainInfo.MapInfoList = new bool[m_MapSpiltSize_x * m_MapSpiltSize_y];

        //地形分块数据
        Dictionary<string, TiledLayer> _layerDic = new Dictionary<string, TiledLayer>();
        for (int i = 0; i < map.layers.Length; i++)
        {
            var layer = map.layers[i];
            string layerName = layer.name;
            _layerDic.Add(layerName, layer);
        }
        if (_layerDic.ContainsKey(TerrainLayerName))
        {
            var terrainTiles = _layerDic[TerrainLayerName].GetTiles();
            int globalSize = terrainTiles.Length;
            for (int j = 0; j < terrainTiles.Length; j++)
            {
                EditorUtility.DisplayProgressBar("ExportTerrain", j + "/" + (globalSize), j / (float)globalSize);
                var _terrTile = terrainTiles[j];
                // int _terrRY = GetRotY(_terrTile);
                int terrainID = _terrTile.Gid;
                GetTilesetNameAndIndex(map, _terrTile.Gid, out terrainID);

                if (terrainID != 0 && !UnSetTerrain.Contains(terrainID))
                {
                    int mapIndex_x = _terrTile.X / spiltMap_w;
                    int mapIndex_y = _terrTile.Y / spiltMap_h;
                    terrainInfo.MapInfoList[mapIndex_x * m_MapSpiltSize_x + mapIndex_y] = true;
                    var _mapinfo = GetStriptableObject<TileMapInfo>(string.Format("Assets/TileMap/MapInfo/MapInfo_{0}_{1}.asset", mapIndex_x, mapIndex_y));
                    int _posIndex = _terrTile.Y * globalGridSizeX + _terrTile.X;

                    int terrainIndex = terrainID == SeaID ? 0 : terrainID;
                    // objInfo.terrainRotY = _terrRY;

                    _mapinfo.posIndex.Add(_posIndex);
                    _mapinfo.terrainIndexList.Add(terrainIndex);

                    // UnityEditor.EditorUtility.SetDirty(_mapinfo);
                }
            }
        }

        System.DateTime afterDT = System.DateTime.Now;
        System.TimeSpan ts = afterDT.Subtract(beforDT);
        Debug.Log(ts.TotalSeconds);
        EditorUtility.ClearProgressBar();

        CleanSOCache();
        CleanGidTilesetCache();

        // UnityEditor.EditorUtility.SetDirty(terrainInfo);
        UnityEditor.AssetDatabase.SaveAssets();

    }
    [MenuItem("Map/ExportObj")]
    public static void ExportObj()
    {
        CleanSOCache();
        CleanGidTilesetCache();
        System.DateTime beforDT = System.DateTime.Now;
        if (Directory.Exists(Application.dataPath + "/TileMap/ItemInfo/"))
        {
            Directory.Delete(Application.dataPath + "/TileMap/ItemInfo/", true);
        }
        Directory.CreateDirectory(Application.dataPath + "/TileMap/ItemInfo/");
        //加载Map
        TiledMap map = LoadMap(m_MapName);
        var globalGridSizeX = map.width;
        var globalGridSizeZ = map.height;
        //计算map分块大小
        int spiltMap_w = Mathf.CeilToInt(globalGridSizeX * 1f / m_MapSpiltSize_x);
        int spiltMap_h = Mathf.CeilToInt(globalGridSizeZ * 1f / m_MapSpiltSize_y);

        //创建地形obj配置
        TerrainInfo terrainInfo = GetStriptableObject<TerrainInfo>("Assets/TileMap/TerrainInfo.asset");
        terrainInfo.MapSize_W = globalGridSizeX;
        terrainInfo.MapSize_H = globalGridSizeZ;
        terrainInfo.SpiltMapSize_W = spiltMap_w;
        terrainInfo.SpiltMapSize_H = spiltMap_h;
        terrainInfo.SpiltMap_X = m_MapSpiltSize_x;
        terrainInfo.SpiltMap_Y = m_MapSpiltSize_y;
        terrainInfo.ItemInfoList = new bool[m_MapSpiltSize_x * m_MapSpiltSize_y];

        //物件分块数据
        Dictionary<string, TiledLayer> _layerDic = new Dictionary<string, TiledLayer>();
        for (int i = 0; i < map.layers.Length; i++)
        {
            var layer = map.layers[i];
            string layerName = layer.name;
            _layerDic.Add(layerName, layer);
        }
        var unRemoveTiles = _layerDic.ContainsKey(UnRemovableItemLayerName) ? _layerDic[UnRemovableItemLayerName].GetTiles() : null;
        var removableTiles = _layerDic.ContainsKey(RemovableItemLayerName) ? _layerDic[RemovableItemLayerName].GetTiles() : null;
        int globalSize = globalGridSizeX * globalGridSizeZ;
        for (int i = 0; i < globalSize; i++)
        {
            // EditorUtility.DisplayProgressBar("ExportObj", i+"/"+(globalSize), i / (float)globalSize);
            TiledLayerTile _unRemoveTile = new TiledLayerTile();
            int _unRemoveID = 0;
            // int _unRemoveRY = 0;
            string _unRemoveName = "";
            if (unRemoveTiles != null)
            {
                _unRemoveTile = unRemoveTiles[i];
                // _unRemoveRY = GetRotY(_unRemoveTile);
                _unRemoveID = _unRemoveTile.Gid;
                _unRemoveName = GetTilesetNameAndIndex(map, _unRemoveTile.Gid, out _unRemoveID);
            }
            TiledLayerTile _removableTile = new TiledLayerTile();
            int _removableID = 0;
            // int _removableRY = 0;
            string _removableName = "";
            if (removableTiles != null)
            {
                _removableTile = removableTiles[i];
                // _removableRY = GetRotY(_removableTile);
                _removableID = _removableTile.Gid;
                _removableName = GetTilesetNameAndIndex(map, _removableTile.Gid, out _removableID);
            }
            if (_unRemoveID == 1 || _removableID == 1)
            {
                int X = _unRemoveID != 0 ? _unRemoveTile.X : _removableTile.X;
                int Y = _unRemoveID != 0 ? _unRemoveTile.Y : _removableTile.Y;


                int itemIndex_x = X / spiltMap_w;
                int itemIndex_y = Y / spiltMap_h;
                terrainInfo.ItemInfoList[itemIndex_x * m_MapSpiltSize_x + itemIndex_y] = true;
                var _mapinfo = GetStriptableObject<ItemMapInfo>(string.Format("Assets/TileMap/ItemInfo/ItemInfo_{0}_{1}.asset", itemIndex_x, itemIndex_y));
                int _posIndex = Y * globalGridSizeX + X;

                string itemName = _unRemoveID == 1 ? _unRemoveName : _removableName;
                // objInfo.itemRotY = _unRemoveID == 1 ? _unRemoveRY : _removableRY;

                _mapinfo.posIndex.Add(_posIndex);
                _mapinfo.itemNameList.Add(itemName);

                // UnityEditor.EditorUtility.SetDirty(_mapinfo);
            }
        }

        System.DateTime afterDT = System.DateTime.Now;
        System.TimeSpan ts = afterDT.Subtract(beforDT);
        Debug.Log(ts.TotalSeconds);
        EditorUtility.ClearProgressBar();
        // UnityEditor.EditorUtility.SetDirty(terrainInfo);
        CleanSOCache();
        CleanGidTilesetCache();
        UnityEditor.AssetDatabase.SaveAssets();

    }

    [MenuItem("Map/ExportTerrainAlpha")]
    public static void ExportTerrainAlpha()
    {
        CleanSOCache();
        CleanGidTilesetCache();
        System.DateTime beforDT = System.DateTime.Now;

        if (Directory.Exists(Application.dataPath + "/TileMap/TerrainAlpha/"))
        {
            Directory.Delete(Application.dataPath + "/TileMap/TerrainAlpha/", true);
        }
        Directory.CreateDirectory(Application.dataPath + "/TileMap/TerrainAlpha/");
        //加载Map
        TiledMap map = LoadMap(m_MapName);
        var globalGridSizeX = map.width;
        var globalGridSizeZ = map.height;
        //计算map分块大小
        int spiltMap_w = Mathf.CeilToInt(globalGridSizeX * 1f / m_MapSpiltSize_x);
        int spiltMap_h = Mathf.CeilToInt(globalGridSizeZ * 1f / m_MapSpiltSize_y);

        //创建地形obj配置
        TerrainInfo terrainInfo = GetStriptableObject<TerrainInfo>("Assets/TileMap/TerrainInfo.asset");
        terrainInfo.MapSize_W = globalGridSizeX;
        terrainInfo.MapSize_H = globalGridSizeZ;
        terrainInfo.SpiltMapSize_W = spiltMap_w;
        terrainInfo.SpiltMapSize_H = spiltMap_h;
        terrainInfo.SpiltMap_X = m_MapSpiltSize_x;
        terrainInfo.SpiltMap_Y = m_MapSpiltSize_y;
        terrainInfo.TerrainAlphaList = new bool[m_MapSpiltSize_x * m_MapSpiltSize_y];

        //物件分块数据
        Dictionary<string, TiledLayer> _layerDic = new Dictionary<string, TiledLayer>();
        for (int i = 0; i < map.layers.Length; i++)
        {
            var layer = map.layers[i];
            string layerName = layer.name;
            _layerDic.Add(layerName, layer);
        }
        var terrainAlphaTiles1 = _layerDic.ContainsKey(TerrainAlphaLayer1Name) ? _layerDic[TerrainAlphaLayer1Name].GetTiles() : null;
        var terrainAlphaTiles2 = _layerDic.ContainsKey(TerrainAlphaLayer2Name) ? _layerDic[TerrainAlphaLayer2Name].GetTiles() : null;
        var terrainAlphaTiles3 = _layerDic.ContainsKey(TerrainAlphaLayer3Name) ? _layerDic[TerrainAlphaLayer3Name].GetTiles() : null;
        int globalSize = globalGridSizeX * globalGridSizeZ;
        for (int i = 0; i < globalSize; i++)
        {
            // EditorUtility.DisplayProgressBar("ExportTerrainAlpha", i+"/"+(globalSize), i / (float)globalSize);
            TiledLayerTile _terrainAlphaTile = new TiledLayerTile();
            int _terrainAlphaIndex = 0;
            // int _terainAlphaRY = 0;
            string _terrainAlphaName = "";
            uint _level = 0;
            if (terrainAlphaTiles3 != null)
            {
                _terrainAlphaTile = terrainAlphaTiles3[i];
                int index;
                string itemName = GetTilesetNameAndIndex(map, _terrainAlphaTile.Gid, out index);
                if (index == 1)
                {
                    _terrainAlphaIndex = index;
                    _terrainAlphaName = itemName;
                    _level = 3;
                }
            }
            if (terrainAlphaTiles2 != null)
            {
                _terrainAlphaTile = terrainAlphaTiles2[i];
                int index;
                string itemName = GetTilesetNameAndIndex(map, _terrainAlphaTile.Gid, out index);
                if (index == 1)
                {
                    _terrainAlphaIndex = index;
                    _terrainAlphaName = itemName;
                    _level = 2;
                }
            }
            if (terrainAlphaTiles1 != null)
            {
                _terrainAlphaTile = terrainAlphaTiles1[i];
                int index;
                string itemName = GetTilesetNameAndIndex(map, _terrainAlphaTile.Gid, out index);
                if (index == 1)
                {
                    _terrainAlphaIndex = index;
                    _terrainAlphaName = itemName;
                    _level = 1;
                }
            }
            if (_terrainAlphaIndex == 1)
            {
                int X = _terrainAlphaTile.X;
                int Y = _terrainAlphaTile.Y;

                int itemIndex_x = X / spiltMap_w;
                int itemIndex_y = Y / spiltMap_h;
                terrainInfo.TerrainAlphaList[itemIndex_x * m_MapSpiltSize_x + itemIndex_y] = true;
                var _mapinfo = GetStriptableObject<AlphaTexInfo>(string.Format("Assets/TileMap/TerrainAlpha/TerrainAlpha_{0}_{1}.asset", itemIndex_x, itemIndex_y));
                int _posIndex = Y * globalGridSizeX + X;

                AlphaTexInfo_S alphaTexInfo = new AlphaTexInfo_S();
                alphaTexInfo.objName = _terrainAlphaName;
                alphaTexInfo.level = _level;

                _mapinfo.posIndex.Add(_posIndex);
                _mapinfo.terrainAlphaList.Add(alphaTexInfo);

                // UnityEditor.EditorUtility.SetDirty(_mapinfo);
            }
        }

        System.DateTime afterDT = System.DateTime.Now;
        System.TimeSpan ts = afterDT.Subtract(beforDT);
        Debug.Log(ts.TotalSeconds);
        EditorUtility.ClearProgressBar();
        CleanSOCache();
        CleanGidTilesetCache();
        UnityEditor.AssetDatabase.SaveAssets();

    }

    [MenuItem("Map/ExportServerData")]
    public static void ExportServerdata()
    {
        //加载Map
        TiledMap map = LoadMap(m_MapName);
        var globalGridSizeX = map.width;
        var globalGridSizeZ = map.height;
        //物件分块数据
        Dictionary<string, TiledLayer> _layerDic = new Dictionary<string, TiledLayer>();
        for (int i = 0; i < map.layers.Length; i++)
        {
            var layer = map.layers[i];
            string layerName = layer.name;
            _layerDic.Add(layerName, layer);
        }
        var terrainTiles = _layerDic.ContainsKey(TerrainLayerName) ? _layerDic[TerrainLayerName].GetTiles() : null;
        var unRemoveTiles = _layerDic.ContainsKey(UnRemovableItemLayerName) ? _layerDic[UnRemovableItemLayerName].GetTiles() : null;
        // var removableTiles = _layerDic.ContainsKey(RemovableItemLayerName) ? _layerDic[RemovableItemLayerName].GetTiles() : null;
        StringBuilder sb = new StringBuilder();
        sb.Append("local kingdomMapData = {\n");
        for (int i = 0; i < globalGridSizeX * globalGridSizeZ; i++)
        {
            int _pos_x = 0;
            int _pos_z = 0;
            bool isUnRemove = false;
            if (unRemoveTiles[i].Gid != 0)
            {
                _pos_x = globalGridSizeX - unRemoveTiles[i].X;
                _pos_z = globalGridSizeZ - unRemoveTiles[i].Y;
                isUnRemove = true;
            }
            else if (UnSetTerrain.Contains(terrainTiles[i].Gid))
            {
                _pos_x = globalGridSizeX - terrainTiles[i].X;
                _pos_z = globalGridSizeZ - terrainTiles[i].Y;
                isUnRemove = true;
            }
            if (isUnRemove)
            {
                int temp = _pos_x;
                _pos_x = _pos_z;
                _pos_z = temp;
                int posIndex = _pos_x * 10000 + _pos_z;
                // sb.Append(posIndex+",");
                sb.AppendFormat("[{0}] = 2,\n", posIndex);
            }
        }
        sb.Append("};return kingdomMapData");

        string path = string.Format("{0}/TileMapSerData/kingdomMapData.lua", Application.dataPath);
        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        Stream stream = File.Open(path, FileMode.Create);
        stream.Write(bytes, 0, bytes.Length);
        stream.Close();
        Debug.Log("ExportServerdata Finish");
    }

    [MenuItem("Map/ExportClientData")]
    public static void ExportClientdata()
    {
        CleanSOCache();
        CleanGidTilesetCache();
        //加载Map
        TiledMap map = LoadMap(m_MapName);
        var globalGridSizeX = map.width;
        var globalGridSizeZ = map.height;
        //计算map分块大小
        int spiltMap_w = Mathf.CeilToInt(globalGridSizeX * 1f / m_MapSpiltSize_x);
        int spiltMap_h = Mathf.CeilToInt(globalGridSizeZ * 1f / m_MapSpiltSize_y);
        //物件分块数据
        Dictionary<string, TiledLayer> _layerDic = new Dictionary<string, TiledLayer>();
        for (int i = 0; i < map.layers.Length; i++)
        {
            var layer = map.layers[i];
            string layerName = layer.name;
            _layerDic.Add(layerName, layer);
        }

        var terrainTiles = _layerDic.ContainsKey(TerrainLayerName) ? _layerDic[TerrainLayerName].GetTiles() : null;
        var unRemoveTiles = _layerDic.ContainsKey(UnRemovableItemLayerName) ? _layerDic[UnRemovableItemLayerName].GetTiles() : null;
        // var removableTiles = _layerDic.ContainsKey(RemovableItemLayerName) ? _layerDic[RemovableItemLayerName].GetTiles() : null;
        StringBuilder sb_AllSet = new StringBuilder();
        StringBuilder[,] sb = new StringBuilder[m_MapSpiltSize_x, m_MapSpiltSize_y];
        int[,] unSetCount = new int[m_MapSpiltSize_x, m_MapSpiltSize_y];
        for (int i = 0; i < sb.GetLength(0); i++)
        {
            for (int j = 0; j < sb.GetLength(1); j++)
            {
                sb[i, j] = new StringBuilder();
                sb[i, j].AppendFormat("local TileMapData_{0}_{1}", i, j);
                sb[i, j].Append(" = {\n");
            }
        }
        sb_AllSet.Append("local TileMapData_AllSet = {\n");
        for (int i = 0; i < globalGridSizeX * globalGridSizeZ; i++)
        {
            int _pos_x = 0;
            int _pos_z = 0;
            bool isUnRemove = false;
            if (unRemoveTiles[i].Gid != 0)
            {
                _pos_x = globalGridSizeX - unRemoveTiles[i].X;
                _pos_z = globalGridSizeZ - unRemoveTiles[i].Y;
                isUnRemove = true;
            }
            else if (UnSetTerrain.Contains(terrainTiles[i].Gid))
            {
                _pos_x = globalGridSizeX - terrainTiles[i].X;
                _pos_z = globalGridSizeZ - terrainTiles[i].Y;
                isUnRemove = true;
            }
            if (isUnRemove)
            {
                int temp = _pos_x;
                _pos_x = _pos_z;
                _pos_z = temp;
                if (_pos_x == 164 && _pos_z == 600)
                {
                    Debug.Log("111");
                }
                int index_x = (_pos_x - 1) / spiltMap_w;
                int index_z = (_pos_z - 1) / spiltMap_h;
                int posIndex = _pos_x * 10000 + _pos_z;
                sb[index_x, index_z].AppendFormat("[{0}] = true,\n", posIndex);
                unSetCount[index_x, index_z]++;
            }
        }
        for (int i = 0; i < sb.GetLength(0); i++)
        {
            for (int j = 0; j < sb.GetLength(1); j++)
            {
                if (unSetCount[i, j] == 0)
                {
                    sb_AllSet.AppendFormat("[\"{0}_{1}\"] = 1,\n", i, j);
                    continue;
                }
                if (unSetCount[i, j] == spiltMap_w * spiltMap_h)
                {
                    sb_AllSet.AppendFormat("[\"{0}_{1}\"] = 2,\n", i, j);
                    continue;
                }

                sb[i, j].Append("};\n");
                sb[i, j].AppendFormat("return TileMapData_{0}_{1}", i, j);
                string path = string.Format("{0}/TileMapClientData/tilemapdata_{1}_{2}.lua", Application.dataPath, i, j);
                var bytes = System.Text.Encoding.UTF8.GetBytes(sb[i, j].ToString());
                Stream stream = File.Open(path, FileMode.Create);
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
        }
        sb_AllSet.Append("};\n");
        sb_AllSet.AppendFormat("return TileMapData_AllSet");
        string path_AllSet = string.Format("{0}/TileMapClientData/tilemapdata_allset.lua", Application.dataPath);
        var bytes_AllSet = System.Text.Encoding.UTF8.GetBytes(sb_AllSet.ToString());
        Stream stream_AllSet = File.Open(path_AllSet, FileMode.Create);
        stream_AllSet.Write(bytes_AllSet, 0, bytes_AllSet.Length);
        stream_AllSet.Close();
        CleanSOCache();
        CleanGidTilesetCache();
        AssetDatabase.Refresh();
        Debug.Log("ExportClientdata Finish");
    }

    private static TiledMap LoadMap(string assetName)
    {
        TiledMap map = TiledMap.ReadMap(assetName);
        List<TiledTileset> tiledTilesetList = new List<TiledTileset>(map.tilesets);
        tiledTilesetList.Sort((a, b) =>
        {
            if (a.firstgid == b.firstgid)
            {
                return 0;
            }
            else if (a.firstgid > b.firstgid)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        });
        map.tilesets = tiledTilesetList.ToArray();
        return map;
    }
    static Dictionary<int, int> m_GidTilesetIndexDic = new Dictionary<int, int>();
    static Dictionary<int, string> m_GidTilesetNameDic = new Dictionary<int, string>();
    private static void CleanGidTilesetCache()
    {
        m_GidTilesetIndexDic.Clear();
        m_GidTilesetNameDic.Clear();
    }
    private static string GetTilesetNameAndIndex(TiledMap map, int gid, out int index)
    {
        if (m_GidTilesetIndexDic.ContainsKey(gid))
        {
            index = m_GidTilesetIndexDic[gid];
            return m_GidTilesetNameDic[gid];
        }
        var tiledTilesetList = map.tilesets;
        string name = "";
        index = 0;
        if (gid <= 0)
        {
            return name;
        }
        for (int i = 0; i < tiledTilesetList.Length; i++)
        {
            if (tiledTilesetList[i].firstgid <= gid)
            {
                name = Path.GetFileNameWithoutExtension(tiledTilesetList[i].source);
                index = gid - tiledTilesetList[i].firstgid + 1;
            }
            else
            {
                m_GidTilesetIndexDic.Add(gid, index);
                m_GidTilesetNameDic.Add(gid, name);
                return name;
            }
        }
        m_GidTilesetIndexDic.Add(gid, index);
        m_GidTilesetNameDic.Add(gid, name);
        return name;
    }
    private static int GetRotY(TiledLayerTile tile)
    {
        if (tile.IsFlippedDiag && tile.IsFlippedHorz)
        {
            return 90;
        }
        else if (tile.IsFlippedDiag && tile.IsFlippedVert)
        {
            return -90;
        }
        else if (tile.IsFlippedVert && tile.IsFlippedHorz)
        {
            return 180;
        }
        return 0;
    }
    static Dictionary<string, ScriptableObject> m_SOCache = new Dictionary<string, ScriptableObject>();
    static T GetStriptableObject<T>(string path) where T : ScriptableObject
    {
        ScriptableObject asset;
        if (!m_SOCache.TryGetValue(path, out asset))
        {
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                UnityEditor.AssetDatabase.CreateAsset(asset, path);
            }
            m_SOCache.Add(path, asset);
        }
        return asset as T;
    }
    static void CleanSOCache()
    {
        SetDirtySOCache();
        m_SOCache.Clear();
        System.GC.Collect();
    }
    static void SetDirtySOCache()
    {
        foreach (var item in m_SOCache)
        {
            UnityEditor.EditorUtility.SetDirty(item.Value);
        }
    }
}
