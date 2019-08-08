using System.Collections.Generic;
using UnityEngine;
public class AlphaTexInfo : ScriptableObject
{
    public List<int> posIndex;
    public List<string> terrainAlphaList;
    public Dictionary<int, string> terrainAlphaDic;
    public AlphaTexInfo()
    {
        posIndex = new List<int>();
        terrainAlphaList = new List<string>();
    }
    public void Init()
    {
        if (terrainAlphaDic != null)
        {
            return;
        }
        int count = posIndex.Count;
        terrainAlphaDic = new Dictionary<int, string>(count);
        for (int i = 0; i < count; i++)
        {
            terrainAlphaDic.Add(posIndex[i], terrainAlphaList[i]);
        }
    }

}