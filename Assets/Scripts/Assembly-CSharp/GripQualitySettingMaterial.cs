using GripTech;
using UnityEngine;

public class GripQualitySettingMaterial : MonoBehaviour
{
	public string pathToTexture = string.Empty;

	private Material qualityMaterial;

	private Texture2D qualityTexture;

	private void Start()
	{
		if (!(pathToTexture == string.Empty))
		{
			qualityTexture = ResourceLoader.Load(pathToTexture) as Texture2D;
			qualityMaterial = new Material(Shader.Find("Diffuse"));
			qualityMaterial.mainTexture = qualityTexture;
			base.GetComponent<Renderer>().material = qualityMaterial;
		}
	}
}
