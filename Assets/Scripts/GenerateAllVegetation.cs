using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateAllVegetation : MonoBehaviour
{
    public VegetationGenerator tree;
    public VegetationGenerator grass;
    public VegetationGenerator grass2;
    public VegetationGenerator grass3;
    public VegetationGenerator flower;
    public VegetationGenerator flower2;
    public VegetationGenerator rock;

    public MapGenerator mapGenerator;

    public void GenerateAll()
    {
        if (mapGenerator.GenerateVegetation == true)
        {
            tree.Generate();
            grass.Generate();
            grass2.Generate();
            grass3.Generate();
            flower.Generate();
            flower2.Generate();
            rock.Generate();
        }
        else
        {
            tree.Clear();
            grass.Clear();
            grass2.Clear();
            grass3.Clear();
            flower.Clear();
            flower2.Clear();
            rock.Clear();
        }
    }
}
