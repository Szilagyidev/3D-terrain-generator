using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode{NoiseMap, ColourMap, Mesh};
    public enum TerrainGenerationType{PerlinNoise, DiamondSquareAlgorithm, WorleyNoise};
    public TerrainGenerationType terrainGenerationType;
    public DrawMode drawMode;

    //perlin noise attributes;
    const int mapChunkSize = 128; // diamondra csak 128, 256 jó
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

    //Diamond square attributes.
    public float roughness;

    //Wolrey attributes;
    public int points;
    public float colourDivider;
    [Range(0,6)]
    public int distanceBetweenPoints;

    public TerrainType[] regions;

    public void GenerateMap(){
        if(terrainGenerationType == TerrainGenerationType.PerlinNoise){

            float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize,mapChunkSize,perlinseed,noiseScale,octaves,presistance,lacunarity,offset);

            Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
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
                }
            }

        else if(terrainGenerationType == TerrainGenerationType.DiamondSquareAlgorithm){

            float[,] diamondsquareMap = DiamondSquareAlgorithm.GenerateDiamondSquareMap(mapChunkSize, mapChunkSize, roughness, 1);

            Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    float currentHeight = diamondsquareMap[x,y];

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
                display.DrawTexture(TextureGenerator.TextureFromHeightMapForDiamond(diamondsquareMap));
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
                    float currentHeight = worleyMap[x,y];

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
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForDiamond(worleyMap, meshHeightMultiplier, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
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
    }
}

[System.Serializable]
public struct TerrainType{
    public string name;
    public float heigth;
    public Color colour;
}
