using System.Collections.Generic;
using UnityEngine;
public class ItemMapInfo : ScriptableObject
{

    // public int mapIndex;
    public List<int> posIndex;
    public List<ItemMapObjInfo> objInfoList;
    public Dictionary<int, ItemMapObjInfo> mapInfoDic;
    public ItemMapInfo()
    {
        posIndex = new List<int>();
        objInfoList = new List<ItemMapObjInfo>();
    }
    public void Init()
    {
        if (mapInfoDic != null)
        {
            return;
        }
        int count = posIndex.Count;
        mapInfoDic = new Dictionary<int, ItemMapObjInfo>(count);
        for (int i = 0; i < count; i++)
        {
            mapInfoDic.Add(posIndex[i], objInfoList[i]);
        }
    }

}
[System.Serializable]
public struct ItemMapObjInfo
{
    //物件ID
    public string itemName;
    //物件旋转
    public int itemRotY;

}