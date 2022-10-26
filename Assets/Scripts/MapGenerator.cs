using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode{NoiseMap, ColourMap, Mesh, FalloffMap};
    public DrawMode drawMode;
    public TerrainData terrainData;
    public TerrainWorleyData terrainWorleyData;
    public NoiseData noiseData;
    public WorleyData worleyData;
    public DiamondData diamondData;
    public TerrainDiamondData terrainDiamondData;
    public RidgedPerlinData ridgedPerlinData;
    public TerrainRidgedPerlinData terrainRidgedPerlin;


    public const int mapChunkSizeDiamond = 256; // diamondra csak 128, 256 jó 2^n miatt
    public const int mapChunkSize = 241;  // 241pedig a level of detailre jó.
    [Range(0,6)]
    public int levelOfDetail;
    public bool autoUpdate;
    public TerrainType[] regions;
    public float[,] fallOffMap;

    [System.NonSerialized]
    public string currentNoise;

    void Awake(){
        fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    void OnValuesUpdated(){
        if(!Application.isPlaying){
           if(currentNoise == "Perlin"){ DrawMapInEditorForPerlin();}
           if(currentNoise == "Diamond") {DrawMapInEditorForDiamond();}
           if(currentNoise == "RidgedPerlin") {DrawMapInEditorForRidgedPerlin();}
           if(currentNoise == "Worley") {DrawMapInEditorForWorley();}
        }
    }

    public void DrawMapInEditorForPerlin(){
            MapData mapData = GenerateMapDataForPerlin();
            MapDisplay display = FindObjectOfType<MapDisplay>();
            if(drawMode == DrawMode.NoiseMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            }else if(drawMode == DrawMode.ColourMap){
                display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
            }else if(drawMode == DrawMode.Mesh){
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForPerlin(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
            }else if(drawMode == DrawMode.FalloffMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
            }
    }

    public void DrawMapInEditorForDiamond(){
            MapData mapData = GenerateMapDataForDiamond();
            MapDisplay display = FindObjectOfType<MapDisplay>();
            if(drawMode == DrawMode.NoiseMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMapForDiamond(mapData.heightMap, diamondData.colourDivider));
            } else if(drawMode == DrawMode.ColourMap){
                display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSizeDiamond, mapChunkSizeDiamond));
            }else if(drawMode == DrawMode.Mesh){
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForDiamond(mapData.heightMap, terrainDiamondData.meshHeightMultiplier, levelOfDetail), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
            }
    }
    public void DrawMapInEditorForWorley(){
            MapData mapData = GenerateMapDataForWolrey();
            MapDisplay display = FindObjectOfType<MapDisplay>();
            if(drawMode == DrawMode.NoiseMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMapForWorley(mapData.heightMap, worleyData.colourDivider));
            }else if(drawMode == DrawMode.ColourMap){
                display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
            }else if(drawMode == DrawMode.Mesh){
                //GenerateForDiamond a név de ugyanaz
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForDiamond(mapData.heightMap, terrainWorleyData.meshHeightMultiplier, levelOfDetail), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
            }
    }

    public void DrawMapInEditorForRidgedPerlin(){
            MapData mapData = GenerateMapDataForRidgedPerlin();
            MapDisplay display = FindObjectOfType<MapDisplay>();
            if(drawMode == DrawMode.NoiseMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            }else if(drawMode == DrawMode.ColourMap){
                display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
            }else if(drawMode == DrawMode.Mesh){
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForPerlin(mapData.heightMap, terrainRidgedPerlin.meshHeightMultiplier, terrainRidgedPerlin.meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
            }else if(drawMode == DrawMode.FalloffMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
            }
    }    

   MapData GenerateMapDataForPerlin(){
            float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize,mapChunkSize,noiseData.perlinseed,noiseData.noiseScale,noiseData.octaves,noiseData.presistance,noiseData.lacunarity,noiseData.offset);

            Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    if(terrainData.useFalloff){
                        noiseMap[x,y] = Mathf.Clamp01(noiseMap[x,y] - fallOffMap[x,y]);
                    }

                    float currentHeight = noiseMap[x,y];

                    for (int i = 0; i < regions.Length; i++)
                    {
                        if(currentHeight <= regions[i].heigth){
                            colourMap[y * mapChunkSize + x] = regions[i].colour;
                            break;
                        }
                    }
                }
            }
            return new MapData(noiseMap, colourMap);
            }

    MapData GenerateMapDataForDiamond(){
            float[,] diamondsquareMap = DiamondSquareAlgorithm.GenerateDiamondSquareMap(mapChunkSizeDiamond, mapChunkSizeDiamond, diamondData.roughness, diamondData.diamondseed);

            Color[] colourMap = new Color[mapChunkSizeDiamond * mapChunkSizeDiamond];
            for (int y = 0; y < mapChunkSizeDiamond; y++)
            {
                for (int x = 0; x < mapChunkSizeDiamond; x++)
                {
                    float currentHeight = diamondsquareMap[x,y] / diamondData.colourDivider;

                    for (int i = 0; i < regions.Length; i++)
                    {
                        if(currentHeight <= regions[i].heigth){
                            colourMap[y * mapChunkSizeDiamond + x] = regions[i].colour;
                            break;
                        }
                    }
                }
            }

            return new MapData(diamondsquareMap, colourMap);
            }

    MapData GenerateMapDataForWolrey(){
            float[,] worleyMap = WorleyNoise.GenerateWorleyMap(mapChunkSize, mapChunkSize, worleyData.points, worleyData.distanceBetweenPoints);

            Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    float currentHeight = worleyMap[x,y] / worleyData.colourDivider;

                    for (int i = 0; i < regions.Length; i++)
                    {
                        if(currentHeight <= regions[i].heigth){
                            colourMap[y * mapChunkSize + x] = regions[i].colour;
                            break;
                        }
                    }
                }
            }
            return new MapData(worleyMap, colourMap);
            }
        
    MapData GenerateMapDataForRidgedPerlin(){
            float[,] ridgednoiseMap = RidgedNoise.GenerateRidgedNoiseMap(mapChunkSize,mapChunkSize,ridgedPerlinData.perlinseed,ridgedPerlinData.noiseScale,ridgedPerlinData.octaves,ridgedPerlinData.presistance,ridgedPerlinData.lacunarity,ridgedPerlinData.offset,
            ridgedPerlinData.inverton);

            Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    if(terrainRidgedPerlin.useFalloff){
                        ridgednoiseMap[x,y] = Mathf.Clamp01(ridgednoiseMap[x,y] - fallOffMap[x,y]);
                    }

                    float currentHeight = ridgednoiseMap[x,y];

                    for (int i = 0; i < regions.Length; i++)
                    {
                        if(currentHeight <= regions[i].heigth){
                            colourMap[y * mapChunkSize + x] = regions[i].colour;
                            break;
                        }
                    }
                }
            }
        return new MapData(ridgednoiseMap, colourMap);
    }
                 
    void OnValidate(){
        if(terrainData != null){
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if(noiseData != null){
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if(worleyData != null){
            worleyData.OnValuesUpdated -= OnValuesUpdated;
            worleyData.OnValuesUpdated += OnValuesUpdated;
        }
        if(diamondData != null){
            diamondData.OnValuesUpdated -= OnValuesUpdated;
            diamondData.OnValuesUpdated += OnValuesUpdated;
        }
        if(ridgedPerlinData != null){
            ridgedPerlinData.OnValuesUpdated -= OnValuesUpdated;
            ridgedPerlinData.OnValuesUpdated += OnValuesUpdated;
        }

        fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }
}

[System.Serializable]
public struct TerrainType{
    public string name;
    public float heigth;
    public Color colour;
}

public struct MapData{
    public float[,] heightMap;
    public Color[] colourMap;

    public MapData(float[,] heightMap, Color[] colourMap){
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}
