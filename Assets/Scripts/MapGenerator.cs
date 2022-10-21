using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode{NoiseMap, ColourMap, Mesh, FalloffMap};
    public enum TerrainGenerationType{PerlinNoise, RidgedPerlinNoise, DiamondSquareAlgorithm, WorleyNoise};
    public TerrainGenerationType terrainGenerationType;
    public DrawMode drawMode;

    //perlin noise attributes;
    const int mapChunkSize = 128; // diamondra csak 128, 256 jó 2^n miatt
    [Range(0,6)]
    public int levelOfDetail;
    public float noiseScale;
    public int octaves;
    [Range(0,1)]
    public float presistance;
    public float lacunarity;
    public int perlinseed;
    public Vector2 offset;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public bool autoUpdate;
    public bool useFalloff;

    //Ridged perlin noise attributes;
    public float inverton;

    //Diamond square attributes.
    public float roughness;

    //Wolrey attributes;
    public int points;
    public float colourDivider;
    [Range(0,6)]
    public int distanceBetweenPoints;

    public TerrainType[] regions;
    public float[,] fallOffMap;

    void Awake(){
        fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    public void GenerateMap(){
        if(terrainGenerationType == TerrainGenerationType.PerlinNoise){

            float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize,mapChunkSize,perlinseed,noiseScale,octaves,presistance,lacunarity,offset);

            Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    if(useFalloff){
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
        
            MapDisplay display = FindObjectOfType<MapDisplay>();
            if(drawMode == DrawMode.NoiseMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
            }else if(drawMode == DrawMode.ColourMap){
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
            }else if(drawMode == DrawMode.Mesh){
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForPerlin(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
            }else if(drawMode == DrawMode.FalloffMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
            }
            }

        else if(terrainGenerationType == TerrainGenerationType.DiamondSquareAlgorithm){

            float[,] diamondsquareMap = DiamondSquareAlgorithm.GenerateDiamondSquareMap(mapChunkSize, mapChunkSize, roughness, 1);

            Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    float currentHeight = diamondsquareMap[x,y] / colourDivider;

                    for (int i = 0; i < regions.Length; i++)
                    {
                        if(currentHeight <= regions[i].heigth){
                            colourMap[y * mapChunkSize + x] = regions[i].colour;
                            break;
                        }
                    }
                }
            }

            MapDisplay display = FindObjectOfType<MapDisplay>();
            if(drawMode == DrawMode.NoiseMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMapForDiamond(diamondsquareMap, colourDivider));
            } else if(drawMode == DrawMode.ColourMap){
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
            }else if(drawMode == DrawMode.Mesh){
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForDiamond(diamondsquareMap, meshHeightMultiplier, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
            }
        }

        else if(terrainGenerationType == TerrainGenerationType.WorleyNoise){

            float[,] worleyMap = WorleyNoise.GenerateWorleyMap(mapChunkSize, mapChunkSize, points, distanceBetweenPoints);

            Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    float currentHeight = worleyMap[x,y] / colourDivider;

                    for (int i = 0; i < regions.Length; i++)
                    {
                        if(currentHeight <= regions[i].heigth){
                            colourMap[y * mapChunkSize + x] = regions[i].colour;
                            break;
                        }
                    }
                }
            }

            MapDisplay display = FindObjectOfType<MapDisplay>();
            if(drawMode == DrawMode.NoiseMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMapForWorley(worleyMap, colourDivider));
            }else if(drawMode == DrawMode.ColourMap){
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
            }else if(drawMode == DrawMode.Mesh){
                //GenerateForDiamond van meghivva pedig ez Worley
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForDiamond(worleyMap, meshHeightMultiplier, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
            }
        }
        else if(terrainGenerationType == TerrainGenerationType.RidgedPerlinNoise){

            float[,] ridgednoiseMap = RidgedNoise.GenerateRidgedNoiseMap(mapChunkSize,mapChunkSize,perlinseed,noiseScale,octaves,presistance,lacunarity,offset, inverton);

            Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    if(useFalloff){
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
        
            MapDisplay display = FindObjectOfType<MapDisplay>();
            if(drawMode == DrawMode.NoiseMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(ridgednoiseMap));
            }else if(drawMode == DrawMode.ColourMap){
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
            }else if(drawMode == DrawMode.Mesh){
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForPerlin(ridgednoiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
            }else if(drawMode == DrawMode.FalloffMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
            }
            }
        
    }

    void OnValidate(){
        if(lacunarity < 1){
            lacunarity = 1;
        }
        if(octaves < 0){
            octaves = 0;
        }
        if(points < 1){
            points = 1;
        }
        if(meshHeightMultiplier <= 0){
            meshHeightMultiplier = 0;
        }
        if(roughness <= 0){
            roughness = 0;
        }
        if(distanceBetweenPoints < 0){
            distanceBetweenPoints = 0;
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
