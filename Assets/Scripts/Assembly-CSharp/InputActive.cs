using System.Collections.Generic;
using UnityEngine;

public class InputActive
{
	private List<GameObject> activeObjects = new List<GameObject>();

	public bool HasActiveObject
	{
		get
		{
			return activeObjects.Count > 0;
		}
	}

	private void Add(GameObject activeObject)
	{
		if (!activeObjects.Contains(activeObject))
		{
			activeObjects.Add(activeObject);
		}
	}

	private void Remove(InputCrawler crawler, InputCrawl crawl, bool sendExitEvent, List<GameObject> objectsToRemove)
	{
		foreach (GameObject item in objectsToRemove)
		{
			activeObjects.Remove(item);
			if (sendExitEvent)
			{
				SendExitEvent(crawler, crawl, item);
			}
		}
	}

	private void Change(InputCrawler crawler, InputCrawl crawl, List<GameObject> newObjects)
	{
		List<GameObject> objectsToRemove = activeObjects.FindAll((GameObject thisObject) => !newObjects.Contains(thisObject));
		Remove(crawler, crawl, true, objectsToRemove);
		activeObjects = newObjects;
	}

	private void SendExitEvent(InputCrawler crawler, InputCrawl crawl, GameObject thisObject)
	{
		InputEvent inputEvent = new InputEvent(InputEvent.EEventType.OnCursorExit, crawl.inputEvent.Position, crawl.inputEvent.CursorIndex);
		inputEvent.Target = thisObject;
		crawler.SendImmediate(inputEvent, thisObject);
	}

	public void Update_Pre_Crawl(InputCrawler crawler, InputCrawl crawl)
	{
		switch (crawl.inputEvent.EventType)
		{
		case InputEvent.EEventType.OnCursorMove:
		{
			List<GameObject> objectsToRemove = activeObjects.FindAll((GameObject thisObject) => crawl.Find_OriginalHits(thisObject) == null);
			Remove(crawler, crawl, true, objectsToRemove);
			break;
		}
		case InputEvent.EEventType.OnCursorDown:
		case InputEvent.EEventType.OnCursorUp:
			activeObjects.Clear();
			break;
		}
	}

	public void Update_After_Crawl(InputCrawler crawler, InputCrawl crawl)
	{
		InputEvent.EEventType eventType = crawl.inputEvent.EventType;
		if (eventType == InputEvent.EEventType.OnCursorDown || eventType == InputEvent.EEventType.OnCursorMove)
		{
			Change(crawler, crawl, crawl.handledBy);
		}
	}
}
