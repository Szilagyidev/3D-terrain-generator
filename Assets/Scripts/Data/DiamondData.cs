using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class DiamondData : UpdatebleData
{
    public float roughness;
    public int diamondseed;
    [Range(0, 2)]
    public float colourDivider;
}