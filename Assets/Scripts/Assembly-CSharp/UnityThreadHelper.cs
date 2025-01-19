using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityThreading;

public class UnityThreadHelper : SingletonSpawningMonoBehaviour<UnityThreadHelper>
{
	private Dispatcher dispatcher;

	private TaskDistributor taskDistributor;

	private List<ThreadBase> registeredThreads = new List<ThreadBase>();

	public static Dispatcher Dispatcher
	{
		get
		{
			return SingletonSpawningMonoBehaviour<UnityThreadHelper>.Instance.CurrentDispatcher;
		}
	}

	public static TaskDistributor TaskDistributor
	{
		get
		{
			return SingletonSpawningMonoBehaviour<UnityThreadHelper>.Instance.CurrentTaskDistributor;
		}
	}

	public Dispatcher CurrentDispatcher
	{
		get
		{
			return dispatcher;
		}
	}

	public TaskDistributor CurrentTaskDistributor
	{
		get
		{
			return taskDistributor;
		}
	}

	public static int MainThreadId { get; private set; }

	public static void Activate()
	{
		if (!(SingletonSpawningMonoBehaviour<UnityThreadHelper>.Instance == null))
		{
			SingletonSpawningMonoBehaviour<UnityThreadHelper>.Instance.EnsureHelper();
		}
	}

	private void EnsureHelper()
	{
		if (dispatcher == null)
		{
			dispatcher = new Dispatcher();
		}
		if (taskDistributor == null)
		{
			taskDistributor = new TaskDistributor();
		}
		MainThreadId = Thread.CurrentThread.ManagedThreadId;
	}

	public static ActionThread CreateThread(Action<ActionThread> action, bool autoStartThread)
	{
		SingletonSpawningMonoBehaviour<UnityThreadHelper>.Instance.EnsureHelper();
		Action<ActionThread> action2 = delegate(ActionThread currentThread)
		{
			try
			{
				action(currentThread);
			}
			catch (Exception)
			{
			}
		};
		ActionThread actionThread = new ActionThread(action2, autoStartThread);
		SingletonSpawningMonoBehaviour<UnityThreadHelper>.Instance.RegisterThread(actionThread);
		return actionThread;
	}

	public static ActionThread CreateThread(Action<ActionThread> action)
	{
		return CreateThread(action, true);
	}

	public static ActionThread CreateThread(Action action, bool autoStartThread)
	{
		return CreateThread((Action<ActionThread>)delegate
		{
			action();
		}, autoStartThread);
	}

	public static ActionThread CreateThread(Action action)
	{
		return CreateThread((Action<ActionThread>)delegate
		{
			action();
		}, true);
	}

	public static void CallOnMainThread(Action action)
	{
		if (action == null)
		{
			return;
		}
		if (!Dispatcher.IsMainThread)
		{
			if (SingletonSpawningMonoBehaviour<UnityThreadHelper>.Exists)
			{
				Dispatcher.Dispatch(action);
			}
		}
		else
		{
			action();
		}
	}

	private void RegisterThread(ThreadBase thread)
	{
		registeredThreads.Add(thread);
	}

	protected override void OnDestroy()
	{
		if (!Application.isEditor || !SingletonSpawningMonoBehaviour<UnityThreadHelper>.applicationQuitting)
		{
			foreach (ThreadBase registeredThread in registeredThreads)
			{
				registeredThread.Dispose();
			}
			if (dispatcher != null)
			{
				dispatcher.Dispose();
			}
			dispatcher = null;
			if (taskDistributor != null)
			{
				taskDistributor.Dispose();
			}
			taskDistributor = null;
		}
		base.OnDestroy();
	}

	private void Update()
	{
		if (dispatcher != null)
		{
			dispatcher.ProcessTasks();
		}
		int num = 0;
		while (num < registeredThreads.Count)
		{
			ThreadBase threadBase = registeredThreads[num];
			if (!threadBase.IsAlive)
			{
				threadBase.Dispose();
				registeredThreads.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}
}
