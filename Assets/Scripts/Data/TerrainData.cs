using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdatebleData
{

    public float uniformscale = 2.5f;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public bool useFalloff;

    public float minHeight
    {
        get
        {
            return uniformscale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return uniformscale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
        }
    }

}
