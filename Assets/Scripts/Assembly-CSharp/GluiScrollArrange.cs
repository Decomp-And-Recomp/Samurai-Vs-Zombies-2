using System.Collections.Generic;
using UnityEngine;

public class GluiScrollArrange : MonoBehaviour
{
	private float[] separatorSpacing = new float[2];

	private Vector2 iconSize;

	private Vector2 iconSpacing;

	private int maxRows;

	private int maxCols;

	private GluiScrollList scrollList;

	public float totalWidth;

	public float totalHeight;

	public void Init(Vector2 iconSize, Vector2 iconSpacing, float[] separatorSpacing, int maxRows, int maxCols, GluiScrollList scrollList)
	{
		this.iconSize = iconSize;
		this.iconSpacing = iconSpacing;
		this.scrollList = scrollList;
		this.maxRows = maxRows;
		this.maxCols = maxCols;
		separatorSpacing.CopyTo(this.separatorSpacing, 0);
	}

	public bool Full(GluiItemCatalog catalog)
	{
		return catalog.Count >= maxRows * maxCols;
	}

	public bool IsItemSelectable(GameObject obj)
	{
		Vector2 vector = new Vector2(iconSize.x / 2f, iconSize.y / 2f);
		Vector2 vector2 = new Vector2(scrollList.Size.x / 2f, scrollList.Size.y / 2f);
		Vector3 vector3 = scrollList.Offset;
		Vector3 vector4 = obj.transform.localPosition + vector3;
		if (vector4.x + vector.x < 0f - vector2.x)
		{
			return false;
		}
		if (vector4.x - vector.x > vector2.x)
		{
			return false;
		}
		if (vector4.y - vector.y > vector2.y)
		{
			return false;
		}
		if (vector4.y + vector.y < 0f - vector2.y)
		{
			return false;
		}
		return true;
	}

	private bool ClipItem(GameObject obj)
	{
		if (obj == null)
		{
			return true;
		}
		bool flag = IsItemSelectable(obj);
		return !flag;
	}

	public void ClipItems(GluiItemCatalog catalog)
	{
		foreach (GluiItemCatalog.Item item in catalog.Items)
		{
			GameObject obj = item.gameObject;
			item.Clipped = ClipItem(obj);
			if (!item.Clipped && scrollList.PostShowItem != null)
			{
				scrollList.PostShowItem(obj);
			}
		}
	}

	public void DistributeIcons(GluiItemCatalog catalog)
	{
		Vector2 size = scrollList.Size;
		float sx = (0f - size.x) / 2f + (iconSize.x / 2f + iconSpacing.x);
		float num = size.y / 2f - (iconSize.y / 2f + iconSpacing.y);
		float dx = iconSpacing.x + iconSize.x;
		float dy = iconSpacing.y + iconSize.y;
		int row = 0;
		int col = 0;
		totalHeight = 0f;
		totalWidth = 0f;
		List<GluiItemCatalog.Item> items = catalog.Items;
		if (items.Count == 0)
		{
			return;
		}
		int count = maxCols;
		if (count > catalog.Items.Count)
		{
			count = catalog.Items.Count;
		}
		totalWidth = (float)count * (iconSpacing.x + iconSize.x);
		if (items[0].type == GluiItemCatalog.Item.Type.Separator)
		{
			num = size.y / 2f - separatorSpacing[0];
		}
		float cx = sx;
		float cy = num;
		items.ForEachWithIndex(delegate(GluiItemCatalog.Item i, int index)
		{
			if (i.type == GluiItemCatalog.Item.Type.Icon)
			{
				i.gameObject.transform.localPosition = new Vector3(cx, cy, 0f);
				if (col == 0)
				{
					totalHeight += dy;
				}
				cx += dx;
				col++;
				if (col >= maxCols)
				{
					col = 0;
					row++;
					cx = sx;
					cy -= dy;
				}
			}
			else if (i.type == GluiItemCatalog.Item.Type.Separator)
			{
				if (col != 0)
				{
					cy -= separatorSpacing[0] + iconSize.y / 2f;
					totalHeight += separatorSpacing[0] / 2f + iconSize.y / 2f;
				}
				cx = sx + size.x / 2f - iconSize.x / 2f;
				i.gameObject.transform.localPosition = new Vector3(cx, cy, 0f);
				cx = sx;
				cy -= separatorSpacing[1] + iconSize.y / 2f + iconSpacing.y / 2f;
				col = 0;
				totalHeight += separatorSpacing[0] + separatorSpacing[1] + iconSize.y / 2f + iconSpacing.y / 2f;
			}
		});
	}
}
