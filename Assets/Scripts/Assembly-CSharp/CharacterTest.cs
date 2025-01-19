using UnityEngine;

[ExecuteInEditMode]
public class CharacterTest : MonoBehaviour
{
	private void Start()
	{
		if (Application.isPlaying)
		{
			DataBundleRuntime.Initialize();
		}
	}

	private void Update()
	{
	}
}
