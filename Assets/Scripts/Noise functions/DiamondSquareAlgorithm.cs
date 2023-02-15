using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public static class DiamondSquareAlgorithm
{
    public static float[,] GenerateDiamondSquareMap(int terrainPoints, DiamondData diamondData)
    {
        int DATA_SIZE = terrainPoints + 1;
        float[,] data = new float[DATA_SIZE, DATA_SIZE];

        System.Random rand = new System.Random(diamondData.diamondseed);

        data[0, 0] = data[0, DATA_SIZE - 1] = data[DATA_SIZE - 1, 0] =
        data[DATA_SIZE - 1, DATA_SIZE - 1] = (float)rand.NextDouble();

        float h = diamondData.roughness;

        for (int sideLength = DATA_SIZE - 1; sideLength >= 2; sideLength /= 2, h /= 2.0f)
        {
            int halfSide = sideLength / 2;

            for (int x = 0; x < DATA_SIZE - 1; x += sideLength)
            {
                for (int y = 0; y < DATA_SIZE - 1; y += sideLength)
                {
                    float avg = data[x, y] + //top left
                    data[x + sideLength, y] +//top right
                    data[x, y + sideLength] + //lower left
                    data[x + sideLength, y + sideLength];//lower right
                    avg /= 4.0f;

                    data[x + halfSide, y + halfSide] = avg + ((float)rand.NextDouble() * 2 * h) - h;
                }
            }

            for (int x = 0; x < DATA_SIZE - 1; x += halfSide)
            {
                for (int y = (x + halfSide) % sideLength; y < DATA_SIZE - 1; y += sideLength)
                {
                    float avg =
                      data[(x - halfSide + (DATA_SIZE - 1)) % (DATA_SIZE - 1), y] + //left of center
                      data[(x + halfSide) % (DATA_SIZE - 1), y] + //right of center
                      data[x, (y + halfSide) % (DATA_SIZE - 1)] + //below center
                      data[x, (y - halfSide + (DATA_SIZE - 1)) % (DATA_SIZE - 1)]; //above center
                    avg /= 4.0f;

                    avg = avg + ((float)rand.NextDouble() * 2 * h) - h;
                    data[x, y] = avg;

                    if (x == 0) data[DATA_SIZE - 1, y] = avg;
                    if (y == 0) data[x, DATA_SIZE - 1] = avg;
                }
            }
        }
        return data;
    }

}