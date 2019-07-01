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
    static int SeaID = 204;
    static Vector2Int m_MapSpiltSize = new Vector2Int(30, 30);

    [MenuItem("Map/ExportTerrain")]
    public static void ExportTerrain()
    {
        Directory.Delete(Application.dataPath+"/TileMap/Map/",true);
        //加载Map
        TiledMap map = LoadMap(m_MapName);

        var globalGridSizeX = map.width;
        var globalGridSizeZ = map.height;

        //计算map分块大小,创建分块map配置
        int spiltMap_w = Mathf.CeilToInt(globalGridSizeX * 1f / m_MapSpiltSize.x);
        int spiltMap_h = Mathf.CeilToInt(globalGridSizeZ * 1f / m_MapSpiltSize.y);

        //创建地形obj配置
        TerrainInfo terrainInfo = GetStriptableObject<TerrainInfo>("Assets/TileMap/TerrainInfo.asset");
        terrainInfo.MapSize = new Vector2Int(globalGridSizeX, globalGridSizeZ);
        terrainInfo.SpiltMapSize = new Vector2Int(spiltMap_w, spiltMap_h);
        UnityEditor.EditorUtility.SetDirty(terrainInfo);

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
            for (int j = 0; j < terrainTiles.Length; j++)
            {
                var _terrTile = terrainTiles[j];
                int _terrRY = GetRotY(_terrTile);
                int terrainID = _terrTile.Gid;
                GetTilesetNameAndIndex(map, _terrTile.Gid, out terrainID);

                if (terrainID != 0 && terrainID != SeaID)
                {
                    var _mapinfo = GetStriptableObject<TileMapInfo>(string.Format("Assets/TileMap/Map/MapInfo_{0}_{1}.asset", _terrTile.X / spiltMap_w, _terrTile.Y / spiltMap_h));
                    int _posIndex = _terrTile.Y * globalGridSizeX + _terrTile.X;

                    var objInfo = new TileMapObjInfo();
                    objInfo.terrainIndex = terrainID == SeaID ? 0 : terrainID;
                    objInfo.terrainRotY = _terrRY;

                    _mapinfo.posIndex.Add(_posIndex);
                    _mapinfo.objInfoList.Add(objInfo);

                    UnityEditor.EditorUtility.SetDirty(_mapinfo);
                }
            }
        }

        UnityEditor.AssetDatabase.SaveAssets();
    }
    [MenuItem("Map/ExportObj")]
    public static void ExportObj()
    {
        Directory.Delete(Application.dataPath+"/TileMap/Item/",true);
        //加载Map
        TiledMap map = LoadMap(m_MapName);
        var globalGridSizeX = map.width;
        var globalGridSizeZ = map.height;
        //计算map分块大小
        int spiltMap_w = Mathf.CeilToInt(globalGridSizeX * 1f / m_MapSpiltSize.x);
        int spiltMap_h = Mathf.CeilToInt(globalGridSizeZ * 1f / m_MapSpiltSize.y);

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
        for (int i = 0; i < globalGridSizeX * globalGridSizeZ; i++)
        {
            TiledLayerTile _unRemoveTile = new TiledLayerTile();
            int _unRemoveID = 0;
            int _unRemoveRY = 0;
            string _unRemoveName = "";
            if (unRemoveTiles != null)
            {
                _unRemoveTile = unRemoveTiles[i];
                _unRemoveRY = GetRotY(_unRemoveTile);
                _unRemoveID = _unRemoveTile.Gid;
                _unRemoveName = GetTilesetNameAndIndex(map, _unRemoveTile.Gid, out _unRemoveID);
            }
            TiledLayerTile _removableTile = new TiledLayerTile();
            int _removableID = 0;
            int _removableRY = 0;
            string _removableName = "";
            if (removableTiles != null)
            {
                _removableTile = removableTiles[i];
                _removableRY = GetRotY(_removableTile);
                _removableID = _removableTile.Gid;
                _removableName = GetTilesetNameAndIndex(map, _removableTile.Gid, out _removableID);
            }
            if (_unRemoveID == 1 || _removableID == 1)
            {
                int X = _unRemoveID != 0 ? _unRemoveTile.X : _removableTile.X;
                int Y = _unRemoveID != 0 ? _unRemoveTile.Y : _removableTile.Y;

                var _mapinfo = GetStriptableObject<ItemMapInfo>(string.Format("Assets/TileMap/Item/ItemInfo_{0}_{1}.asset", X / spiltMap_w, Y / spiltMap_h));
                int _posIndex = Y * globalGridSizeX + X;

                var objInfo = new ItemMapObjInfo();
                objInfo.itemName = _unRemoveID == 1 ? _unRemoveName : _removableName;
                objInfo.itemRotY = _unRemoveID == 1 ? _unRemoveRY : _removableRY;

                _mapinfo.posIndex.Add(_posIndex);
                _mapinfo.objInfoList.Add(objInfo);

                UnityEditor.EditorUtility.SetDirty(_mapinfo);
            }
        }

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
        var removableTiles = _layerDic.ContainsKey(RemovableItemLayerName) ? _layerDic[RemovableItemLayerName].GetTiles() : null;
        StringBuilder sb = new StringBuilder();
        sb.Append("local kingdomMapData = {\n");
        for (int i = 0; i < globalGridSizeX * globalGridSizeZ; i++)
        {
            int _pos_x = 0;
            int _pos_z = 0;
            bool isUnRemove = false;
            if(unRemoveTiles[i].Gid != 0)
            {
                _pos_x = globalGridSizeX - unRemoveTiles[i].X;
                _pos_z = globalGridSizeZ - unRemoveTiles[i].Y;
                isUnRemove = true;
            }
            else if (terrainTiles[i].Gid == SeaID)
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
                int posIndex = _pos_x *10000+_pos_z;
                // sb.Append(posIndex+",");
                sb.AppendFormat("[{0}] = 2,\n",posIndex);
            }
        }
        sb.Append("};return kingdomMapData");

        string path = string.Format("{0}/TileMapSerData/kingdomMapData.lua",Application.dataPath);
        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        Stream stream = File.Open (path, FileMode.Create);
        stream.Write (bytes, 0, bytes.Length);
        stream.Close ();
		
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
    private static string GetTilesetNameAndIndex(TiledMap map, int gid, out int index)
    {
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
                return name;
            }
        }
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
    static T GetStriptableObject<T>(string path) where T : ScriptableObject
    {
        T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            UnityEditor.AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }
}
