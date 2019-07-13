using System.Collections.Generic;
using UnityEngine;
public class ItemMapInfo : ScriptableObject
{

    // public int mapIndex;
    public List<int> posIndex;
    public List<string> itemNameList;
    public Dictionary<int, string> mapInfoDic;
    public ItemMapInfo()
    {
        posIndex = new List<int>();
        itemNameList = new List<string>();
    }
    public void Init()
    {
        if (mapInfoDic != null)
        {
            return;
        }
        int count = posIndex.Count;
        mapInfoDic = new Dictionary<int, string>(count);
        for (int i = 0; i < count; i++)
        {
            mapInfoDic.Add(posIndex[i], itemNameList[i]);
        }
    }

}
// [System.Serializable]
// public struct ItemMapObjInfo
// {
//     //物件ID
//     public string itemName;
//     //物件旋转
//     // public int itemRotY;

// }