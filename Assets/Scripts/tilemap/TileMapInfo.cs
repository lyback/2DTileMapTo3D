using System.Collections.Generic;
using UnityEngine;

public class TileMapInfo : ScriptableObject
{

    // public int mapIndex;
    public List<int> posIndex;
    public List<TileMapObjInfo> objInfoList;
    public Dictionary<int, TileMapObjInfo> mapInfoDic;
    public TileMapInfo()
    {
        posIndex = new List<int>();
        objInfoList = new List<TileMapObjInfo>();
    }
    public void Init()
    {
        if (mapInfoDic != null)
        {
            return;
        }
        int count = posIndex.Count;
        mapInfoDic = new Dictionary<int, TileMapObjInfo>(count);
        for (int i = 0; i < count; i++)
        {
            mapInfoDic.Add(posIndex[i], objInfoList[i]);
        }
    }
}
[System.Serializable]
public struct TileMapObjInfo
{
    //地块ID
    public int terrainIndex;
    //地块旋转
    public int terrainRotY;
}