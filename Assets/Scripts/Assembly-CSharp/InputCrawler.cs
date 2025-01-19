using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Input/Input Crawler")]
public class InputCrawler : MonoBehaviour
{
	public enum InputEventImmediacy
	{
		Immediate = 0,
		Delay_Next_Update = 1
	}

	public InputRouter[] routers;

	public InputExcluder inputExcluder = new InputExcluder();

	public InputFocus inputFocus = new InputFocus();

	public InputActive inputActive = new InputActive();

	public InputTrace inputTrace;

	public List<InputEvent> unhandledEvents = new List<InputEvent>();

	public List<InputEvent> eventsToSend = new List<InputEvent>();

	public InputEventImmediacy eventImmediacy;

	public void Add(InputEvent newEvent)
	{
		eventsToSend.Add(newEvent);
		if (eventImmediacy == InputEventImmediacy.Immediate)
		{
			Send();
		}
	}

	public void SendImmediate(InputEvent eventToSend, GameObject target)
	{
		List<InputTrace.HitInfo> list = new List<InputTrace.HitInfo>();
		InputTrace.HitInfo hitInfo = new InputTrace.HitInfo();
		hitInfo.target = target;
		list.Add(hitInfo);
		InputCrawl inputCrawl = new InputCrawl(eventToSend, list);
		inputCrawl.containerCannotStopInput = true;
		Send(inputCrawl);
	}

	private bool Send(InputCrawl crawl)
	{
		if (routers == null || routers.Length == 0)
		{
			return false;
		}
		if (crawl.hits.Count == 0)
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		do
		{
			InputRouter inputRouter = routers[num];
			InputRouter.InputResponse response;
			inputRouter.ParseInputTrace(crawl, out response);
			num2 = num + 1;
			if (num2 >= routers.Length)
			{
				num2 = 0;
			}
			switch (response)
			{
			case InputRouter.InputResponse.Handled:
			{
				InputTrace.HitInfo currentHit = crawl.CurrentHit;
				crawl.handledBy.Add(currentHit.target);
				return true;
			}
			case InputRouter.InputResponse.Passthrough:
				num3 = num;
				break;
			default:
				if (num2 == num3)
				{
					crawl.Pop();
				}
				break;
			}
			num = num2;
		}
		while (crawl.hits.Count > 0);
		return false;
	}

	public void SendAnyQueuedSubCrawls(InputCrawl crawl)
	{
		while (crawl.queuedSubCrawls.Count > 0)
		{
			InputCrawl inputCrawl = crawl.queuedSubCrawls[0];
			crawl.queuedSubCrawls.RemoveAt(0);
			Send(inputCrawl);
			crawl.handledBy.AddRange(inputCrawl.handledBy);
			crawl.queuedSubCrawls.AddRange(inputCrawl.queuedSubCrawls);
		}
	}

	public void Send()
	{
		foreach (InputEvent item in eventsToSend)
		{
			if (SendCallbackOnNewEvent(item))
			{
				continue;
			}
			List<InputTrace.HitInfo> hits;
			if (inputFocus.HasFocusedObject)
			{
				hits = new List<InputTrace.HitInfo>();
				hits.Add(inputFocus.CreateFocusedHit());
			}
			else
			{
				hits = inputTrace.Trace(item.Position);
				inputExcluder.FilterInput(ref hits);
			}
			InputCrawl inputCrawl = new InputCrawl(item, hits);
			inputActive.Update_Pre_Crawl(this, inputCrawl);
			if (inputFocus.HasFocusedObject)
			{
				inputCrawl.containerCannotStopInput = true;
			}
			bool flag = Send(inputCrawl);
			SendAnyQueuedSubCrawls(inputCrawl);
			inputActive.Update_After_Crawl(this, inputCrawl);
			if (!flag)
			{
				if (eventImmediacy == InputEventImmediacy.Immediate)
				{
					SingletonMonoBehaviour<InputManager>.Instance.OnInputEventUnhandled(item);
				}
				else
				{
					unhandledEvents.Add(item);
				}
			}
		}
		eventsToSend.Clear();
	}

	public bool SendCallbackOnNewEvent(InputEvent thisEvent)
	{
		List<InputRouter.InputResponse> list = SingletonMonoBehaviour<InputManager>.Instance.OnNewInputEvent(thisEvent);
		if (list != null && (list.Contains(InputRouter.InputResponse.Blocked) || list.Contains(InputRouter.InputResponse.Handled)))
		{
			return true;
		}
		return false;
	}

	public void SendUnhandledEvents()
	{
		if (inputExcluder.IsEmpty)
		{
			foreach (InputEvent unhandledEvent in unhandledEvents)
			{
				SingletonMonoBehaviour<InputManager>.Instance.OnInputEventUnhandled(unhandledEvent);
			}
		}
		unhandledEvents.Clear();
	}
}
