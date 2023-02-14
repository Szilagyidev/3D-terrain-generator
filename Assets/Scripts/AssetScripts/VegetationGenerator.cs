using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VegetationGenerator : MonoBehaviour
{
    public MapGenerator mapGenerator;
    [SerializeField] GameObject[] prefabs;
    [SerializeField] PrefabArguments[] prefabArguments; // Array of prefab-specific arguments
    public int terrainScale = 1;

    public void Generate()
    {
        Clear();

        for (int i = 0; i < prefabs.Length; i++)
        {
            GameObject prefab = prefabs[i];
            PrefabArguments arguments = prefabArguments[i];

            for (int j = 0; j < arguments.density; j++)
            {
                float sampleX = Random.Range((-mapGenerator.mapChunkSize - 50) * terrainScale, (mapGenerator.mapChunkSize + 50) * terrainScale);
                float sampleY = Random.Range((-mapGenerator.mapChunkSize - 50) * terrainScale, (mapGenerator.mapChunkSize + 50) * terrainScale);
                Vector3 rayStart = new Vector3(sampleX, arguments.maxHeight, sampleY);

                if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity))
                    continue;

                if (hit.point.y < arguments.minHeight)
                    continue;

                GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
                instantiatedPrefab.transform.position = hit.point;
                instantiatedPrefab.transform.Rotate(Vector3.up, Random.Range(arguments.rotationRange.x, arguments.rotationRange.y), Space.Self);
                instantiatedPrefab.transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation * Quaternion.FromToRotation(instantiatedPrefab.transform.up, hit.normal), arguments.rotateTowardsNormal);
                instantiatedPrefab.transform.localScale = new Vector3(
                    Random.Range(arguments.minScale.x, arguments.maxScale.x),
                    Random.Range(arguments.minScale.y, arguments.maxScale.y),
                    Random.Range(arguments.minScale.z, arguments.maxScale.z)
                );
            }
        }
        if (mapGenerator.GenerateVegetation == false) { Clear(); }
    }
    public void Clear()
    {
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

}

[System.Serializable]
public struct PrefabArguments
{
    public Vector3 minScale;
    public Vector3 maxScale;
    public Vector2 rotationRange;
    public float rotateTowardsNormal;
    public int density;
    public float minHeight;
    public float maxHeight;
}