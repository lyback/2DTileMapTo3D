using System.Collections.Generic;
using System;
using UnityEngine;
public class AlphaTexInfo : ScriptableObject
{
    public List<int> posIndex;
    public List<AlphaTexInfo_S> terrainAlphaList;
    public Dictionary<int, AlphaTexInfo_S> terrainAlphaDic;
    public AlphaTexInfo()
    {
        posIndex = new List<int>();
        terrainAlphaList = new List<AlphaTexInfo_S>();
    }
    public void Init()
    {
        if (terrainAlphaDic != null)
        {
            return;
        }
        int count = posIndex.Count;
        terrainAlphaDic = new Dictionary<int, AlphaTexInfo_S>(count);
        for (int i = 0; i < count; i++)
        {
            terrainAlphaDic.Add(posIndex[i], terrainAlphaList[i]);
        }
    }
}
[Serializable]
public struct AlphaTexInfo_S
{
    public string objName;
    public uint level;
}