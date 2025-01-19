using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Input/Input Router")]
public class InputRouter : MonoBehaviour
{
	public enum InputResponse
	{
		Unhandled = 0,
		Passthrough = 1,
		Handled_Passive = 2,
		Handled = 3,
		Blocked = 4
	}

	private struct InputContainerFilterNode
	{
		public Component container;
	}

	public LayerMask layersToMatch = -1;

	private Func<GameObject, Component[]> getRouterComponents;

	public InputRouter()
	{
		getRouterComponents = FindAllSearchMethod;
	}

	protected InputRouter(Func<GameObject, Component[]> routerComponentSearch)
	{
		getRouterComponents = routerComponentSearch;
	}

	public virtual void ParseInputTrace(InputCrawl crawl, out InputResponse response)
	{
		response = InputResponse.Unhandled;
		bool flag;
		do
		{
			flag = false;
			InputTrace.HitInfo hitInfo = crawl.hits[0];
			GameObject target = hitInfo.target;
			if (target == null)
			{
				crawl.Pop();
			}
			else
			{
				if ((int)layersToMatch != -1 && ((1 << target.layer) & (int)layersToMatch) == 0)
				{
					continue;
				}
				Component[] array = null;
				if (getRouterComponents != null)
				{
					array = getRouterComponents(target);
				}
				if (array == null || array.Length <= 0)
				{
					continue;
				}
				Component[] array2 = array;
				foreach (Component component in array2)
				{
					InputResponse response2;
					ContainerPrefilter(crawl, target, out response2);
					if (crawl.containerCannotStopInput)
					{
						response2 = InputResponse.Handled;
					}
					switch (response2)
					{
					case InputResponse.Handled_Passive:
						crawl.handledBy.Add(target);
						flag = true;
						crawl.Pop();
						break;
					case InputResponse.Passthrough:
						flag = true;
						crawl.Pop();
						break;
					case InputResponse.Blocked:
						response = InputResponse.Handled;
						return;
					case InputResponse.Unhandled:
					case InputResponse.Handled:
						SendToComponent(component, crawl, out response);
						switch (response)
						{
						case InputResponse.Handled:
							return;
						case InputResponse.Passthrough:
							flag = true;
							crawl.Pop();
							break;
						case InputResponse.Unhandled:
							flag = false;
							break;
						}
						break;
					}
				}
			}
		}
		while (flag && crawl.hits.Count > 0);
	}

	protected void ContainerPrefilter(InputCrawl crawl, GameObject target, out InputResponse response)
	{
		bool flag = false;
		bool flag2 = false;
		List<Component> inputContainersToHandle = GetInputContainersToHandle(target.transform, crawl.stopContainerSearchAtObject);
		foreach (Component item in inputContainersToHandle)
		{
			if (item != null)
			{
				InputResponse response2;
				((IInputContainer)item).FilterInput(crawl, target, out response2);
				switch (response2)
				{
				case InputResponse.Handled_Passive:
					crawl.handledBy.Add(item.gameObject);
					break;
				case InputResponse.Handled:
					flag = true;
					break;
				case InputResponse.Blocked:
					response = InputResponse.Blocked;
					flag2 = true;
					return;
				}
			}
		}
		if (flag && !flag2)
		{
			response = InputResponse.Handled;
		}
		else
		{
			response = InputResponse.Unhandled;
		}
	}

	protected List<Component> GetInputContainersToHandle(Transform objectToStartAt, Transform objectToStopAt)
	{
		Transform transform = objectToStartAt;
		List<Component> list = new List<Component>();
		while (transform != null && transform != objectToStopAt)
		{
			Component component = FindComponent(transform, typeof(IInputContainer));
			if (component != null)
			{
				list.Insert(0, component);
			}
			transform = transform.parent;
		}
		return list;
	}

	protected Component FindComponent(Transform objectToTest, Type typeToFind)
	{
		Component[] components = objectToTest.GetComponents<Component>();
		Component[] array = components;
		foreach (Component component in array)
		{
			if (component is IInputContainer)
			{
				return component;
			}
		}
		return null;
	}

	protected virtual void SendToComponent(Component component, InputCrawl crawl, out InputResponse response)
	{
		((IInputHandler)component).HandleInput(crawl, out response);
	}

	private static Component[] FindAllSearchMethod(GameObject o)
	{
		return ObjectUtils.FindComponents<IInputHandler>(o) as Component[];
	}
}
