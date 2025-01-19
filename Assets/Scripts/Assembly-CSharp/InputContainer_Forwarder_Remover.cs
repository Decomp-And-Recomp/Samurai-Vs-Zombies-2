using UnityEngine;

public class InputContainer_Forwarder_Remover : MonoBehaviour
{
	public InputContainer_Forwarder forwarder;

	public void OnDestroy()
	{
		Object.Destroy(forwarder);
	}
}
