using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                if(mapGen.currentNoise=="Perlin"){mapGen.DrawMapInEditorForPerlin();}
                if(mapGen.currentNoise=="Diamond"){mapGen.DrawMapInEditorForDiamond();}
                if(mapGen.currentNoise=="RidgedPerlin"){mapGen.DrawMapInEditorForRidgedPerlin();}
                if(mapGen.currentNoise=="Worley"){mapGen.DrawMapInEditorForWorley();}
            }
        }

        if (GUILayout.Button("Generate Perlin")) { mapGen.DrawMapInEditorForPerlin(); mapGen.currentNoise = "Perlin";}
        if (GUILayout.Button("Generate RidgedPerlin")) { mapGen.DrawMapInEditorForRidgedPerlin(); mapGen.currentNoise = "RidgedPerlin"; }
        if (GUILayout.Button("Generate Diamond")) { mapGen.DrawMapInEditorForDiamond(); mapGen.currentNoise = "Diamond";}
        if (GUILayout.Button("Generate Worley")) { mapGen.DrawMapInEditorForWorley(); mapGen.currentNoise = "Worley";}

    }
}
