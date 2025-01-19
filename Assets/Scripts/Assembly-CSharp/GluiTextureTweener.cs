using System;
using UnityEngine;

[AddComponentMenu("Glui/Texture Tweener")]
[ExecuteInEditMode]
public class GluiTextureTweener : GluiTransition
{
	public enum ELoopType
	{
		eLoopType_Repeat = 0,
		eLoopType_PingPong = 1
	}

	[Serializable]
	public class Ease
	{
		public float TweenTime = 1f;

		public bool Loop;

		public ELoopType LoopType;
	}

	[Serializable]
	public class Frame
	{
		public Vector3 scale = Vector3.one;

		public Color color = Color.clear;
	}

	public Ease ColorEase;

	public Ease ScaleEase;

	public Frame[] frames = new Frame[2];

	public Frame introFrame = new Frame();

	private GluiWidget widget;

	public bool ScaleTweenEnabled;

	public bool ColorTweenEnabled;

	private float scaletimer;

	private Position scaleGoalPos;

	private Position scaleCurPos;

	private float colortimer;

	private Position colorGoalPos;

	private Position colorCurPos;

	public override bool IsDone
	{
		get
		{
			if (scaletimer == 0f && colortimer == 0f && !ScaleEase.Loop && !ColorEase.Loop)
			{
				return true;
			}
			return false;
		}
	}

	protected override void OnCreate()
	{
		widget = GetComponent<GluiWidget>();
	}

	protected virtual void Update()
	{
		if (base.Enabled)
		{
			OnUpdate(GluiTime.deltaTime);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	public void SetColor()
	{
		Frame frame = frames[(int)colorCurPos];
		Frame frame2 = frames[(int)colorGoalPos];
		if (widget != null)
		{
			Color color = new Color(Mathf.Lerp(frame.color.r, frame2.color.r, colortimer / ColorEase.TweenTime), Mathf.Lerp(frame.color.g, frame2.color.g, colortimer / ColorEase.TweenTime), Mathf.Lerp(frame.color.b, frame2.color.b, colortimer / ColorEase.TweenTime), Mathf.Lerp(frame.color.a, frame2.color.a, colortimer / ColorEase.TweenTime));
			widget.Color = color;
		}
	}

	public void SetScale()
	{
		Frame frame = frames[(int)scaleCurPos];
		Frame frame2 = frames[(int)scaleGoalPos];
		Vector3 localScale = new Vector3(Mathf.Lerp(frame.scale.x, frame2.scale.x, scaletimer / ScaleEase.TweenTime), Mathf.Lerp(frame.scale.y, frame2.scale.y, scaletimer / ScaleEase.TweenTime), Mathf.Lerp(frame.scale.z, frame2.scale.z, scaletimer / ScaleEase.TweenTime));
		base.gameObject.transform.localScale = localScale;
	}

	protected void OnUpdate(float deltaTime)
	{
		if (ScaleTweenEnabled)
		{
			scaletimer += deltaTime;
			if (scaletimer > ScaleEase.TweenTime)
			{
				scaletimer = ScaleEase.TweenTime;
				base.gameObject.transform.localScale = frames[(int)scaleGoalPos].scale;
				if (ScaleEase.Loop)
				{
					if (ScaleEase.LoopType == ELoopType.eLoopType_Repeat)
					{
						GoScale(Position.End);
					}
					else if (ScaleEase.LoopType == ELoopType.eLoopType_PingPong)
					{
						scaleCurPos = DetermineOtherScale();
						GoScale(Position.Other);
					}
					else
					{
						scaleCurPos = scaleGoalPos;
						SetScale();
					}
				}
			}
			else
			{
				SetScale();
			}
		}
		if (!ColorTweenEnabled)
		{
			return;
		}
		colortimer += deltaTime;
		if (widget != null)
		{
			Color color = frames[(int)colorGoalPos].color;
			widget.Color = color;
		}
		if (colortimer > ColorEase.TweenTime)
		{
			if (widget != null)
			{
				widget.Color = frames[(int)colorCurPos].color;
			}
			if (ColorEase.Loop)
			{
				if (ColorEase.LoopType == ELoopType.eLoopType_Repeat)
				{
					GoColor(colorGoalPos);
				}
				else if (ColorEase.LoopType == ELoopType.eLoopType_PingPong)
				{
					colorCurPos = DetermineOtherColor();
					GoColor(Position.Other);
				}
			}
			else
			{
				colorCurPos = colorGoalPos;
				SetColor();
			}
		}
		else
		{
			SetColor();
		}
	}

	public void GoScale(Position to)
	{
		GoScale(to, ScaleEase.TweenTime);
	}

	public void GoScale(Position to, float moveTime)
	{
		if (to == Position.Other)
		{
			if (scaleCurPos == Position.Start)
			{
				GoScale(Position.End);
			}
			else if (scaleCurPos == Position.End)
			{
				GoScale(Position.Start);
			}
		}
		else
		{
			scaleGoalPos = to;
			scaletimer = 0f;
			OnUpdate(0f);
		}
	}

	private Position DetermineOtherScale()
	{
		if (scaleCurPos == Position.Start)
		{
			return Position.End;
		}
		return Position.Start;
	}

	public void ToggleScale()
	{
		GoScale(Position.Other);
	}

	public void FinishNow()
	{
		colortimer = ColorEase.TweenTime + GluiTime.deltaTime;
		colorCurPos = colorGoalPos;
		scaletimer = ScaleEase.TweenTime + GluiTime.deltaTime;
		scaleCurPos = scaleGoalPos;
		OnUpdate(0f);
	}

	public void GoColor(Position to)
	{
		GoColor(to, ColorEase.TweenTime);
	}

	public void GoColor(Position to, float moveTime)
	{
		if (to == Position.Other)
		{
			if (colorCurPos == Position.Start)
			{
				GoColor(Position.End);
			}
			else if (colorCurPos == Position.End)
			{
				GoColor(Position.Start);
			}
		}
		else
		{
			colorGoalPos = to;
			colortimer = 0f;
			OnUpdate(0f);
		}
	}

	private Position DetermineOtherColor()
	{
		if (colorCurPos == Position.Start)
		{
			return Position.End;
		}
		return Position.Start;
	}

	public void ToggleColor()
	{
		GoColor(Position.Other);
	}

	public override bool Transition_Forward(bool reverse)
	{
		if (reverse)
		{
			return Transition_Back();
		}
		return Transition_Forward();
	}

	public override bool Transition_Forward()
	{
		if (scaleCurPos == Position.Start || colorCurPos == Position.Start)
		{
			if (scaleCurPos == Position.Start)
			{
				GoScale(Position.Other, ScaleEase.TweenTime);
			}
			if (colorCurPos == Position.Start)
			{
				GoColor(Position.Other, ColorEase.TweenTime);
			}
			return true;
		}
		return false;
	}

	public override bool Transition_Back(bool reverse)
	{
		if (reverse)
		{
			return Transition_Forward();
		}
		return Transition_Back();
	}

	public override bool Transition_Back()
	{
		if (scaleCurPos == Position.End || colorCurPos == Position.End)
		{
			if (scaleCurPos == Position.End)
			{
				GoScale(Position.Other, ScaleEase.TweenTime);
			}
			if (colorCurPos == Position.End)
			{
				GoColor(Position.Other, ColorEase.TweenTime);
			}
			return true;
		}
		return false;
	}
}
