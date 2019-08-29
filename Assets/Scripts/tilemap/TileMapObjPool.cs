using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TileMapObjPool
{
    GameObject m_Prefab;
    Transform m_Parent;
    Queue<GameObject> m_ObjCache;

    Vector3 pos_temp;

    public TileMapObjPool(GameObject prefab, Transform parent, int count)
    {
        m_ObjCache = new Queue<GameObject>(count);
        m_Prefab = prefab;
        m_Parent = parent;
    }
    public GameObject Get(float x, float z)
    {
        GameObject obj;
        if (m_ObjCache.Count > 0)
        {
            obj = m_ObjCache.Dequeue();
            SetPos(obj, x, z, false);
            return obj;
        }
        obj = GameObject.Instantiate(m_Prefab);
        obj.transform.SetParent(m_Parent);
        SetPos(obj, x, z, true);
        return obj;
    }
    public void Recycle(GameObject obj)
    {
        SetOutSidePos(obj);
        m_ObjCache.Enqueue(obj);
    }

    void SetPos(GameObject obj, float x, float z, bool isFirst){
        pos_temp = obj.transform.localPosition;
        pos_temp.x = x;
        if (!isFirst)
        {
            pos_temp.y -= 1000;
        }
        pos_temp.z = z;
        obj.transform.localPosition = pos_temp;
    }
    void SetOutSidePos(GameObject obj){
        pos_temp = obj.transform.localPosition;
        pos_temp.y += 1000;
        obj.transform.localPosition = pos_temp;
    }
}