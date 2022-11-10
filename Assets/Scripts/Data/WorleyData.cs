using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class WorleyData : UpdatebleData
{
    public int points;
    [Range(0.0f, 0.1f)]
    public float colourDivider;
    [Range(0, 6)]
    public int distanceBetweenPoints;

    #if UNITY_EDITOR

    protected override void OnValidate()
    {
        if (points < 1)
        {
            points = 1;
        }
        if (distanceBetweenPoints < 0)
        {
            distanceBetweenPoints = 0;
        }
        base.OnValidate();
    }

    #endif
}
