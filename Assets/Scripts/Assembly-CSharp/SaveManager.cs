using System;
using System.Collections.Generic;

public class SaveManager : SingletonSpawningMonoBehaviour<SaveManager>
{
	private Dictionary<string, SaveProvider> saves = new Dictionary<string, SaveProvider>();

	private Queue<Func<bool>> updateTasks = new Queue<Func<bool>>();

	public SaveProvider AddSave(SaveProvider s)
	{
		if (s == null || string.IsNullOrEmpty(s.Name))
		{
			return null;
		}
		saves[s.Name] = s;
		return s;
	}

	public void RemoveSave(SaveProvider s)
	{
		if (s != null && !string.IsNullOrEmpty(s.Name))
		{
			saves.Remove(s.Name);
		}
	}

	public SaveProvider GetSave(string saveName)
	{
		SaveProvider value = null;
		if (!saves.TryGetValue(saveName, out value))
		{
		}
		return value;
	}

	public void EnqueueUpdateTask(Func<bool> onUpdate)
	{
		updateTasks.Enqueue(onUpdate);
	}

	private void LateUpdate()
	{
		DateTime now = DateTime.Now;
		foreach (SaveProvider value in saves.Values)
		{
			if (value.AutoSaveTime.HasValue && now >= value.AutoSaveTime.Value)
			{
				value.Save();
			}
		}
		UpdateTasks();
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			OnExit();
		}
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
		OnExit();
	}

	private void OnExit()
	{
		foreach (SaveProvider value in saves.Values)
		{
			if (value.SaveOnExit)
			{
				value.Save();
			}
		}
		UpdateTasks();
	}

	private void UpdateTasks()
	{
		int num = 0;
		while (num < updateTasks.Count)
		{
			Func<bool> func = updateTasks.Peek();
			bool flag = true;
			if (func != null)
			{
				flag = func();
			}
			if (flag)
			{
				updateTasks.Dequeue();
			}
			else
			{
				num++;
			}
		}
	}
}
