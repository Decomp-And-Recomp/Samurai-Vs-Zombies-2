using System.Collections.Generic;
using UnityEngine;

public class ResourceTemplate
{
	public GameObject prefab;

	public int amount;

	public float weight;

	public float lifetime = 5f;

	public float contentsTotalWeight;

	public Dictionary<string, float> contents;

	public float postDeathContentsTotalWeight;

	public Dictionary<string, float> postDeathContents;
}
