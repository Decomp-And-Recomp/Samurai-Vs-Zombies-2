using System;
using System.Collections.Generic;
using UnityEngine;

public class InputCrawl
{
	public InputEvent inputEvent;

	public List<InputTrace.HitInfo> hits;

	private List<InputTrace.HitInfo> originalHits;

	public List<GameObject> handledBy;

	public Transform stopContainerSearchAtObject;

	public bool containerCannotStopInput;

	public List<InputCrawl> queuedSubCrawls = new List<InputCrawl>();

	public InputTrace.HitInfo CurrentHit
	{
		get
		{
			if (hits.Count > 0)
			{
				return hits[0];
			}
			throw new Exception();
		}
	}

	public List<InputTrace.HitInfo> OriginalHits
	{
		get
		{
			return originalHits;
		}
		set
		{
			originalHits = value;
		}
	}

	public InputCrawl(InputEvent inputEvent, List<InputTrace.HitInfo> hits)
	{
		this.inputEvent = inputEvent;
		this.hits = hits;
		handledBy = new List<GameObject>();
		queuedSubCrawls = new List<InputCrawl>();
		originalHits = new List<InputTrace.HitInfo>();
		originalHits.AddRange(hits);
	}

	public void QueueSubCrawl(InputEvent.EEventType inputEventType, List<InputTrace.HitInfo> hits, List<InputTrace.HitInfo> originalHits, Transform sender)
	{
		InputCrawl inputCrawl = new InputCrawl(new InputEvent(inputEventType, inputEvent.Position, inputEvent.CursorIndex), hits);
		inputCrawl.stopContainerSearchAtObject = sender;
		inputCrawl.OriginalHits = originalHits;
		queuedSubCrawls.Add(inputCrawl);
	}

	public InputTrace.HitInfo Pop()
	{
		if (hits.Count > 0)
		{
			InputTrace.HitInfo result = hits[0];
			hits.RemoveAt(0);
			return result;
		}
		return null;
	}

	public List<InputTrace.HitInfo> Find_OriginalHits_ChildrenOf(GameObject parent)
	{
		return originalHits.FindAll((InputTrace.HitInfo hit) => hit.target.transform.IsChildOf(parent.transform));
	}

	public InputTrace.HitInfo Find_OriginalHits(GameObject target)
	{
		return originalHits.Find((InputTrace.HitInfo hit) => hit.target == target);
	}
}
