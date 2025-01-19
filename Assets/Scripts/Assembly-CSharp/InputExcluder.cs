using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InputExcluder
{
	private IndexSortedObjectList exclusiveInputContainers = new IndexSortedObjectList();

	private IndexSortedObjectList inclusiveInputContainers = new IndexSortedObjectList();

	public bool IsEmpty
	{
		get
		{
			return exclusiveInputContainers.IsEmpty;
		}
	}

	public void AddExclusiveInputContainer(GameObject container, int exclusiveLayer, InputLayerType layerType)
	{
		switch (layerType)
		{
		case InputLayerType.Exclusive:
			exclusiveInputContainers.Add(container, exclusiveLayer);
			break;
		case InputLayerType.Inclusive:
			inclusiveInputContainers.Add(container, exclusiveLayer);
			break;
		case InputLayerType.Normal:
			inclusiveInputContainers.Remove(container, exclusiveLayer);
			exclusiveInputContainers.Remove(container, exclusiveLayer);
			break;
		}
	}

	public void RemoveExclusiveInputContainer(GameObject container, int exclusiveLayer)
	{
		if (exclusiveLayer < 0)
		{
			exclusiveInputContainers.Remove(container);
			inclusiveInputContainers.Remove(container);
		}
		else
		{
			exclusiveInputContainers.Remove(container, exclusiveLayer);
			inclusiveInputContainers.Remove(container, exclusiveLayer);
		}
	}

	private bool TestAllowInput(InputTrace.HitInfo hit, List<GameObject> allowedList)
	{
		foreach (GameObject allowed in allowedList)
		{
			if (hit.target.transform.IsChildOf(allowed.transform))
			{
				return true;
			}
		}
		return false;
	}

	public List<GameObject> BuildAllowedInputGroups()
	{
		List<GameObject> allowedInputList = new List<GameObject>();
		IndexedObjectList highest = exclusiveInputContainers.Highest;
		allowedInputList.AddRange(highest.objects);
		List<IndexedObjectList> list2 = inclusiveInputContainers.HigherOrEqual(highest.index);
		list2.ForEach(delegate(IndexedObjectList list)
		{
			allowedInputList.AddRange(list.objects);
		});
		return allowedInputList;
	}

	public void FilterInput(ref List<InputTrace.HitInfo> hits)
	{
		if (!IsEmpty)
		{
			List<GameObject> allowedList = BuildAllowedInputGroups();
			hits = hits.FindAll((InputTrace.HitInfo hit) => TestAllowInput(hit, allowedList));
		}
	}
}
