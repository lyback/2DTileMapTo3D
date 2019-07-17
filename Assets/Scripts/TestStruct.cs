using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStruct : MonoBehaviour
{

    teststruct[] m_teststruct = new teststruct[100000];
    // Use this for initialization
    void Start()
    {
		int x = m_teststruct.GetLength(0);
		int y = m_teststruct.GetLength(1);
		for (int i = 0; i < x; i++)
		{
			for (int j = 0; j < y; j++)
			{
				// m_teststruct[i,j].a = 1;
				// m_teststruct[i,j].b = 1;
				// m_teststruct[i,j].s = "1";
				// m_teststruct[i,j].a = 1;
				// m_teststruct[i,j].b = 1;
				// m_teststruct[i,j].s = "1";
				// m_teststruct[i,j].a = 1;
				// m_teststruct[i,j].b = 1;
				// m_teststruct[i,j].s = "1";
				// m_teststruct[i,j].m00 = 1;
				// m_teststruct[i,j].m01 = 1;
				// m_teststruct[i,j].m02 = 1;
				// m_teststruct[i,j].m03 = 1;
				// m_teststruct[i,j].m10 = 1;
				// m_teststruct[i,j].m20 = 1;
			}
		}
		for (int i = 0; i < x; i++)
		{
			for (int j = 0; j < y; j++)
			{
				// var obj = m_teststruct[i,j];
				// obj.m00 = 1;
				// obj.m01 = 1;
				// obj.m02 = 1;
				// obj.m03 = 1;
				// obj.m10 = 1;
				// obj.m20 = 1;
				// m_teststruct[i,j] = obj;
			}
		}
		// for (int i = 0; i < x; i++)
		// {
		// 	for (int j = 0; j < y; j++)
		// 	{
		// 		m_teststruct[i,j].Set(1,1,"1");
		// 	}
		// }
    }

    // Update is called once per frame
    void Update()
    {

    }
    struct teststruct
    {
        public int a;
        public int b;
        public string s;

        public void Set(int _a, int _b, string _s)
        {
            a = _a;
            b = _b;
            s = _s;
        }
    }
}
