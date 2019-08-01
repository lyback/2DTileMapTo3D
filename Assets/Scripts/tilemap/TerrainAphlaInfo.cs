using System.Collections.Generic;
using UnityEngine;
public class TerrainAphlaInfo : ScriptableObject
{
    public List<int> posIndex;
    public List<string> terrainAphlaList;
    public Dictionary<int, string> terrainAphlaDic;
    public TerrainAphlaInfo()
    {
        posIndex = new List<int>();
        terrainAphlaList = new List<string>();
    }
    public void Init()
    {
        if (terrainAphlaDic != null)
        {
            return;
        }
        int count = posIndex.Count;
        terrainAphlaDic = new Dictionary<int, string>(count);
        for (int i = 0; i < count; i++)
        {
            terrainAphlaDic.Add(posIndex[i], terrainAphlaList[i]);
        }
    }

}