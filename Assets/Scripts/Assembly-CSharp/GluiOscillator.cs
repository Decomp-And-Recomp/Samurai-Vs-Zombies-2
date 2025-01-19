using System;
using UnityEngine;

[AddComponentMenu("Glui/Oscillator")]
[ExecuteInEditMode]
public class GluiOscillator : GluiBase
{
	public enum Wave
	{
		Sine = 0,
		Square = 1,
		Sawtooth = 2,
		Triangle = 3
	}

	public enum Target
	{
		PositionX = 0,
		PositionY = 1,
		PositionZ = 2,
		RotationX = 3,
		RotationY = 4,
		RotationZ = 5,
		ScaleX = 6,
		ScaleY = 7,
		ScaleZ = 8,
		ColorR = 9,
		ColorG = 10,
		ColorB = 11,
		ColorA = 12,
		Visible = 13
	}

	[Serializable]
	public class Curve
	{
		public Target target;

		public Wave wave;

		public float frequencyBase = 1f;

		public float frequencyRandom;

		public float amplitudeBase = 1f;

		public float amplitudeRandom;

		public float shiftBase;

		public float shiftRandom;

		public float offsetBase;

		public float offsetRandom;

		public float frequency = 1f;

		public float amplitude = 1f;

		public float offset;

		public float time;

		public GameObject gameObject;

		public GluiWidget widget;

		public bool valid;

		public void Start(GameObject gameObject, GluiWidget widget)
		{
			Recalculate();
			this.gameObject = gameObject;
			this.widget = widget;
		}

		public void Recalculate()
		{
			frequency = UnityEngine.Random.Range(0f, frequencyRandom) + frequencyBase;
			amplitude = UnityEngine.Random.Range(0f, amplitudeRandom) + amplitudeBase;
			offset = UnityEngine.Random.Range(0f, offsetRandom) + offsetBase;
			time = UnityEngine.Random.Range(0f, shiftRandom) + shiftBase;
		}

		public void UpdateSine(float elapsedTime)
		{
			float value = Mathf.Sin(1f / frequency * elapsedTime + time) * amplitude + offset;
			SetValue(value);
		}

		public void UpdateSquare(float elapsedTime)
		{
		}

		public void UpdateSawtooth(float elapsedTime)
		{
		}

		public void UpdateTriangle(float elapsedTime)
		{
		}

		private void SetValue(float newValue)
		{
			switch (target)
			{
			case Target.PositionX:
				gameObject.transform.localPosition = new Vector3(newValue, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z);
				break;
			case Target.PositionY:
				gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, newValue, gameObject.transform.localPosition.z);
				break;
			case Target.PositionZ:
				gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, newValue);
				break;
			case Target.RotationX:
				break;
			case Target.RotationY:
				break;
			case Target.RotationZ:
				break;
			case Target.ScaleX:
				break;
			case Target.ScaleY:
				break;
			case Target.ScaleZ:
				break;
			case Target.Visible:
				break;
			case Target.ColorA:
				break;
			case Target.ColorR:
				break;
			case Target.ColorG:
				break;
			case Target.ColorB:
				break;
			}
		}
	}

	public Wave curve;

	public Curve[] curves = new Curve[1];

	private float startTime;

	protected override void OnCreate()
	{
		GluiWidget component = base.gameObject.GetComponent<GluiWidget>();
		Curve[] array = curves;
		foreach (Curve curve in array)
		{
			curve.Start(base.gameObject, component);
		}
		startTime = Time.time;
	}

	public virtual void Update()
	{
		if (!base.Enabled)
		{
			return;
		}
		float elapsedTime = Time.time - startTime;
		Curve[] array = curves;
		foreach (Curve curve in array)
		{
			switch (curve.wave)
			{
			case Wave.Sine:
				curve.UpdateSine(elapsedTime);
				break;
			case Wave.Square:
				curve.UpdateSquare(elapsedTime);
				break;
			case Wave.Sawtooth:
				curve.UpdateSawtooth(elapsedTime);
				break;
			case Wave.Triangle:
				curve.UpdateTriangle(elapsedTime);
				break;
			}
		}
	}
}
