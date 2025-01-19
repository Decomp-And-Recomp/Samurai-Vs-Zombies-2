using UnityEngine;

public class MysteryBoxFXTextureSwap : MonoBehaviour
{
	public GameObject present;

	private void Start()
	{
		if (!(MysteryBoxImpl.GetOverrideTexture() != null))
		{
			return;
		}
		ParticleSystem component = present.GetComponent<ParticleSystem>();
		if (component != null)
		{
			Renderer renderer = component.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.material.mainTexture = MysteryBoxImpl.GetOverrideTexture();
			}
		}
	}
}
