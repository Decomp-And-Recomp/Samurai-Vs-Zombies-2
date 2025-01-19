using UnityEngine;

public interface IInputContainer
{
	void FilterInput(InputCrawl crawl, GameObject objectToFilter, out InputRouter.InputResponse response);
}
