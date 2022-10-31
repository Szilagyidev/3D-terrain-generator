using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainDiamondData : UpdatebleData
{
  public float meshHeightMultiplier;
  public int uniformscale = 128;
  public AnimationCurve meshHeightCurve;

  public float minHeight{
        get{
            return uniformscale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
        }
    }

  public float maxHeight{
        get{
            return uniformscale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
        }
    }
}
