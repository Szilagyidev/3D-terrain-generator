using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainRidgedPerlinData : UpdatebleData
{
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public bool useFalloff;
}
