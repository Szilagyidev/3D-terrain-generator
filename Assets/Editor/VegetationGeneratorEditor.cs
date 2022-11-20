using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(VegetationGenerator))]
public class VegetationGeneratorEditor : Editor
{
       public override void OnInspectorGUI(){
        base.OnInspectorGUI();

        VegetationGenerator vegetationGen = (VegetationGenerator)target;

        if (GUILayout.Button("Generate")) { vegetationGen.Generate();}
        if (GUILayout.Button("Clear")) { vegetationGen.Clear();}
       }
}
