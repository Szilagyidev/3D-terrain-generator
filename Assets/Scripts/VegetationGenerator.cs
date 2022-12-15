using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VegetationGenerator : MonoBehaviour
{
	public MapGenerator mapGenerator;
    [SerializeField] GameObject prefab;

    [Header("Raycast Settings")]
    [SerializeField] int density;

    [Space]

    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;

    [Header("Prefab Variation Settings")]
    [SerializeField, Range(0, 1)] float rotateTowardsNormal;
    [SerializeField] Vector2 rotationRange;
    [SerializeField] Vector3 minScale;
    [SerializeField] Vector3 maxScale;
	[System.NonSerialized] public int scale = 1;

#if UNITY_EDITOR
	public void Generate() { 
		Clear();

		for (int i = 0; i < density + (scale*10); i++) {
			float sampleX = Random.Range(-mapGenerator.mapChunkSize - 50 - scale, mapGenerator.mapChunkSize + 50 + scale);
			float sampleY = Random.Range(-mapGenerator.mapChunkSize - 50 - scale, mapGenerator.mapChunkSize + 50 + scale);
			Vector3 rayStart = new Vector3(sampleX, maxHeight, sampleY);

			if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity))
				continue;

			if (hit.point.y < minHeight)
				continue;

			GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(this.prefab, transform);
			instantiatedPrefab.transform.position = hit.point;
			instantiatedPrefab.transform.Rotate(Vector3.up, Random.Range(rotationRange.x, rotationRange.y), Space.Self);
			instantiatedPrefab.transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation * Quaternion.FromToRotation(instantiatedPrefab.transform.up, hit.normal), rotateTowardsNormal);
			instantiatedPrefab.transform.localScale = new Vector3(
				Random.Range(minScale.x, maxScale.x),
				Random.Range(minScale.y, maxScale.y),
				Random.Range(minScale.z, maxScale.z)
			);
		}
	}

	public void Clear() {
		while (transform.childCount != 0) {
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
	}
#endif
}