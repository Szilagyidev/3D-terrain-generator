using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RidgedNoise
{
    public enum NormalizeMode { Local, Global };
    public static float[,] GenerateRidgedNoiseMap(int mapWidth, int mapHeight, RidgedPerlinData ridgedPerlinData, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(ridgedPerlinData.perlinseed);
        Vector2[] octaveOffsets = new Vector2[ridgedPerlinData.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < ridgedPerlinData.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + ridgedPerlinData.offset.x;
            float offsetY = prng.Next(-100000, 100000) - ridgedPerlinData.offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= ridgedPerlinData.presistance;
        }

        if (ridgedPerlinData.noiseScale <= 0)
        {
           ridgedPerlinData.noiseScale = 0.0001f;
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

                for (int i = 0; i < ridgedPerlinData.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / ridgedPerlinData.noiseScale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / ridgedPerlinData.noiseScale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= ridgedPerlinData.presistance;
                    frequency *= ridgedPerlinData.lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = Mathf.Abs(noiseHeight) * -ridgedPerlinData.inverton;
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