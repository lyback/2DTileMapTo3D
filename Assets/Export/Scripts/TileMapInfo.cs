using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapInfo : ScriptableObject
{

    public int mapIndex;
    public List<int> posIndex;
    public List<TileMapObjInfo> objInfoList;
    public Dictionary<int, TileMapObjInfo> mapInfoDic;
    public void Init()
    {
		if (mapInfoDic != null)
		{
			return;
		}
		mapInfoDic = new Dictionary<int, TileMapObjInfo>();
        for (int i = 0; i < posIndex.Count; i++)
        {
			mapInfoDic.Add(posIndex[i],objInfoList[i]);
        }
    }
}
[System.Serializable]
public struct TileMapObjInfo
{
    //地块ID
    public int objIndex;
    //地块旋转
    public int objRotY;
    //物件ID
    public int itemIndex;
    //物件旋转
    public int itemRotY;
}