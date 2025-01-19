using UnityEngine;

public class InputContainer_Forwarder : MonoBehaviour, IInputContainer
{
	public GameObject objectToForwardTo;

	public void Awake()
	{
		if (!(objectToForwardTo != null))
		{
		}
	}

	public void SetObjectToForwardTo(GameObject objectToForwardTo)
	{
		this.objectToForwardTo = objectToForwardTo;
		SetupForwardingTargetRemover();
	}

	private void SetupForwardingTargetRemover()
	{
		InputContainer_Forwarder_Remover inputContainer_Forwarder_Remover = objectToForwardTo.AddComponent(typeof(InputContainer_Forwarder_Remover)) as InputContainer_Forwarder_Remover;
		inputContainer_Forwarder_Remover.forwarder = this;
	}

	public void FilterInput(InputCrawl crawl, GameObject objectToFilter, out InputRouter.InputResponse response)
	{
		if (objectToForwardTo == null)
		{
			Object.Destroy(this);
		}
		IInputContainer inputContainer = ObjectUtils.FindComponent<IInputContainer>(objectToForwardTo) as IInputContainer;
		if (inputContainer == null)
		{
			response = InputRouter.InputResponse.Passthrough;
		}
		else
		{
			inputContainer.FilterInput(crawl, objectToFilter, out response);
		}
	}
}
