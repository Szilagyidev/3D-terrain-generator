using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class RidgedPerlinData : UpdatebleData
{
    public float noiseScale;
    public int octaves;
    [Range(0,1)]
    public float presistance;
    public float lacunarity;
    public int perlinseed;
    public Vector2 offset;
    public float inverton;

    protected override void OnValidate(){
        if(lacunarity < 1){
            lacunarity = 1;
        }
        if(octaves < 0){
            octaves = 0;
        }

        base.OnValidate();
    }
}