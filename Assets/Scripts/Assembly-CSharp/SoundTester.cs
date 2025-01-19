using UnityEngine;

[AddComponentMenu("Audio/Sound Tester")]
public class SoundTester : MonoBehaviour
{
	private void Awake()
	{
		DataBundleRuntime.Initialize();
	}

	private void Start()
	{
		SingletonSpawningMonoBehaviour<DebugMain>.Instance.Allow4TouchActivate = true;
	}
}
