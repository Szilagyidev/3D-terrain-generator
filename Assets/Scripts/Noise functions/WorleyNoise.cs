using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

public static class WorleyNoise
{
    public static float[,] GenerateWorleyMap(int mapWidth, int mapHeight, WorleyData worleyData)
    {
        float[,] worleyMap = new float[mapWidth, mapHeight];

        Vector2[] allpoints = new Vector2[worleyData.points];
        System.Random rand = new System.Random(worleyData.seed);
        for (int i = 0; i < worleyData.points; i++)
        {
            allpoints[i] = new Vector2(rand.Next(1, mapWidth) + worleyData.offset.x, rand.Next(1, mapHeight) + worleyData.offset.y); //can be 0 too, can apply algortihm not just random number for more complexity.
        }

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float[] sortedDistances = new float[worleyData.points];

                for (int i = 0; i < worleyData.points; i++)
                {
                    Vector2 pixel = new Vector2(x, y);
                    float d = Vector2.Distance(pixel, allpoints[i]);
                    sortedDistances[i] = d;
                }
                Array.Sort(sortedDistances);
                float noise = sortedDistances[worleyData.distanceBetweenPoints];
                worleyMap[x, y] = noise;
            }
        }
        return worleyMap;
    }
}
