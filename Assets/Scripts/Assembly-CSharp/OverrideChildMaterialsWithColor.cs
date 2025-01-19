using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OverrideChildMaterialsWithColor : MonoBehaviour
{
	public Material material;

	public Color color;

	public string filter;

	private List<Material> materials;

	private void Start()
	{
		materials = new List<Material>();
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer renderer in componentsInChildren)
		{
			materials.Add(renderer.sharedMaterial);
		}
	}

	private void Update()
	{
		foreach (Material material in materials)
		{
			material.SetColor("_Color", color);
			material.SetColor("_MainColor", color);
			material.SetColor("_RimColor", color);
		}
	}
}
