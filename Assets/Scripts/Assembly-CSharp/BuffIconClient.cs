using System.Collections.Generic;
using UnityEngine;

public class BuffIconClient
{
	private class Entry
	{
		public string iconFile;

		public int priority;

		public float killTimer;

		public int serialNumber;

		public bool asEffect = true;
	}

	private List<Entry> mEntries = new List<Entry>();

	private TypedWeakReference<CharacterModelController> mTarget;

	private string mActiveIcon = string.Empty;

	private GameObject mActiveGO;

	private int mNextSerialNumber;

	public BuffIconClient(CharacterModelController mc)
	{
		mTarget = new TypedWeakReference<CharacterModelController>(mc);
	}

	public void Update()
	{
		bool flag = false;
		for (int num = mEntries.Count - 1; num >= 0; num--)
		{
			Entry entry = mEntries[num];
			if (entry.killTimer > 0f)
			{
				entry.killTimer -= Time.deltaTime;
				if (entry.killTimer <= 0f)
				{
					Hide(entry.iconFile, false);
					flag = true;
				}
			}
		}
		if (flag)
		{
			RefreshVisibleIcon();
		}
	}

	public void Clear()
	{
		mEntries.Clear();
		RefreshVisibleIcon();
	}

	public void Hide(string iconFile)
	{
		Hide(iconFile, true);
	}

	public void Show(string iconFile)
	{
		Show(iconFile, 0f, 0);
	}

	public void Show(string iconFile, float killTimer)
	{
		Show(iconFile, killTimer, 0);
	}

	public void Show(string iconFile, float killTimer, int priority)
	{
		foreach (Entry mEntry in mEntries)
		{
			if (mEntry.iconFile == iconFile)
			{
				mEntry.killTimer = killTimer;
				mEntry.priority = priority;
				RefreshVisibleIcon();
				return;
			}
		}
		Entry entry = new Entry();
		entry.iconFile = iconFile;
		entry.killTimer = killTimer;
		entry.priority = priority;
		entry.serialNumber = mNextSerialNumber++;
		mEntries.Add(entry);
		RefreshVisibleIcon();
	}

	private void RefreshVisibleIcon()
	{
		if (mEntries.Count == 0)
		{
			KillActiveIcon();
			return;
		}
		mEntries.Sort(delegate(Entry a, Entry b)
		{
			int num = b.priority.CompareTo(a.priority);
			return (num != 0) ? num : b.serialNumber.CompareTo(a.serialNumber);
		});
		Entry entry = mEntries[0];
		if (entry.iconFile != mActiveIcon)
		{
			SetActiveIcon(entry);
		}
	}

	private void SetActiveIcon(Entry e)
	{
		KillActiveIcon();
		mActiveIcon = e.iconFile;
		if (!object.ReferenceEquals(mTarget.ptr, null))
		{
			if (e.asEffect)
			{
				mActiveGO = mTarget.ptr.SpawnEffectAtJoint(WeakGlobalInstance<BuffIconManager>.Instance.GetPrefab(mActiveIcon), "buff_icon", true);
			}
			else
			{
				mActiveGO = mTarget.ptr.InstantiateObjectOnJoint(WeakGlobalInstance<BuffIconManager>.Instance.GetPrefab(mActiveIcon), "buff_icon");
			}
		}
	}

	private void KillActiveIcon()
	{
		if (mActiveIcon != string.Empty)
		{
			mActiveIcon = string.Empty;
			Object.Destroy(mActiveGO);
			mActiveGO = null;
		}
	}

	private void Hide(string iconFile, bool cleanup)
	{
		for (int num = mEntries.Count - 1; num >= 0; num--)
		{
			Entry entry = mEntries[num];
			if (entry.iconFile == iconFile)
			{
				mEntries.RemoveAt(num);
			}
		}
		if (cleanup)
		{
			RefreshVisibleIcon();
		}
	}
}
