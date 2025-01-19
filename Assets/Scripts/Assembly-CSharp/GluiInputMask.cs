using System;
using UnityEngine;

[Serializable]
public class GluiInputMask
{
	public enum InputMaskType
	{
		Mask_Collider = 0
	}

	public InputMaskType maskBy;

	public virtual void FilterInput(InputCrawl crawl, GameObject objectToFilter, out InputRouter.InputResponse response, GameObject owner)
	{
		if (maskBy == InputMaskType.Mask_Collider)
		{
			FilterByCollider(crawl, objectToFilter, out response, owner);
		}
		else
		{
			response = InputRouter.InputResponse.Unhandled;
		}
	}

	private void FilterByCollider(InputCrawl crawl, GameObject objectToFilter, out InputRouter.InputResponse response, GameObject owner)
	{
		if (owner == objectToFilter)
		{
			response = InputRouter.InputResponse.Handled;
		}
		else if (crawl.Find_OriginalHits(owner) != null)
		{
			response = InputRouter.InputResponse.Handled;
		}
		else
		{
			response = InputRouter.InputResponse.Blocked;
		}
	}
}
