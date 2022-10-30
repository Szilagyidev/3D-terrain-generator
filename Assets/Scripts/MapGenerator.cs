using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode{NoiseMap, Mesh, FalloffMap};
    public DrawMode drawMode;

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

    public const int mapChunkSize = 241;  // a level of detailre jó. a diamond square az 256
    [Range(0,6)]
    public int levelOfDetail;
    public bool autoUpdate;
    public float[,] fallOffMap;

    [System.NonSerialized]
    public string currentNoise;

    void OnValuesUpdated(){
        if(!Application.isPlaying){
           if(currentNoise == "Perlin"){ DrawMapInEditorForPerlin();}
           if(currentNoise == "Diamond") {DrawMapInEditorForDiamond();}
           if(currentNoise == "RidgedPerlin") {DrawMapInEditorForRidgedPerlin();}
           if(currentNoise == "Worley") {DrawMapInEditorForWorley();}
        }
    }

    void OnTextureValuesUpdated(){
     textureData.ApplyToMaterial(terrainMaterial);
    }

    public void DrawMapInEditorForPerlin(){
            MapData mapData = GenerateMapDataForPerlin();
            MapDisplay display = FindObjectOfType<MapDisplay>();
            if(drawMode == DrawMode.NoiseMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            }else if(drawMode == DrawMode.Mesh){
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForPerlin(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, levelOfDetail));
            }else if(drawMode == DrawMode.FalloffMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
            }
    }

    public void DrawMapInEditorForDiamond(){
            MapData mapData = GenerateMapDataForDiamond();
            MapDisplay display = FindObjectOfType<MapDisplay>();
            if(drawMode == DrawMode.NoiseMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMapForDiamond(mapData.heightMap, diamondData.colourDivider));
            }else if(drawMode == DrawMode.Mesh){
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForDiamond(mapData.heightMap, terrainDiamondData.meshHeightMultiplier, levelOfDetail));
            }
    }
    public void DrawMapInEditorForWorley(){
            MapData mapData = GenerateMapDataForWolrey();
            MapDisplay display = FindObjectOfType<MapDisplay>();
            if(drawMode == DrawMode.NoiseMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMapForWorley(mapData.heightMap, worleyData.colourDivider));
            }else if(drawMode == DrawMode.Mesh){
                //GenerateForDiamond a név de ugyanaz
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForDiamond(mapData.heightMap, terrainWorleyData.meshHeightMultiplier, levelOfDetail));
            }
    }

    public void DrawMapInEditorForRidgedPerlin(){
            MapData mapData = GenerateMapDataForRidgedPerlin();
            MapDisplay display = FindObjectOfType<MapDisplay>();
            if(drawMode == DrawMode.NoiseMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            }else if(drawMode == DrawMode.Mesh){
                display.DrawMesh(MeshGenerator.GenerateTerrainMeshForPerlin(mapData.heightMap, terrainRidgedPerlin.meshHeightMultiplier, terrainRidgedPerlin.meshHeightCurve, levelOfDetail));
            }else if(drawMode == DrawMode.FalloffMap){
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
            }
    }    

   MapData GenerateMapDataForPerlin(){
            float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize,mapChunkSize,noiseData.perlinseed,noiseData.noiseScale,noiseData.octaves,noiseData.presistance,noiseData.lacunarity,noiseData.offset);

            if(terrainData.useFalloff){

                if(fallOffMap == null){
                    fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
                }

                for (int y = 0; y < mapChunkSize; y++)
                {
                    for (int x = 0; x < mapChunkSize; x++)
                    {
                        if(terrainData.useFalloff){
                            noiseMap[x,y] = Mathf.Clamp01(noiseMap[x,y] - fallOffMap[x,y]);
                        }
                    }
                }
            }
            textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
            return new MapData(noiseMap);
        }

    MapData GenerateMapDataForDiamond(){
            float[,] diamondsquareMap = DiamondSquareAlgorithm.GenerateDiamondSquareMap(mapChunkSize + 15, mapChunkSize + 15, diamondData.roughness, diamondData.diamondseed); // 241 + 15 = 256

                for (int y = 0; y < mapChunkSize; y++)
                {
                    for (int x = 0; x < mapChunkSize; x++)
                    {
                        //float currentHeight = diamondsquareMap[x,y] / diamondData.colourDivider;
                    }
                }
            return new MapData(diamondsquareMap);
        }

    MapData GenerateMapDataForWolrey(){
            float[,] worleyMap = WorleyNoise.GenerateWorleyMap(mapChunkSize, mapChunkSize, worleyData.points, worleyData.distanceBetweenPoints);

            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    //float currentHeight = worleyMap[x,y] / worleyData.colourDivider;
                }
            }
            return new MapData(worleyMap);
        }
        
    MapData GenerateMapDataForRidgedPerlin(){
            float[,] ridgednoiseMap = RidgedNoise.GenerateRidgedNoiseMap(mapChunkSize,mapChunkSize,ridgedPerlinData.perlinseed,ridgedPerlinData.noiseScale,ridgedPerlinData.octaves,ridgedPerlinData.presistance,ridgedPerlinData.lacunarity,ridgedPerlinData.offset,
            ridgedPerlinData.inverton);

            if(terrainData.useFalloff){

            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    if(terrainRidgedPerlin.useFalloff){
                        ridgednoiseMap[x,y] = Mathf.Clamp01(ridgednoiseMap[x,y] - fallOffMap[x,y]);
                    }
                }
            }
        }
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        return new MapData(ridgednoiseMap);
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
        if(textureData != null){
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }
}

public struct MapData{
    public float[,] heightMap;
    public MapData(float[,] heightMap){
        this.heightMap = heightMap;
    }
}
