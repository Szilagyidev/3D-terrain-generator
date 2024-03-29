using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrviewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] detailLevels;
    public static float maxViewDst;

    public Transform viewer;
    public Material[] mapMaterials;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunkVisibleInViewDst;
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    [SerializeField] GameObject prefab;
    public float heatThresholdScale = 20.0f;
    public bool useHeatMap;
    public HeatTerrainTypes[] heatTerrainTypes;

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        chunkSize = mapGenerator.mapChunkSize - 1; //241 - 1
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        UpdateVisibleChunks();

        if (mapGenerator.currentNoise == "Perlin") { mapGenerator.DrawMapInEditorForPerlin(); }
        if (mapGenerator.currentNoise == "RidgedPerlin") { mapGenerator.DrawMapInEditorForRidgedPerlin(); }

        if (mapGenerator.GenerateVegetation == true)
        {
            mapGenerator.vegetationGenerator.terrainScale = 5;
            mapGenerator.vegetationGenerator.Generate();
        }
        else
        {
            mapGenerator.vegetationGenerator.Clear();
        }
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformscale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrviewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }

    }

    void UpdateVisibleChunks()
    {

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCord))
                {
                    terrainChunkDictionary[viewedChunkCord].UpdateTerrainChunk();
                }
                else
                {
                    if(useHeatMap == true){
                        terrainChunkDictionary.Add(viewedChunkCord, new TerrainChunk(viewedChunkCord, chunkSize, detailLevels, transform, ApplyMaterialByTreshold(viewedChunkCord), prefab));
                    } else if(useHeatMap == false){
                        terrainChunkDictionary.Add(viewedChunkCord, new TerrainChunk(viewedChunkCord, chunkSize, detailLevels, transform, mapMaterials[2], prefab));
                    }
                    
                }
            }
        }
    }

    //this method generates in a cirlce pattern
    Material ApplyMaterialByTreshold(Vector2 viewedChunkCord)
    {
        float vecMagnitude = viewedChunkCord.magnitude / heatThresholdScale;
        int i = 0;
        while (vecMagnitude >= heatTerrainTypes[i].threshold)
        {
            i++;
            if (i >= heatTerrainTypes.Length)
            {
                i = 0;
                vecMagnitude = vecMagnitude % heatTerrainTypes[heatTerrainTypes.Length - 1].threshold; //ensure that it remains within the range of the thresholds.
            }
        }
        heatTerrainTypes[i].textureData.UpdateMeshHeights(mapMaterials[i], mapGenerator.terrainData.minHeight, mapGenerator.terrainData.maxHeight);
        heatTerrainTypes[i].textureData.ApplyToMaterial(mapMaterials[i]);
        return mapMaterials[i];
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;
        MeshCollider meshCollider;
        GameObject waterPrefab;
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material, GameObject prefab)
        {
            this.detailLevels = detailLevels;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformscale;
            meshObject.transform.parent = parent;
            meshRenderer.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformscale;

            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.mesh;

            if (mapGenerator.GenerateWater == true)
            {
                Vector3 scaleForWater = new Vector3(mapGenerator.mapChunkSize / 4 + 0.5f, mapGenerator.mapChunkSize / 4 + 0.5f, mapGenerator.mapChunkSize / 4 + 0.5f);
                waterPrefab = Instantiate(prefab, positionV3 * mapGenerator.terrainData.uniformscale, Quaternion.identity);
                waterPrefab.transform.position = new Vector3(waterPrefab.transform.position.x, 10, waterPrefab.transform.position.z);
                waterPrefab.transform.localScale = scaleForWater;
                waterPrefab.transform.parent = parent;
            }

            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= maxViewDst;

                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                            meshCollider.sharedMesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }
                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
            if (mapGenerator.GenerateWater == true) { waterPrefab.SetActive(visible); }
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }

    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataRecieved(MeshData meshData)
        {
            mesh = meshData.CreateMesh();

            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecieved);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        [Range(0, MeshGenerator.numSupportedLODs - 1)]
        public int lod;
        public float visibleDstThreshold;
    }

}

[System.Serializable]
public struct HeatTerrainTypes
{
    public string name;
    public float threshold;
    public TextureData textureData;
}