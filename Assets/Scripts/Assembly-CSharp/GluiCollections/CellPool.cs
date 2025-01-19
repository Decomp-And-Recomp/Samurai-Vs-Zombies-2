using System.Collections.Generic;
using UnityEngine;

namespace GluiCollections
{
	public class CellPool
	{
		private Dictionary<string, List<Cell>> mPool = new Dictionary<string, List<Cell>>();

		public void Destroy()
		{
			Clear();
		}

		public void DeactivateAll()
		{
			foreach (KeyValuePair<string, List<Cell>> item in mPool)
			{
				foreach (Cell item2 in item.Value)
				{
					item2.DataIndex = -1;
					item2.Visible = false;
					item2.Obj.transform.parent = null;
				}
			}
		}

		public void Clear()
		{
			foreach (KeyValuePair<string, List<Cell>> item in mPool)
			{
				foreach (Cell item2 in item.Value)
				{
					Object.DestroyImmediate(item2.Obj);
				}
				item.Value.Clear();
			}
			mPool.Clear();
		}

		public void Add(Cell cell, string type)
		{
			if (!mPool.ContainsKey(type))
			{
				mPool.Add(type, new List<Cell>());
			}
			mPool[type].Add(cell);
		}

		public Cell GetAny(string prefabPath)
		{
			if (mPool.ContainsKey(prefabPath))
			{
				List<Cell> list = mPool[prefabPath];
				if (list.Count > 0)
				{
					return list[0];
				}
			}
			return CreateIfPossible(prefabPath);
		}

		public Cell GetAvailable(string prefabPath)
		{
			if (mPool.ContainsKey(prefabPath))
			{
				List<Cell> list = mPool[prefabPath];
				foreach (Cell item in list)
				{
					if (!item.Obj.activeSelf)
					{
						return item;
					}
				}
			}
			return CreateIfPossible(prefabPath);
		}

		private Cell CreateIfPossible(string prefabPath)
		{
			Cell cell = new Cell();
			cell.Obj = Object.Instantiate(ResourceCache.GetCachedResource(prefabPath, 1).Resource) as GameObject;
			cell.Visible = false;
			cell.element = cell.Obj.GetComponent<GluiElement_Base>();
			GluiScreen component = cell.Obj.GetComponent<GluiScreen>();
			cell.Size = new Vector2(component.nativeWidth, component.nativeHeight);
			Add(cell, prefabPath);
			return cell;
		}
	}
}
