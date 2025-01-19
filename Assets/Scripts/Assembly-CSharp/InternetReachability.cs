using System;
using System.Collections.Generic;
using UnityEngine;

public class InternetReachability : MonoBehaviour
{
	public enum Reachability
	{
		Unknown = 0,
		NotReachable = 1,
		ReachableViaWiFi = 2,
		ReachableViaWWAN = 3
	}

	public string hostName = "api.gamespy.net";

	private static InternetReachability instance;

	private List<GameObject> handlers = new List<GameObject>();

	public Reachability status { get; private set; }

	public bool Online
	{
		get
		{
			if (status == Reachability.ReachableViaWiFi || status == Reachability.ReachableViaWWAN)
			{
				return true;
			}
			return false;
		}
	}

	public static InternetReachability Instance
	{
		get
		{
			return instance;
		}
	}

	public void AddChangeHandler(GameObject handler)
	{
		handlers.Add(handler);
		RemoveNullHandlers();
	}

	public void RemoveChangeHandler(GameObject handler)
	{
		handlers.Remove(handler);
		RemoveNullHandlers();
	}

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		UnityEngine.Object.DontDestroyOnLoad(instance);
		if (!string.IsNullOrEmpty(hostName))
		{
			NUF.SetupReachabilityHandler(hostName, base.gameObject.name, "HandleReachabilityChange");
		}
	}

	private void HandleReachabilityChange(string rechability)
	{
		Reachability returnValue = Reachability.Unknown;
		if (!TryParse<Reachability>(rechability, out returnValue))
		{
			returnValue = Reachability.Unknown;
		}
		status = returnValue;
		foreach (GameObject handler in handlers)
		{
			if (handler != null)
			{
				handler.SendMessage("ReachabilityChanged", status);
			}
		}
		RemoveNullHandlers();
	}

	private bool TryParse<T>(string stringValue, out T returnValue)
	{
		try
		{
			returnValue = (T)Enum.Parse(typeof(T), stringValue);
			return true;
		}
		catch
		{
			returnValue = default(T);
			return false;
		}
	}

	private void RemoveNullHandlers()
	{
		handlers.RemoveAll((GameObject handler) => (handler == null) ? true : false);
	}
}
