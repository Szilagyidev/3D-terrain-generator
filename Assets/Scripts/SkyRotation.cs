using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyRotation : MonoBehaviour
{
    [SerializeField] private Material skybox;
    private float elapsedTime = 0f;
    private float timeScale = 0.5f;
    private static readonly int Rotation = Shader.PropertyToID("_Rotation");

    void Update()
    {
        elapsedTime += Time.deltaTime;
        skybox.SetFloat(Rotation, elapsedTime * timeScale);
    }
}
