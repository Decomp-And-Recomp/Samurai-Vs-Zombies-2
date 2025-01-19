using System.Collections.Generic;
using UnityEngine;

public class DebugStats
{
	public enum DebugStatsDisplay
	{
		Basic = 0,
		Memory_Assets = 1
	}

	private const float lineHeight = 15f;

	private const float border = 4f;

	private float deviceScale;

	public bool visible;

	private float width;

	public DebugStatsDisplay display;

	public DebugStats()
	{
		visible = false;
	}

	public void Start()
	{
		deviceScale = DebugScale.Scale();
	}

	private void DrawIntervalBar(ref Rect rect, float percentageValue, int intervals)
	{
		GrRenderer instance = Singleton<GrRenderer>.Instance;
		float x = width / (float)intervals;
		Rect rect2 = rect;
		rect2.y += 4f;
		rect2.width = width * percentageValue;
		rect2.height = 11f;
		instance.rect2d(rect2, Color.white);
		Vector2 start = new Vector2(rect2.x, rect2.y);
		Vector2 end = new Vector2(rect2.x, rect2.y + rect2.height);
		Vector2 vector = new Vector2(x, 0f);
		List<GrRenderer.Line2d> list = new List<GrRenderer.Line2d>();
		for (int i = 0; i < 6; i++)
		{
			list.Add(new GrRenderer.Line2d(start, end));
			start += vector;
			end += vector;
		}
		instance.lines2d(list, Color.green);
		rect.y += 15f;
	}

	private void DrawMousePosition(ref Rect rect)
	{
		Vector2 cursorPosition = Singleton<GrGui>.Instance.getCursorPosition();
		GUI.Label(rect, string.Format("mouse:[{0},{1}]", (int)cursorPosition.x, (int)cursorPosition.y));
		rect.y += 15f;
	}

	private void DrawCameraPosition(ref Rect rect)
	{
		GameObject gameObject = Camera.main.gameObject;
		Vector3 vector = Vector3.zero;
		if (gameObject != null)
		{
			vector = gameObject.transform.position;
		}
		float x = vector.x;
		float y = vector.y;
		float z = vector.z;
		GUI.Label(rect, string.Format("cam:[{0:0.0},{1:0.0},{2:0.0}]", x, y, z));
		rect.y += 15f;
	}

	private void DrawResolution(ref Rect rect)
	{
		GUI.Label(rect, string.Format("res:{0}x{1}", Screen.width, Screen.height));
		rect.y += 15f;
	}

	private void DrawFPS(ref Rect rect)
	{
		float num = Time.timeScale / Time.deltaTime;
		GUI.Label(rect, string.Format("fps:{0:000.0}", num));
		rect.y += 15f;
	}

	private void DrawTimeIntervalChart(ref Rect rect)
	{
		GrRenderer instance = Singleton<GrRenderer>.Instance;
		float num = 25f;
		float num2 = 1f / 60f;
		float num3 = num * Time.deltaTime / num2;
		Rect rect2 = rect;
		rect2.y += 4f;
		rect2.width = num3;
		rect2.height = 11f;
		instance.rect2d(rect2, Color.white);
		Vector2 start = new Vector2(rect2.x, rect2.y);
		Vector2 end = new Vector2(rect2.x, rect2.y + rect2.height);
		Vector2 vector = new Vector2(num, 0f);
		List<GrRenderer.Line2d> list = new List<GrRenderer.Line2d>();
		for (int i = 0; i < 6; i++)
		{
			list.Add(new GrRenderer.Line2d(start, end));
			start += vector;
			end += vector;
		}
		instance.lines2d(list, Color.green);
		rect.y += 15f;
	}

	private void DrawHeapSpace(ref Rect rect)
	{
		GUI.Label(rect, string.Format("heap:{0}K max{1}K", MemoryScanner.mono_gc_get_used_size() / 1024, MemoryScanner.mono_gc_get_heap_size() / 1024));
		rect.y += 15f;
	}

	private string GetResourceText<T>(string name) where T : Object
	{
		T[] allResources;
		long totalBytes;
		MemoryScanner.GetAllGenericResources<T>(out allResources, out totalBytes);
		return string.Format("{0}:{1} {2}K", name, allResources.Length.ToString(), (totalBytes / 1024).ToString());
	}

	private void DrawAssetCounts_Tex_Audio(ref Rect rect)
	{
		GUI.Label(rect, string.Format("{0} {1}", GetResourceText<Texture2D>("tex"), GetResourceText<AudioClip>("aud")));
		rect.y += 15f;
	}

	private void DrawAssetCounts_Anm_Mesh(ref Rect rect)
	{
		GUI.Label(rect, string.Format("{0} {1}", GetResourceText<AnimationClip>("anm"), GetResourceText<Mesh>("mesh")));
		rect.y += 15f;
	}

	private void DrawAccelerometerDisplay()
	{
	}

	public void OnGUI()
	{
		if (visible)
		{
			Matrix4x4 matrix = GUI.matrix;
			Matrix4x4 matrix4x = Matrix4x4.Scale(new Vector3(deviceScale, deviceScale, 1f));
			GUI.matrix = matrix * matrix4x;
			GrRenderer instance = Singleton<GrRenderer>.Instance;
			GrGui instance2 = Singleton<GrGui>.Instance;
			int num = 5;
			instance.start2d();
			float num2 = instance2.getVirtualWidth() * (1f - 0.25f * deviceScale) / deviceScale;
			float virtualWidth = instance2.getVirtualWidth();
			float top = ((!Singleton<GrConsole>.Instance.Visible) ? 0f : (instance2.getVirtualHeight() / 2f));
			float height = 0f + 15f * (float)num + 8f;
			width = virtualWidth - num2;
			Rect rect = new Rect(num2, top, virtualWidth, height);
			instance.rect2d(rect, new Color(0f, 0f, 0f, 1f));
			rect.x += 4f;
			rect.y += 4f;
			switch (display)
			{
			case DebugStatsDisplay.Basic:
				DrawTimeIntervalChart(ref rect);
				rect.y += 15f;
				DrawIntervalBar(ref rect, (float)MemoryScanner.mono_gc_get_used_size() / (float)MemoryScanner.mono_gc_get_heap_size(), 4);
				rect.y -= 15f;
				rect.y -= 15f;
				break;
			case DebugStatsDisplay.Memory_Assets:
				DrawTimeIntervalChart(ref rect);
				DrawIntervalBar(ref rect, (float)MemoryScanner.mono_gc_get_used_size() / (float)MemoryScanner.mono_gc_get_heap_size(), 4);
				break;
			}
			instance.end2d();
			switch (display)
			{
			case DebugStatsDisplay.Basic:
				DrawFPS(ref rect);
				rect.y += 15f;
				DrawHeapSpace(ref rect);
				DrawMousePosition(ref rect);
				break;
			case DebugStatsDisplay.Memory_Assets:
				DrawHeapSpace(ref rect);
				DrawAssetCounts_Tex_Audio(ref rect);
				DrawAssetCounts_Anm_Mesh(ref rect);
				break;
			}
			GUI.matrix = matrix;
		}
	}
}
