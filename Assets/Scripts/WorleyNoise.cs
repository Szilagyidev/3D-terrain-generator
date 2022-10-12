using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;
using System;

public static class WorleyNoise
{
 public static float[,] GenerateWorleyMap(int mapWidth, int mapHeight, int points, int distanceBetweenPoints){

    float[,] worleyMap = new float[mapWidth, mapHeight];

    Vector2[] allpoints = new Vector2[points];

    for (int i = 0; i < points; i++)
    {
       allpoints[i] = new Vector2(Random.Range(1, mapWidth), Random.Range(1, mapHeight)); //lehet 0 is, lehet más alapján is nem csak random
    }
    

    for (int x = 0; x < mapWidth; x++)
    {
        for (int y = 0; y < mapHeight; y++)
        {

            float[] distances = new float[points];
            float[] sortedDistances = new float[points];

            for (int i = 0; i < points; i++)
            {
                Vector2 pixel = new Vector2(x,y);
                float d = Vector2.Distance(pixel, allpoints[i]);
                distances[i] = d;
                sortedDistances[i] = d;
            }
            Array.Sort(sortedDistances);
            float noise = sortedDistances[distanceBetweenPoints];
            worleyMap[x, y] = noise;
        }
    }
    return worleyMap;
 }

}
