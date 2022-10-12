using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int heigth){ // valszeg ez is kell külön minden algra
        Texture2D texture =new Texture2D(width, heigth);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap){
        int width = heightMap.GetLength(0);
        int heigth = heightMap.GetLength(1);

        Color[] colorMap = new Color[width * heigth];
        for (int y = 0; y < heigth; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x,y]);
            }
        }
        return TextureFromColourMap(colorMap, width, heigth);
    }

    public static Texture2D TextureFromHeightMapForDiamond(float[,] heightMap){
        int width = heightMap.GetLength(0);
        int heigth = heightMap.GetLength(1);

        Color[] colorMap = new Color[width * heigth];
        for (int y = 0; y < heigth; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x,y]); // nem pontosan 0-1 érték között mozog hanem 1.2..stb
            }
        }
        return TextureFromColourMap(colorMap, width, heigth);
    }

    public static Texture2D TextureFromHeightMapForWorley(float[,] heightMap, float colourDivider){
        int width = heightMap.GetLength(0);
        int heigth = heightMap.GetLength(1);

        Color[] colorMap = new Color[width * heigth];
        for (int y = 0; y < heigth; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x,y] / colourDivider);
            }
        }
        return TextureFromColourMap(colorMap, width, heigth);
    }
}
