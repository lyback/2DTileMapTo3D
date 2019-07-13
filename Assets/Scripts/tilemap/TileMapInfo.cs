using System.Collections.Generic;
using UnityEngine;

public class TileMapInfo : ScriptableObject
{

    // public int mapIndex;
    public List<int> posIndex;
    public List<int> terrainIndexList;
    public Dictionary<int, int> mapInfoDic;
    public TileMapInfo()
    {
        posIndex = new List<int>();
        terrainIndexList = new List<int>();
    }
    public void Init()
    {
        if (mapInfoDic != null)
        {
            return;
        }
        int count = posIndex.Count;
        mapInfoDic = new Dictionary<int, int>(count);
        for (int i = 0; i < count; i++)
        {
            mapInfoDic.Add(posIndex[i], terrainIndexList[i]);
        }
    }
}
