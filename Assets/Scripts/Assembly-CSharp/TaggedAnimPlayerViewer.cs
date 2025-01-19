using System.Collections.Generic;
using UnityEngine;

public class TaggedAnimPlayerViewer : MonoBehaviour
{
	public GameObject[] taggedObjects;

	private string[] taggedNames;

	private List<string> animNames;

	private int selectedModel = -1;

	private int selectedAnim = -1;

	private Vector2 modelScrollPos = Vector2.zero;

	private Vector2 animScrollPos = Vector2.zero;

	private GameObject modelViewed;

	private TaggedAnimPlayer animPlayer;

	private void Start()
	{
		taggedNames = new string[taggedObjects.Length];
		for (int i = 0; i < taggedNames.Length; i++)
		{
			taggedNames[i] = taggedObjects[i].name;
		}
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
		GUI.Box(new Rect(0f, 0f, 256f, Screen.height), "Anim Viewer");
		int num = 24;
		int num2 = 224;
		int num3 = num * taggedNames.Length;
		modelScrollPos = GUI.BeginScrollView(new Rect(0f, 32f, 256f, Screen.height / 2 - 32), modelScrollPos, new Rect(0f, 0f, 256f, num3));
		int num4 = GUI.SelectionGrid(new Rect(0f, 0f, num2, num3), selectedModel, taggedNames, 1);
		GUI.EndScrollView();
		if (num4 != selectedModel)
		{
			selectedModel = num4;
			if (modelViewed != null)
			{
				Object.Destroy(modelViewed);
			}
			modelViewed = Object.Instantiate(taggedObjects[selectedModel]) as GameObject;
			modelViewed.transform.position = new Vector3(55f, -72.65f, 322f);
			modelViewed.transform.eulerAngles = new Vector3(0f, 180f, 0f);
			animPlayer = modelViewed.GetComponent<TaggedAnimPlayer>();
			animPlayer.GetComponent<Animation>().playAutomatically = false;
			animPlayer.jointAnimation.playAutomatically = false;
			animPlayer.SetBaseAnim("idle");
			selectedAnim = 0;
			animNames = new List<string>();
			foreach (AnimationState item in modelViewed.GetComponent<Animation>())
			{
				if (!animNames.Contains(item.clip.name))
				{
					animNames.Add(item.clip.name);
				}
			}
		}
		if (modelViewed != null)
		{
			int num5 = num * animNames.Count;
			animScrollPos = GUI.BeginScrollView(new Rect(0f, Screen.height / 2 + 16, 256f, Screen.height / 2 - 16), animScrollPos, new Rect(0f, 0f, 256f, num5));
			num4 = GUI.SelectionGrid(new Rect(0f, 0f, num2, num5), selectedAnim, animNames.ToArray(), 1);
			GUI.EndScrollView();
			if (num4 != selectedAnim)
			{
				selectedAnim = num4;
				animPlayer.PlayAnim(animNames[selectedAnim]);
			}
		}
	}
}
