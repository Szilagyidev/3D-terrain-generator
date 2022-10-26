using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class WorleyData : UpdatebleData
{
    public int points;
    public float colourDivider;
    [Range(0,6)]
    public int distanceBetweenPoints;

        protected override void OnValidate(){
        if(points < 1){
            points = 1;
        }
        if(distanceBetweenPoints < 0){
            distanceBetweenPoints = 0;
        }
        base.OnValidate();
    }
}
