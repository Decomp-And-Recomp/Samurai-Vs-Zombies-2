using System.Collections.Generic;
using UnityEngine;

public class IndexSortedObjectList
{
	private List<IndexedObjectList> indexedObjectList = new List<IndexedObjectList>();

	public bool IsEmpty
	{
		get
		{
			return indexedObjectList.Count == 0;
		}
	}

	public IndexedObjectList Highest
	{
		get
		{
			if (indexedObjectList.Count > 0)
			{
				return indexedObjectList[indexedObjectList.Count - 1];
			}
			return null;
		}
	}

	public void Add(GameObject newObject, int index)
	{
		IndexedObjectList indexedObjectList = Find(index);
		if (indexedObjectList == null)
		{
			indexedObjectList = Add(index);
		}
		if (!indexedObjectList.objects.Contains(newObject))
		{
			indexedObjectList.objects.Add(newObject);
		}
	}

	private IndexedObjectList Add(int index)
	{
		IndexedObjectList indexedObjectList = new IndexedObjectList();
		indexedObjectList.index = index;
		int num = this.indexedObjectList.FindIndex((IndexedObjectList thisList) => thisList.index > index);
		if (num == -1)
		{
			this.indexedObjectList.Add(indexedObjectList);
		}
		else
		{
			this.indexedObjectList.Insert(num, indexedObjectList);
		}
		return indexedObjectList;
	}

	public void Remove(GameObject newObject)
	{
		List<IndexedObjectList> list2 = new List<IndexedObjectList>();
		foreach (IndexedObjectList indexedObject in indexedObjectList)
		{
			indexedObject.objects.Remove(newObject);
			if (indexedObject.objects.Count == 0)
			{
				list2.Add(indexedObject);
			}
		}
		list2.ForEach(delegate(IndexedObjectList list)
		{
			indexedObjectList.Remove(list);
		});
	}

	public void Remove(GameObject newObject, int index)
	{
		IndexedObjectList indexedObjectList = Find(index);
		if (indexedObjectList != null)
		{
			indexedObjectList.objects.Remove(newObject);
			if (indexedObjectList.objects.Count == 0)
			{
				this.indexedObjectList.Remove(indexedObjectList);
			}
		}
	}

	public IndexedObjectList Find(int index)
	{
		return indexedObjectList.Find((IndexedObjectList indexedList) => indexedList.index == index);
	}

	public List<IndexedObjectList> HigherOrEqual(int index)
	{
		return indexedObjectList.FindAll((IndexedObjectList list) => list.index >= index);
	}
}
