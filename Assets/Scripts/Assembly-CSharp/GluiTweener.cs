using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Glui/Transition Tweener")]
public class GluiTweener : GluiTransition
{
	public enum ObjectInheritance
	{
		Local_InheritFromParent = 0,
		Global_IgnoreParent = 1
	}

	public enum ColorChannelGroup
	{
		None = 0,
		Alpha = 1,
		ColorRGB = 2,
		FullARGB = 3
	}

	[Serializable]
	public class Frame
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale = Vector3.one;

		public Color color = Color.white;
	}

	[Serializable]
	public class TransitionPath
	{
		public Frame[] frames = new Frame[2];

		public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public float time = 1f;

		public float delayBeforeStart;
	}

	public string actionOnDoneForward;

	public string actionOnDoneBackward;

	public GameObject[] objectsToTween;

	public bool usePosition = true;

	public bool useRotation;

	public bool useScale;

	public ColorChannelGroup colorChannels = ColorChannelGroup.Alpha;

	public bool recursiveColor = true;

	public bool replaceColor;

	private Color? pendingColor;

	public ObjectInheritance objectInheritance;

	public TransitionPath transitionForward;

	public TransitionPath transitionBack;

	private float timer;

	private Position goalPos;

	private Position curPos;

	private float timeToStart;

	private TransitionToUse transitionToUse = TransitionToUse.Main;

	private TransitionPath transitionPathCurrent;

	private bool transitionPathForward = true;

	private Dictionary<int, List<Color>> startingColors = new Dictionary<int, List<Color>>();

	private List<GluiWidget> colorObjectsToTween = new List<GluiWidget>();

	[SerializeField]
	private Frame[] frames;

	[SerializeField]
	private AnimationCurve curve;

	[SerializeField]
	private float time;

	[SerializeField]
	private float delayBeforeStart;

	public Position CurrentPosition
	{
		get
		{
			return curPos;
		}
	}

	public Position GoalPosition
	{
		get
		{
			return goalPos;
		}
	}

	public override bool IsDone
	{
		get
		{
			return timer == 0f;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (frames != null && curve != null && frames.Length > 0)
		{
			transitionForward = new TransitionPath();
			transitionForward.curve = curve;
			transitionForward.delayBeforeStart = delayBeforeStart;
			transitionForward.frames = frames;
			transitionForward.time = time;
			curve = null;
			delayBeforeStart = 0f;
			frames = null;
			time = 0f;
		}
	}

	protected virtual void Update()
	{
		if (base.Enabled)
		{
			float num = GluiTime.deltaTime;
			if (num == 0f)
			{
				num = Time.maximumDeltaTime;
			}
			UpdateTweener(num);
		}
	}

	protected override void OnDisable()
	{
		if (!IsDone && onDoneCallback != null)
		{
			onDoneCallback(curPos);
		}
		base.OnDisable();
	}

	protected void UpdateTweener(float deltaTime)
	{
		if (Time.realtimeSinceStartup < timeToStart || transitionPathCurrent == null)
		{
			return;
		}
		Frame[] array = transitionPathCurrent.frames;
		AnimationCurve animationCurve = transitionPathCurrent.curve;
		float num = transitionPathCurrent.time;
		if (!(timer > 0f))
		{
			return;
		}
		timer -= deltaTime;
		if (colorChannels != 0)
		{
			UpdateColorObjectsToTween();
		}
		if (timer <= 0f)
		{
			Position position = goalPos;
			timer = 0f;
			curPos = goalPos;
			goalPos = Position.None;
			Frame frame = array[(int)curPos];
			SetLocation(frame.position, frame.rotation, frame.scale);
			pendingColor = array[(int)curPos].color;
			if (onDoneCallback != null)
			{
				onDoneCallback(curPos);
			}
			switch (position)
			{
			case Position.End:
				GluiActionSender.SendGluiAction(actionOnDoneForward, base.gameObject, null);
				break;
			case Position.Start:
				GluiActionSender.SendGluiAction(actionOnDoneBackward, base.gameObject, null);
				break;
			}
		}
		else
		{
			Frame frame2 = array[(int)curPos];
			Frame frame3 = array[(int)goalPos];
			float num2 = ((!(num > 0f)) ? 0f : ((num - timer) / num));
			float num3 = num2 * (float)animationCurve.length;
			float num4 = animationCurve.Evaluate(num3);
			Vector3 position2 = frame2.position + (frame3.position - frame2.position) * num4;
			Quaternion rotation = Quaternion.Slerp(frame2.rotation, frame3.rotation, num4);
			Vector3 scale = frame2.scale + (frame3.scale - frame2.scale) * num4;
			Color value = frame2.color + (frame3.color - frame2.color) * num4;
			SetLocation(position2, rotation, scale);
			pendingColor = value;
		}
	}

	protected void LateUpdate()
	{
		if (pendingColor.HasValue)
		{
			if (colorChannels != 0)
			{
				SetColor(pendingColor.Value);
			}
			pendingColor = null;
		}
	}

	public bool Go(Position to, bool reverse)
	{
		if (to == Position.Other)
		{
			if (curPos == Position.Start)
			{
				return Go(Position.End, reverse);
			}
			if (curPos == Position.End)
			{
				return Go(Position.Start, reverse);
			}
			return false;
		}
		GetTransitionImplemented(to, out transitionToUse, out transitionPathForward);
		if (reverse)
		{
			transitionPathForward = !transitionPathForward;
		}
		if (transitionToUse == TransitionToUse.None)
		{
			curPos = to;
			return false;
		}
		transitionPathCurrent = GetTransitionPath(transitionToUse);
		if (transitionPathCurrent == null)
		{
			return false;
		}
		if (!transitionPathForward)
		{
			curPos = Position.End;
			goalPos = Position.Start;
		}
		else
		{
			curPos = Position.Start;
			goalPos = Position.End;
		}
		if ((timer = transitionPathCurrent.time) <= 0f)
		{
			timer = GluiTime.deltaTime;
		}
		UpdateTweener(0f);
		float num = transitionPathCurrent.delayBeforeStart;
		if (num > 0f)
		{
			timeToStart = Time.realtimeSinceStartup + num;
		}
		return true;
	}

	public TransitionPath GetTransitionPath(TransitionToUse transition)
	{
		switch (transition)
		{
		case TransitionToUse.Second:
			return transitionBack;
		case TransitionToUse.Main:
			return transitionForward;
		default:
			return null;
		}
	}

	public void SaveTransitionPath(TransitionToUse transition, TransitionPath path)
	{
		switch (transition)
		{
		case TransitionToUse.Second:
			transitionBack = path;
			break;
		case TransitionToUse.Main:
			transitionForward = path;
			break;
		}
	}

	public void FinishNow()
	{
		UpdateTweener(timer);
	}

	public void Toggle()
	{
		Go(Position.Other, false);
	}

	public override bool Transition_Forward()
	{
		return Go(Position.End, false);
	}

	public override bool Transition_Forward(bool reverse)
	{
		return Go(Position.End, reverse);
	}

	public override bool Transition_Back()
	{
		return Go(Position.Start, false);
	}

	public override bool Transition_Back(bool reverse)
	{
		return Go(Position.Start, reverse);
	}

	private void UpdateColorObjectsToTween()
	{
		colorObjectsToTween.Clear();
		if (objectsToTween != null && objectsToTween.Length > 0)
		{
			GameObject[] array = objectsToTween;
			foreach (GameObject gameObject in array)
			{
				if (!(gameObject != null))
				{
					continue;
				}
				if (recursiveColor)
				{
					colorObjectsToTween.AddRange(gameObject.GetComponentsInChildren<GluiWidget>(true));
					continue;
				}
				GluiWidget component = gameObject.GetComponent<GluiWidget>();
				if (component != null)
				{
					colorObjectsToTween.Add(component);
				}
			}
		}
		else if (recursiveColor)
		{
			colorObjectsToTween.AddRange(base.gameObject.GetComponentsInChildren<GluiWidget>(true));
		}
		else
		{
			GluiWidget component2 = base.gameObject.GetComponent<GluiWidget>();
			if (component2 != null)
			{
				colorObjectsToTween.Add(component2);
			}
		}
		foreach (GluiWidget item in colorObjectsToTween)
		{
			int instanceID = item.GetInstanceID();
			if (!startingColors.ContainsKey(instanceID))
			{
				startingColors[instanceID] = new List<Color>(item.Colors);
			}
		}
	}

	private void SetColor(Color color)
	{
		foreach (GluiWidget item in colorObjectsToTween)
		{
			SetColor(item, color);
		}
	}

	private void SetColor(GluiWidget w, Color color)
	{
		if (!(w != null) || !(w.GetComponent<Renderer>() != null) || !(w.GetComponent<Renderer>().sharedMaterial != null))
		{
			return;
		}
		List<Color> list = new List<Color>(w.Colors);
		for (int i = 0; i < list.Count; i++)
		{
			Color value = list[i];
			Color color2 = startingColors[w.GetInstanceID()][i];
			switch (colorChannels)
			{
			case ColorChannelGroup.Alpha:
				value.a = ((!replaceColor) ? (color.a * color2.a) : color.a);
				break;
			case ColorChannelGroup.ColorRGB:
				value.r = ((!replaceColor) ? (color.r * color2.r) : color.r);
				value.g = ((!replaceColor) ? (color.g * color2.g) : color.g);
				value.b = ((!replaceColor) ? (color.b * color2.b) : color.b);
				break;
			case ColorChannelGroup.FullARGB:
				value = ((!replaceColor) ? (color * color2) : color);
				break;
			}
			list[i] = value;
		}
		w.Colors = list;
	}

	public void SetLocation(Vector3 position, Quaternion rotation, Vector3 scale)
	{
		if (objectsToTween == null || objectsToTween.Length == 0)
		{
			SetLocation(base.gameObject, position, rotation, scale);
			return;
		}
		objectsToTween.ForEachWithIndex(delegate(GameObject gameObject, int index)
		{
			SetLocation(gameObject, position, rotation, scale);
		});
	}

	private void SetLocation(GameObject go, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		if (go == null)
		{
			return;
		}
		Transform transform = go.transform;
		switch (objectInheritance)
		{
		case ObjectInheritance.Local_InheritFromParent:
			if (usePosition)
			{
				transform.localPosition = position;
			}
			if (useRotation)
			{
				transform.localRotation = rotation;
			}
			break;
		case ObjectInheritance.Global_IgnoreParent:
			if (usePosition)
			{
				transform.position = position;
			}
			if (useRotation)
			{
				transform.rotation = rotation;
			}
			break;
		}
		if (useScale)
		{
			transform.localScale = scale;
		}
	}
}
