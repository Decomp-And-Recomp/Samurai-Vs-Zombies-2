using System.Collections.Generic;
using UnityEngine;

public class GripNetwork_RequestManager : MonoBehaviour
{
	private static GripNetwork_RequestManager smInst;

	private List<DisposableMonoBehaviour> mRequests = new List<DisposableMonoBehaviour>();

	public static GripNetwork_RequestManager Instance
	{
		get
		{
			if (smInst == null)
			{
				GameObject gameObject = new GameObject("RequestManager");
				Object.DontDestroyOnLoad(gameObject);
				smInst = gameObject.AddComponent<GripNetwork_RequestManager>();
			}
			return smInst;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (mRequests.Count > 0)
		{
			if (mRequests[0] == null)
			{
				mRequests.RemoveAt(0);
				if (mRequests.Count == 0)
				{
					base.enabled = false;
				}
				else
				{
					mRequests[0].enabled = true;
				}
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	public void QueueRequest(DisposableMonoBehaviour request)
	{
		mRequests.Add(request);
		if (mRequests.Count > 1)
		{
			request.enabled = false;
		}
		base.enabled = true;
	}
}
