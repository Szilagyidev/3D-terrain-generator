using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GenerateWater : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    public MapGenerator mapGenerator;
    public float waterLevel;
    public Vector3 posForWater;

#if UNITY_EDITOR
    public void GenerateWaterForTerrain(){
        if(mapGenerator.GenerateWater == true){
            Clear();
            GameObject current = (GameObject)PrefabUtility.InstantiatePrefab(this.prefab, transform);
            current.transform.position = new Vector3(0, waterLevel, 0);
            current.transform.localScale = posForWater;
        }else{
           	Clear();
		    }
        }

    public void Clear() {
		while (transform.childCount != 0) {
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
	}

#endif
}
