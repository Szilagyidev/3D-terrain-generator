using System.Collections;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
   public Renderer textureRenderer;
   public MeshFilter meshFilter;
   public MeshRenderer meshRenderer;

   public void DrawTexture(Texture2D texture){
      textureRenderer.sharedMaterial.mainTexture = texture;
      textureRenderer.transform.localScale = Vector3.one * FindObjectOfType<MapGenerator>().terrainData.uniformscale * 14;

      textureRenderer.gameObject.SetActive (true);
	   meshFilter.gameObject.SetActive (false);
   }

   public void DrawMesh(MeshData meshData){
      meshFilter.sharedMesh = meshData.CreateMesh();
      meshFilter.transform.localScale = Vector3.one * FindObjectOfType<MapGenerator>().terrainData.uniformscale; // mapchunksize
      
		textureRenderer.gameObject.SetActive(false);
		meshFilter.gameObject.SetActive (true);
   }
}
