using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using TiledJsonUtility;

public class ExportMap : Editor
{
    static string m_MapName = "Map_KOW";
    static string TerrainLayerName = "Terrain";
    static string RemovableItemLayerName = "RemovableItem";
    static string UnRemovableItemLayerName = "UnRemovableItem";
    static Vector2Int m_MapSpiltSize = new Vector2Int(30, 30);
    [MenuItem("Map/Export")]
    public static void Export()
    {
        //加载Map
        TiledMap map = LoadMap(m_MapName);
        Dictionary<string, Dictionary<int, GameObject>> layerObjDic = GetObjRef(map);

        var globalGridSizeX = map.width;
        var globalGridSizeZ = map.height;

        //计算map分块大小,创建分块map配置
        int spiltMap_w = Mathf.CeilToInt(globalGridSizeX * 1f / m_MapSpiltSize.x);
        int spiltMap_h = Mathf.CeilToInt(globalGridSizeZ * 1f / m_MapSpiltSize.y);
        TileMapInfo[,] mapList = new TileMapInfo[m_MapSpiltSize.x, m_MapSpiltSize.y];
        for (int w = 0; w < m_MapSpiltSize.x; w++)
        {
            for (int h = 0; h < m_MapSpiltSize.y; h++)
            {
                mapList[w, h] = GetStriptableObject<TileMapInfo>(string.Format("Assets/Map/MapInfo_{0}_{1}.asset", w, h));
                mapList[w, h].mapIndex = h * spiltMap_w + w;
                mapList[w, h].posIndex = new List<int>();
                mapList[w, h].objInfoList = new List<TileMapObjInfo>();
            }
        }

        //创建地形obj配置
        TerrainInfo terrainInfo = GetStriptableObject<TerrainInfo>("Assets/Map/TerrainInfo.asset");
        var terrainObjDic = layerObjDic[TerrainLayerName];
        GameObject[] objList = new GameObject[terrainObjDic.Keys.Max() + 1];
        foreach (var kv in terrainObjDic)
        {
            objList[kv.Key] = kv.Value;
        }
        terrainInfo.gameObjectRef = objList.ToList();
        terrainInfo.MapSize = new Vector2Int(globalGridSizeX, globalGridSizeZ);
        terrainInfo.SpiltMapSize = new Vector2Int(spiltMap_w, spiltMap_h);
        UnityEditor.EditorUtility.SetDirty(terrainInfo);

        //地形分块数据
        for (int i = 0; i < map.layers.Length; i++)
        {
            var layer = map.layers[i];
            string layerName = layer.name;
            if (layerName == TerrainLayerName)
            {
                var tiles = layer.GetTiles();
                for (int j = 0; j < tiles.Length; j++)
                {
                    var tile = tiles[j];
                    int objID = tile.Gid;
                    int rY = GetRotY(tile);
                    if (objID != 0)
                    {
                        var _mapinfo = mapList[tile.X / spiltMap_w, tile.Y / spiltMap_h];
                        int _posIndex = tile.Y * globalGridSizeX + tile.X;
                        // int _index = _mapinfo.posIndex.FindIndex((a) => { return a == _posIndex; });
                        // if (_index >= 0)
                        // {
                        //     var objInfo = _mapinfo.objInfoList[_index];
                        //     objInfo.objIndex = objID;
                        //     objInfo.objRotY = rY;
                        //     _mapinfo.objInfoList[_index] = objInfo;
                        // }
                        // else
                        // {
                        //     var objInfo = new TileMapObjInfo();
                        //     objInfo.objIndex = objID;
                        //     objInfo.objRotY = rY;
                        //     _mapinfo.posIndex.Add(_posIndex);
                        //     _mapinfo.objInfoList.Add(objInfo);
                        // }

                        var objInfo = new TileMapObjInfo();
                        objInfo.objIndex = objID;
                        objInfo.objRotY = rY;
                        _mapinfo.posIndex.Add(_posIndex);
                        _mapinfo.objInfoList.Add(objInfo);

                        UnityEditor.EditorUtility.SetDirty(_mapinfo);
                    }
                }
            }
            else if(layerName == UnRemovableItemLayerName || layerName == RemovableItemLayerName){
                var tiles = layer.GetTiles();
                for (int j = 0; j < tiles.Length; j++)
                {
                    var tile = tiles[j];
                    int objID = tile.Gid;
                    int rY = GetRotY(tile);
                    if (objID != 0)
                    {
                        var _mapinfo = mapList[tile.X / spiltMap_w, tile.Y / spiltMap_h];
                        int _posIndex = tile.Y * globalGridSizeX + tile.X;
                        int _index = _mapinfo.posIndex.FindIndex((a) => { return a == _posIndex; });
                        if (_index >= 0)
                        {
                            var objInfo = _mapinfo.objInfoList[_index];
                            objInfo.objIndex = objID;
                            objInfo.itemRotY = rY;
                            _mapinfo.objInfoList[_index] = objInfo;
                        }
                        else
                        {
                            var objInfo = new TileMapObjInfo();
                            objInfo.objIndex = objID;
                            objInfo.itemRotY = rY;
                            _mapinfo.posIndex.Add(_posIndex);
                            _mapinfo.objInfoList.Add(objInfo);
                        }
                        UnityEditor.EditorUtility.SetDirty(_mapinfo);
                    }
                }
            }
        }

        UnityEditor.AssetDatabase.SaveAssets();
    }

    private static TiledMap LoadMap(string assetName)
    {
        TiledMap map = TiledMap.ReadMap(assetName);
        return map;
    }
    private static Dictionary<string, Dictionary<int, GameObject>> GetObjRef(TiledMap map)
    {
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

        var layerObjDic = new Dictionary<string, Dictionary<int, GameObject>>();
        foreach (TiledLayer layer in map.layers)
        {
            layerObjDic.Add(layer.name, new Dictionary<int, GameObject>());
            TiledLayerTile[] tiles = layer.GetTiles();
            if (tiles != null)
            {
                foreach (TiledLayerTile tile in tiles)
                {
                    // Debug.LogFormat("Tile({0},{1}) = {2} H={3} V={4} D={5}", tile.X, tile.Y, tile.Gid, tile.IsFlippedHorz, tile.IsFlippedVert, tile.IsFlippedDiag);
                    if (tile.Gid != 0 && !layerObjDic[layer.name].ContainsKey(tile.Gid))
                    {
                        int texIndex = 0;
                        int tilesetIndex = 0;
                        for (int i = 0; i < tiledTilesetList.Count; i++)
                        {
                            if (tiledTilesetList[i].firstgid > tile.Gid)
                            {
                                tilesetIndex = i - 1;
                                texIndex = tile.Gid - tiledTilesetList[i - 1].firstgid + 1;
                            }
                            else
                            {
                                texIndex = tile.Gid;
                            }
                        }
                        string objName = Path.GetFileNameWithoutExtension(tiledTilesetList[tilesetIndex].source) + "@" + texIndex;
                        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(string.Format("Assets/Art/TileMap/Prefab/{0}.prefab", objName));
                        if (obj == null)
                        {
                            Debug.LogError("找不到预制：" + string.Format("Assets/Art/TileMap/Prefab/{0}.prefab", objName));
                        }
                        else
                        {
                            layerObjDic[layer.name].Add(tile.Gid, obj);
                        }
                    }

                }
            }
        }
        return layerObjDic;
    }
    private static List<string> RemoveDuplicates(List<string> myList)
    {
        List<string> newList = new List<string>();

        for (int i = 0; i < myList.Count; i++)
            if (!newList.Contains(myList[i].ToString()))
                newList.Add(myList[i].ToString());

        return newList;
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
