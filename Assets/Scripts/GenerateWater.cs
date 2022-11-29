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
    private float waterLevel;

#if UNITY_EDITOR
    public void GenerateWaterForTerrain(){
        Clear();

        Vector3 posForWater = new Vector3(mapGenerator.mapChunkSize / 4, mapGenerator.mapChunkSize / 4, mapGenerator.mapChunkSize / 4);
        
        if(mapGenerator.currentNoise == "Perlin"){
            waterLevel = 10;
        }
        if(mapGenerator.currentNoise == "RidgedPerlin"){
            waterLevel = 20;
        }
        if(mapGenerator.currentNoise =="Diamond"){
            waterLevel = 20;
            posForWater = new Vector3(mapGenerator.mapChunkSizeDiamond / 4, mapGenerator.mapChunkSizeDiamond / 4, mapGenerator.mapChunkSizeDiamond / 4);
        }
        if(mapGenerator.currentNoise =="Worley"){
            waterLevel = 20;
        }

        if(mapGenerator.GenerateWater == true){
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
