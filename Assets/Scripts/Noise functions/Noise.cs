using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    public enum NormalizeMode { Local, Global };
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseData noiseData, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(noiseData.perlinseed);
        Vector2[] octaveOffsets = new Vector2[noiseData.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < noiseData.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + noiseData.offset.x;
            float offsetY = prng.Next(-100000, 100000) - noiseData.offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= noiseData.presistance;
        }

        if (noiseData.noiseScale <= 0)
        {
            noiseData.noiseScale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < noiseData.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / noiseData.noiseScale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / noiseData.noiseScale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= noiseData.presistance;
                    frequency *= noiseData.lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }
}