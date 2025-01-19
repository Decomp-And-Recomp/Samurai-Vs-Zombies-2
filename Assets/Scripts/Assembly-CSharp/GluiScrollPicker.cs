using System;
using System.Collections.Generic;
using UnityEngine;

public class GluiScrollPicker : MonoBehaviour
{
	public Vector2 snapStrength = new Vector2(0.1f, 0.1f);

	public string onReticleSelect;

	public int reticleIndexOffset;

	public GameObject handler;

	private GluiScrollList scrollList;

	private int? currentIndex;

	private void Awake()
	{
		scrollList = GetComponent<GluiScrollList>();
		if (!(scrollList == null))
		{
			GluiScrollList gluiScrollList = scrollList;
			gluiScrollList.FlingOverride = (GluiScrollList.FlingOverrideHandler)Delegate.Combine(gluiScrollList.FlingOverride, new GluiScrollList.FlingOverrideHandler(SnapFling));
			GluiScrollList gluiScrollList2 = scrollList;
			gluiScrollList2.OffsetOverride = (GluiScrollList.OffsetOverrideHandler)Delegate.Combine(gluiScrollList2.OffsetOverride, new GluiScrollList.OffsetOverrideHandler(SetPickerOffset));
			GluiScrollList gluiScrollList3 = scrollList;
			gluiScrollList3.PostClear = (Action)Delegate.Combine(gluiScrollList3.PostClear, new Action(AddEmptyIcon));
			GluiScrollList gluiScrollList4 = scrollList;
			gluiScrollList4.GetItemOverride = (Func<List<GluiItemCatalog.Item>, int, GluiItemCatalog.Item>)Delegate.Combine(gluiScrollList4.GetItemOverride, new Func<List<GluiItemCatalog.Item>, int, GluiItemCatalog.Item>(GetItem));
			GluiScrollList gluiScrollList5 = scrollList;
			gluiScrollList5.GetItemsOverride = (Func<List<GluiItemCatalog.Item>, List<GluiItemCatalog.Item>>)Delegate.Combine(gluiScrollList5.GetItemsOverride, new Func<List<GluiItemCatalog.Item>, List<GluiItemCatalog.Item>>(GetItems));
			GluiScrollList gluiScrollList6 = scrollList;
			gluiScrollList6.GetItemCountOverride = (Func<List<GluiItemCatalog.Item>, int>)Delegate.Combine(gluiScrollList6.GetItemCountOverride, new Func<List<GluiItemCatalog.Item>, int>(GetItemCount));
		}
	}

	private void Update()
	{
		Vector2 vector = scrollList.iconSize + scrollList.iconSpacing;
		Vector2 vector2 = new Vector2(scrollList.Offset.x / vector.x, scrollList.Offset.y / vector.y);
		Vector2 vector3 = new Vector2((int)(vector2.x - 0.5f), (int)(vector2.y + 0.5f));
		int num = (int)(0f - vector3.x) + (int)vector3.y * scrollList.maxCols;
		if (currentIndex != num)
		{
			currentIndex = num;
			if (handler != null && !string.IsNullOrEmpty(onReticleSelect))
			{
				handler.SendMessage(StripDelegateName(onReticleSelect), currentIndex);
			}
		}
	}

	private List<GluiItemCatalog.Item> GetItems(List<GluiItemCatalog.Item> orgItems)
	{
		return orgItems.GetRange(reticleIndexOffset, orgItems.Count - reticleIndexOffset);
	}

	private GluiItemCatalog.Item GetItem(List<GluiItemCatalog.Item> orgItems, int n)
	{
		return orgItems[n + reticleIndexOffset];
	}

	private int GetItemCount(List<GluiItemCatalog.Item> orgItems)
	{
		return orgItems.Count - reticleIndexOffset;
	}

	private void AddEmptyIcon()
	{
		for (int i = 0; i < reticleIndexOffset; i++)
		{
			scrollList.AddObject(new GameObject("empty_icon"));
		}
	}

	private string StripDelegateName(string delegateName)
	{
		int num = delegateName.IndexOf('(');
		if (num == -1)
		{
			return delegateName;
		}
		return delegateName.Substring(0, num);
	}

	private void SnapFling(GluiCursorState state, ref Vector2 fling)
	{
		if (state != GluiCursorState.Held && !(snapStrength == Vector2.zero))
		{
			Vector2 vector = fling;
			Vector2 vector2 = scrollList.iconSize + scrollList.iconSpacing;
			Vector2 vector3 = new Vector2(scrollList.Offset.x / vector2.x, scrollList.Offset.y / vector2.y);
			float val = scrollList.Items.Count / scrollList.maxRows - 1;
			float val2 = scrollList.Items.Count / scrollList.maxCols - 1;
			vector3 = new Vector2(Math.Min(vector3.x, val), Math.Min(vector3.y, val2));
			Vector2 vector4 = new Vector2((int)(vector3.x - 0.5f), (int)(vector3.y + 0.5f));
			Vector2 vector5 = new Vector2(vector4.x * vector2.x, vector4.y * vector2.y);
			Vector2 vector6 = new Vector2((vector5.x - scrollList.Offset.x) * snapStrength.x, (vector5.y - scrollList.Offset.y) * snapStrength.y);
			if (Math.Abs(vector.x) < 0.5f)
			{
				vector.x = vector6.x;
			}
			if (Math.Abs(vector.y) < 0.5f)
			{
				vector.y = vector6.y;
			}
			fling = vector;
		}
	}

	private void SetPickerOffset(ref Vector2 ofs)
	{
		Vector2 vector = ofs;
		int num = scrollList.maxCols;
		if (num > scrollList.Items.Count + reticleIndexOffset)
		{
			num = scrollList.Items.Count + reticleIndexOffset;
		}
		float num2 = (float)num * (scrollList.iconSpacing.x + scrollList.iconSize.x);
		float num3 = num2 - (scrollList.iconSize.x + scrollList.iconSpacing.x) * (float)(1 + reticleIndexOffset);
		if (num3 < 0f)
		{
			num3 = 0f;
		}
		float num4 = scrollList.TotalHeight - scrollList.Size.y + scrollList.iconSpacing.y;
		if (num4 < 0f)
		{
			num4 = 0f;
		}
		vector.x = Mathf.Clamp(vector.x, 0f - num3, 0f);
		vector.y = Mathf.Clamp(vector.y, 0f, num4);
		ofs = vector;
	}
}
