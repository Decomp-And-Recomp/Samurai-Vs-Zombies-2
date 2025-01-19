using UnityEngine;

public class EffectUseSpawnersTexture : MonoBehaviour
{
	private void SpawnedFrom(GameObject source)
	{
		Renderer componentInChildren = source.GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren == null)
		{
			componentInChildren = source.GetComponentInChildren<MeshRenderer>();
		}
		Renderer componentInChildren2 = GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren2 == null)
		{
			componentInChildren2 = GetComponentInChildren<MeshRenderer>();
		}
		if (componentInChildren != null && componentInChildren2 != null)
		{
			componentInChildren2.material.mainTexture = componentInChildren.sharedMaterial.mainTexture;
		}
	}
}
