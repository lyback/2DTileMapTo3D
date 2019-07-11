using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TerrainInfo : ScriptableObject
{
    public int MapSize_W;
    public int MapSize_H;
    public int SpiltMapSize_W;
    public int SpiltMapSize_H;
    public int SpiltMap_X;
    public int SpiltMap_Y;
    public bool[] MapInfoList;
    public bool[] ItemInfoList;
}