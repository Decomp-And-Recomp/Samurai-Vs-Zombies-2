using System;
using UnityEngine;

[AddComponentMenu("Glui/Flipbook")]
[ExecuteInEditMode]
public class GluiFlipbook : GluiSprite
{
	public enum Repeat
	{
		Forever = 0,
		Once = 1
	}

	public int frame;

	public float animRate = 30f;

	public int frameDelta = 1;

	public int totalFrames = 1;

	public int cols = 1;

	public int textureWidth;

	public int textureHeight;

	public int frameWidth;

	public int frameHeight;

	public bool useImageSheet = true;

	public Texture2D[] textureArray;

	public Repeat repeat;

	public float repeatDelayMin;

	public float repeatDelayMax;

	private int visibleFrame = -1;

	private float animTimer;

	private bool paused;

	private bool repeatWait;

	private float repeatAtTime;

	public Action onAnimDone;

	public bool Paused
	{
		get
		{
			return paused;
		}
		set
		{
			paused = value;
		}
	}

	protected void Update()
	{
		if (base.Visible && base.Enabled && !paused && totalFrames != 0 && (useImageSheet || textureArray != null))
		{
			if (repeatWait)
			{
				UpdateRepeat();
			}
			else
			{
				UpdateAnimation();
			}
			UpdateFrame(false);
		}
	}

	private void UpdateAnimation()
	{
		if (animRate == 0f)
		{
			return;
		}
		animTimer -= Time.deltaTime;
		while (animTimer <= 0f)
		{
			animTimer += 1f / animRate;
			int num = frame + frameDelta;
			if (num >= totalFrames || num < 0)
			{
				RepeatStart();
			}
			if (!repeatWait)
			{
				frame = num;
			}
		}
	}

	private void UpdateRepeat()
	{
		if (Time.time >= repeatAtTime)
		{
			frame += frameDelta;
			repeatWait = false;
		}
	}

	private void RepeatStart()
	{
		if (repeat == Repeat.Forever)
		{
			RepeatStart_Internal();
		}
		else if (repeat == Repeat.Once)
		{
			Paused = true;
		}
	}

	private void RepeatStart_Internal()
	{
		if (repeatDelayMin > 0f)
		{
			if (repeatDelayMax > repeatDelayMin)
			{
				repeatAtTime = Time.time + UnityEngine.Random.Range(repeatDelayMin, repeatDelayMax);
			}
			else
			{
				repeatAtTime = Time.time + repeatDelayMin;
			}
			repeatWait = true;
		}
	}

	public void SetTextureArray(Texture tex)
	{
		textureArray = new Texture2D[1];
		textureArray[0] = (Texture2D)tex;
		InitTextureArray();
		UpdateFrame(true);
	}

	public void SetTextureArray(Texture2D[] texArray)
	{
		textureArray = texArray;
		InitTextureArray();
		UpdateFrame(true);
	}

	private void InitTextureArray()
	{
		useImageSheet = false;
		totalFrames = textureArray.Length;
		visibleFrame = -1;
	}

	private void ClampFrameIndex()
	{
		if (totalFrames == 0)
		{
			frame = 0;
			return;
		}
		if (frame < 0)
		{
			frame = totalFrames - 1;
		}
		if (frame >= totalFrames)
		{
			frame = 0;
		}
	}

	private void UpdateFrame(bool force)
	{
		ClampFrameIndex();
		if (visibleFrame == frame && !force)
		{
			return;
		}
		visibleFrame = frame;
		if (useImageSheet)
		{
			MeshFilter component = GetComponent<MeshFilter>();
			if (component != null)
			{
				float num = base.Size.x / 2f;
				float num2 = base.Size.y / 2f;
				Vector3[] vertices = new Vector3[4]
				{
					new Vector3(0f - num, num2, 0f),
					new Vector3(num, num2, 0f),
					new Vector3(num, 0f - num2, 0f),
					new Vector3(0f - num, 0f - num2, 0f)
				};
				component.sharedMesh.vertices = vertices;
				float num3 = ((!((float)textureWidth <= 0f)) ? ((float)(frame % cols) * (float)frameWidth / (float)textureWidth) : 0f);
				float num4 = ((!((float)textureHeight <= 0f)) ? ((float)(frame / cols) * (float)frameHeight / (float)textureHeight) : 0f);
				float num5 = ((!((float)textureWidth <= 0f)) ? ((float)frameWidth / (float)textureWidth) : 0f);
				float num6 = ((!((float)textureHeight <= 0f)) ? ((float)frameHeight / (float)textureHeight) : 0f);
				Vector2[] uv = new Vector2[4]
				{
					new Vector3(num3, 1f - num4, 0f),
					new Vector3(num3 + num5, 1f - num4, 0f),
					new Vector3(num3 + num5, 1f - (num4 + num6), 0f),
					new Vector3(num3, 1f - (num4 + num6), 0f)
				};
				component.sharedMesh.uv = uv;
			}
		}
		else if (textureArray == null)
		{
			Texture = null;
		}
		else if (textureArray[visibleFrame] == null)
		{
			visibleFrame += frameDelta;
		}
		else
		{
			Texture = textureArray[visibleFrame];
		}
	}

	protected override void OnReset()
	{
		if (animRate != 0f)
		{
			animTimer = 1f / animRate;
		}
		visibleFrame = -1;
		frame = 0;
		UpdateFrame(true);
	}

	public bool Validate()
	{
		if (Texture == null)
		{
			return false;
		}
		if (textureWidth % (int)base.Size.x != 0)
		{
			return false;
		}
		if (textureHeight % (int)base.Size.y != 0)
		{
			return false;
		}
		return true;
	}
}
