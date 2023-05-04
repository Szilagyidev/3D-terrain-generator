using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;
    public Noise.NormalizeMode normalizeMode;
    public RidgedNoise.NormalizeMode ridgedNormalizeMode;

    public TerrainData terrainData;
    public TerrainWorleyData terrainWorleyData;
    public TerrainDiamondData terrainDiamondData;
    public TerrainRidgedPerlinData terrainRidgedPerlin;

    public NoiseData noiseData;
    public WorleyData worleyData;
    public DiamondData diamondData;
    public RidgedPerlinData ridgedPerlinData;

    public TextureData textureData;
    public Material terrainMaterial;
    public GenerateWater generateWater;
    public VegetationGenerator vegetationGenerator;

    [Range(0, MeshGenerator.numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;

    [Range(0, MeshGenerator.numSupportedChunkSizesforDiamond - 1)]
    public int chunkSizeIndexforDiamond;

    [Range(0, MeshGenerator.numSupportedLODs - 1)]
    public int editorPreviewLOD;
    public bool autoUpdate;
    public bool GenerateVegetation;
    public bool GenerateWater;
    public float[,] fallOffMap;

    public string currentNoise = "Perlin";

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    void Awake()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        textureData.UpdateMeshHeights(terrainMaterial, terrainRidgedPerlin.minHeight, terrainRidgedPerlin.maxHeight * diamondData.colourDivider);
        textureData.UpdateMeshHeights(terrainMaterial, terrainDiamondData.minHeight, terrainDiamondData.maxHeight / diamondData.colourDivider);
        textureData.UpdateMeshHeights(terrainMaterial, terrainWorleyData.minHeight, terrainWorleyData.maxHeight / worleyData.colourDivider);
        textureData.ApplyToMaterial(terrainMaterial);
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            textureData.ApplyToMaterial(terrainMaterial);
            if (currentNoise == "Perlin") { DrawMapInEditorForPerlin(); }
            if (currentNoise == "Diamond") { DrawMapInEditorForDiamond(); }
            if (currentNoise == "RidgedPerlin") { DrawMapInEditorForRidgedPerlin(); }
            if (currentNoise == "Worley") { DrawMapInEditorForWorley(); }
        }
    }
    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    public int mapChunkSize
    {
        get
        {
            return MeshGenerator.supportedChunkSizes[chunkSizeIndex] - 1;
        }
    }

    public int mapChunkSizeDiamond
    {
        get
        {
            return MeshGenerator.supportedChunkSizesforDiamond[chunkSizeIndexforDiamond];
        }
    }

    public void DrawMapInEditorForPerlin()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        MapData mapData = GenerateMapDataForPerlin(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            GenerateWater = false;
            GenerateVegetation = false;
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMeshForPerlin(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            GenerateWater = false;
            GenerateVegetation = false;
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
        generateWater.GenerateWaterForTerrain();
        vegetationGenerator.Generate();
    }

    public void DrawMapInEditorForRidgedPerlin()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainRidgedPerlin.minHeight, terrainRidgedPerlin.maxHeight * 0.42f);
        MapData mapData = GenerateMapDataForRidgedPerlin(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            GenerateWater = false;
            GenerateVegetation = false;
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMeshForPerlin(mapData.heightMap, terrainRidgedPerlin.meshHeightMultiplier, terrainRidgedPerlin.meshHeightCurve, editorPreviewLOD));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            GenerateWater = false;
            GenerateVegetation = false;
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
        generateWater.GenerateWaterForTerrain();
        vegetationGenerator.Generate();
    }

    public void DrawMapInEditorForDiamond()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainDiamondData.minHeight, terrainDiamondData.maxHeight / diamondData.colourDivider);
        MapData mapData = GenerateMapDataForDiamond();
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            GenerateWater = false;
            GenerateVegetation = false;
            display.DrawTexture(TextureGenerator.TextureFromHeightMapForDiamond(mapData.heightMap, diamondData.colourDivider * 5));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMeshForDiamond(mapData.heightMap, terrainDiamondData.meshHeightMultiplier, editorPreviewLOD));
        }
        generateWater.GenerateWaterForTerrain();
        vegetationGenerator.Generate();
    }
    public void DrawMapInEditorForWorley()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainWorleyData.minHeight, terrainWorleyData.maxHeight / worleyData.colourDivider);
        MapData mapData = GenerateMapDataForWolrey();
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            GenerateWater = false;
            GenerateVegetation = false;
            display.DrawTexture(TextureGenerator.TextureFromHeightMapForWorley(mapData.heightMap, worleyData.colourDivider * 5000));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            //GenerateForDiamond is the method name but does the same as worley
            display.DrawMesh(MeshGenerator.GenerateTerrainMeshForDiamond(mapData.heightMap, terrainWorleyData.meshHeightMultiplier, editorPreviewLOD));
        }
        generateWater.GenerateWaterForTerrain();
        vegetationGenerator.Generate();
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapDataForPerlin(centre);
        MapData mapDataRidged = GenerateMapDataForRidgedPerlin(centre);
        lock (mapDataThreadInfoQueue)
        {
            if (currentNoise == "Perlin") { mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData)); }
            if (currentNoise == "RidgedPerlin") { mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapDataRidged)); }
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMeshForPerlin(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod);
        MeshData meshDataRidged = MeshGenerator.GenerateTerrainMeshForPerlin(mapData.heightMap, terrainRidgedPerlin.meshHeightMultiplier, terrainRidgedPerlin.meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            if (currentNoise == "Perlin") { meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData)); }
            if (currentNoise == "RidgedPerlin") { meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshDataRidged)); }
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapDataForPerlin(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.perlinseed, noiseData.noiseScale,
        noiseData.octaves, noiseData.presistance, noiseData.lacunarity, centre + noiseData.offset, normalizeMode);

        if (terrainData.useFalloff)
        {

            if (fallOffMap == null)
            {
                fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
            }

            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                {
                    if (terrainData.useFalloff)
                    {
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallOffMap[x, y]);
                    }
                }
            }
        }
        generateWater.posForWater = new Vector3(mapChunkSize / 4 + 0.5f, mapChunkSize / 4 + 0.5f, mapChunkSize / 4 + 0.5f);
        generateWater.waterLevel = 10;
        return new MapData(noiseMap);
    }

    MapData GenerateMapDataForRidgedPerlin(Vector2 centre)
    {
        float[,] ridgednoiseMap = RidgedNoise.GenerateRidgedNoiseMap(mapChunkSize + 2, mapChunkSize + 2, ridgedPerlinData.perlinseed, ridgedPerlinData.noiseScale,
        ridgedPerlinData.octaves, ridgedPerlinData.presistance, ridgedPerlinData.lacunarity, centre + ridgedPerlinData.offset, ridgedPerlinData.inverton, ridgedNormalizeMode);
        if (terrainRidgedPerlin.useFalloff)
        {

            if (fallOffMap == null)
            {
                fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
            }

            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                {
                    if (terrainRidgedPerlin.useFalloff)
                    {
                        ridgednoiseMap[x, y] = Mathf.Clamp01(ridgednoiseMap[x, y] - fallOffMap[x, y]);
                    }
                }
            }
        }
        generateWater.posForWater = new Vector3(mapChunkSize / 4 + 0.5f, mapChunkSize / 4 + 0.5f, mapChunkSize / 4 + 0.5f);
        generateWater.waterLevel = 20;
        return new MapData(ridgednoiseMap);
    }

    MapData GenerateMapDataForDiamond()
    {
        float[,] diamondsquareMap = DiamondSquareAlgorithm.GenerateDiamondSquareMap(mapChunkSizeDiamond, diamondData);
        if (terrainDiamondData.useFalloff)
        {

            if (fallOffMap == null)
            {
                fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSizeDiamond);
            }

            for (int y = 0; y < mapChunkSizeDiamond; y++)
            {
                for (int x = 0; x < mapChunkSizeDiamond; x++)
                {
                    if (terrainDiamondData.useFalloff)
                    {
                        //diamondsquareMap[x,y] = Mathf.Clamp01(diamondsquareMap[x,y] - fallOffMap[x,y]);
                    }
                }
            }
        }
        generateWater.posForWater = new Vector3(mapChunkSizeDiamond / 4 + 0.5f, mapChunkSizeDiamond / 4 + 0.5f, mapChunkSizeDiamond / 4 + 0.5f);
        generateWater.waterLevel = 20;
        return new MapData(diamondsquareMap);
    }

    MapData GenerateMapDataForWolrey()
    {
        float[,] worleyMap = WorleyNoise.GenerateWorleyMap(mapChunkSize, mapChunkSize, worleyData);

        if (terrainWorleyData.useFalloff)
        {

            if (fallOffMap == null)
            {
                fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
            }

            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    if (terrainWorleyData.useFalloff)
                    {
                        //worleyMap[x,y] = Mathf.Clamp01(worleyMap[x,y] - fallOffMap[x,y]);
                    }
                }
            }
        }
        generateWater.posForWater = new Vector3(mapChunkSize / 4 + 0.5f, mapChunkSize / 4 + 0.5f, mapChunkSize / 4 + 0.5f);
        generateWater.waterLevel = 20;
        return new MapData(worleyMap);
    }
    void OnValidate()
    {
        if (terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if (worleyData != null)
        {
            worleyData.OnValuesUpdated -= OnValuesUpdated;
            worleyData.OnValuesUpdated += OnValuesUpdated;
        }
        if (terrainWorleyData != null)
        {
            terrainWorleyData.OnValuesUpdated -= OnValuesUpdated;
            terrainWorleyData.OnValuesUpdated += OnValuesUpdated;
        }
        if (diamondData != null)
        {
            diamondData.OnValuesUpdated -= OnValuesUpdated;
            diamondData.OnValuesUpdated += OnValuesUpdated;
        }
        if (terrainDiamondData != null)
        {
            terrainDiamondData.OnValuesUpdated -= OnValuesUpdated;
            terrainDiamondData.OnValuesUpdated += OnValuesUpdated;
        }
        if (ridgedPerlinData != null)
        {
            ridgedPerlinData.OnValuesUpdated -= OnValuesUpdated;
            ridgedPerlinData.OnValuesUpdated += OnValuesUpdated;
        }
        if (terrainRidgedPerlin != null)
        {
            terrainRidgedPerlin.OnValuesUpdated -= OnValuesUpdated;
            terrainRidgedPerlin.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }
}
public struct MapData
{
    public readonly float[,] heightMap;
    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}
