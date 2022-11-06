using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

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

    public const int mapChunkSize = 241;  // a level of detailre jó a 241. a diamond square az 256 vagy 128
    [Range(0, 6)]
    public int editorPreviewLOD;
    public bool autoUpdate;
    public float[,] fallOffMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    void Awake()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        textureData.UpdateMeshHeights(terrainMaterial, terrainRidgedPerlin.minHeight, terrainRidgedPerlin.maxHeight);
        textureData.UpdateMeshHeights(terrainMaterial, terrainDiamondData.minHeight, terrainDiamondData.maxHeight / diamondData.colourDivider);
        textureData.UpdateMeshHeights(terrainMaterial, terrainWorleyData.minHeight, terrainWorleyData.maxHeight / worleyData.colourDivider);
    }

    [System.NonSerialized]
    public string currentNoise;

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
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

    public void DrawMapInEditorForPerlin()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        MapData mapData = GenerateMapDataForPerlin(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMeshForPerlin(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
    }

    public void DrawMapInEditorForRidgedPerlin()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainRidgedPerlin.minHeight, terrainRidgedPerlin.maxHeight);
        MapData mapData = GenerateMapDataForRidgedPerlin();
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMeshForPerlin(mapData.heightMap, terrainRidgedPerlin.meshHeightMultiplier, terrainRidgedPerlin.meshHeightCurve, editorPreviewLOD));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
    }

    public void DrawMapInEditorForDiamond()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainDiamondData.minHeight, terrainDiamondData.maxHeight / diamondData.colourDivider);
        MapData mapData = GenerateMapDataForDiamond();
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMapForDiamond(mapData.heightMap, diamondData.colourDivider * 5));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMeshForDiamond(mapData.heightMap, terrainDiamondData.meshHeightMultiplier, editorPreviewLOD));
        }
    }
    public void DrawMapInEditorForWorley()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainWorleyData.minHeight, terrainWorleyData.maxHeight / worleyData.colourDivider);
        MapData mapData = GenerateMapDataForWolrey();
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMapForWorley(mapData.heightMap, worleyData.colourDivider * 5000));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            //GenerateForDiamond a név de ugyanaz
            display.DrawMesh(MeshGenerator.GenerateTerrainMeshForDiamond(mapData.heightMap, terrainWorleyData.meshHeightMultiplier, editorPreviewLOD));
        }
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
        MapData mapData = GenerateMapDataForPerlin(centre); // csak perlinre van
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
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
        MeshData meshData = MeshGenerator.GenerateTerrainMeshForPerlin(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod); // // csak perlinre van
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
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
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseData.perlinseed, noiseData.noiseScale,
        noiseData.octaves, noiseData.presistance, noiseData.lacunarity, centre + noiseData.offset, normalizeMode);

        if (terrainData.useFalloff)
        {

            if (fallOffMap == null)
            {
                fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
            }

            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    if (terrainData.useFalloff)
                    {
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallOffMap[x, y]);
                    }
                }
            }
        }
        return new MapData(noiseMap);
    }

    MapData GenerateMapDataForRidgedPerlin()
    {
        float[,] ridgednoiseMap = RidgedNoise.GenerateRidgedNoiseMap(mapChunkSize, mapChunkSize, ridgedPerlinData.perlinseed, ridgedPerlinData.noiseScale,
        ridgedPerlinData.octaves, ridgedPerlinData.presistance, ridgedPerlinData.lacunarity, ridgedPerlinData.offset, ridgedPerlinData.inverton);

        if (terrainRidgedPerlin.useFalloff)
        {

            if (fallOffMap == null)
            {
                fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
            }

            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    if (terrainRidgedPerlin.useFalloff)
                    {
                        ridgednoiseMap[x, y] = Mathf.Clamp01(ridgednoiseMap[x, y] - fallOffMap[x, y]);
                    }
                }
            }
        }
        return new MapData(ridgednoiseMap);
    }

    MapData GenerateMapDataForDiamond()
    {
        float[,] diamondsquareMap = DiamondSquareAlgorithm.GenerateDiamondSquareMap(mapChunkSize + 15, mapChunkSize + 15, diamondData.roughness, diamondData.diamondseed);

        if (terrainDiamondData.useFalloff)
        {

            if (fallOffMap == null)
            {
                fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
            }

            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    if (terrainDiamondData.useFalloff)
                    {
                        //diamondsquareMap[x,y] = Mathf.Clamp01(diamondsquareMap[x,y] - fallOffMap[x,y]);
                    }
                }
            }
        }
        return new MapData(diamondsquareMap);
    }

    MapData GenerateMapDataForWolrey()
    {
        float[,] worleyMap = WorleyNoise.GenerateWorleyMap(mapChunkSize, mapChunkSize, worleyData.points, worleyData.distanceBetweenPoints);

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
