using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SavePrefab {
	static string prefabPath = "Assets/Art/TileMap/Prefab/";
	[MenuItem("GameObject/CreatePrefab")]
	public static void CreatePrefab(){
		
		for (int i = 0; i < Selection.gameObjects.Length; i++)
		{
			var obj = Selection.gameObjects[i];
			PrefabUtility.CreatePrefab(string.Format("{0}{1}.prefab",prefabPath,obj.name),obj);
		}
	}
}
