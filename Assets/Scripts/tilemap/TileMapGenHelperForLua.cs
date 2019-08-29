using UnityEngine;
public class TileMapGenHelperForLua
{
    static TileMapGenBase m_TileMapGen;
    public static bool m_IsInit = false;
    public static void Init(Transform terrainRoot, object terrainInfo, int v_count, int h_count, int itemEx_x, int itemEx_y, float offset_x, float offset_y){
        m_TileMapGen = terrainRoot.GetComponent<TileMapGenBase>();
        if (m_TileMapGen)
        {
            var data = terrainInfo as TerrainInfo;
            m_TileMapGen.Init(data, v_count, h_count, itemEx_x, itemEx_y, offset_x, offset_y);
        }
        m_IsInit = true;
    }
    public static void Dispose(){
        m_IsInit = false;
    }
    public static void SetResPath(bool fullPath, string tileMapInfoPath, string tileMapInfoName,
     string itemInfoPath, string itemInfoName,
     string alphaTexPath, string alphaTexName,
     string terrainObjPath, string terrainObjName,
     string itemObjPath,
     string alphaTexObjPath){
        m_TileMapGen.SetResPath(fullPath, tileMapInfoPath, tileMapInfoName, itemInfoPath, itemInfoName, alphaTexPath, alphaTexName, terrainObjPath, terrainObjName, itemObjPath, alphaTexObjPath);
    }
    public static void MoveTo(float x, float z){
        m_TileMapGen.MoveTo(x, z);
    }
    public static void HideItemAtPos(int x, int z){
        m_TileMapGen.HideItemAtPos(x,z);
    }
    public static void ShowItemAtPos(int x, int z){
        m_TileMapGen.ShowItemAtPos(x,z);
    }
    public static void SetForceMoveTo(){
        m_TileMapGen.SetForceMoveTo();
    }
}
