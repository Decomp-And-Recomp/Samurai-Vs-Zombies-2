using System.Collections.Generic;
using UnityEngine;

public class GrRenderer : Singleton<GrRenderer>
{
	public struct Line2d
	{
		public Vector2 start;

		public Vector2 end;

		public Line2d(Vector2 start, Vector2 end)
		{
			this.start = start;
			this.end = end;
		}
	}

	private Material mMaterial2d;

	public GrRenderer()
	{
		createMaterial2d();
	}

	public void line2d(float x0, float y0, float x1, float y1, Color color)
	{
		line2d(new Vector2(x0, y0), new Vector2(x1, y1), color);
	}

	public void line2d(Vector2 p0, Vector2 p1, Color color)
	{
		GL.Begin(1);
		GL.Color(color);
		GL.Vertex(p0);
		GL.Vertex(p1);
		GL.End();
	}

	public void lines2d(List<Line2d> lines, Color color)
	{
		GL.Begin(1);
		GL.Color(color);
		foreach (Line2d line in lines)
		{
			GL.Vertex(line.start);
			GL.Vertex(line.end);
		}
		GL.End();
	}

	public void lineRect2d(Vector2 start, Vector2 end, Color color)
	{
		Vector2 p = new Vector2(start.x, end.y);
		Vector2 p2 = new Vector2(end.x, start.y);
		line2d(start, p2, color);
		line2d(start, p, color);
		line2d(end, p2, color);
		line2d(end, p, color);
	}

	public void lineRect2d(Rect rect, Color color)
	{
		Vector2 start = new Vector2(rect.x, rect.y);
		Vector2 end = new Vector2(rect.x + rect.width, rect.y + rect.height);
		lineRect2d(start, end, color);
	}

	public void quad2d(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
	{
		GL.Begin(7);
		GL.Color(color);
		GL.Vertex(p0);
		GL.Vertex(p1);
		GL.Vertex(p2);
		GL.Vertex(p3);
		GL.End();
	}

	public void rect2d(Vector2 start, Vector2 end, Color color)
	{
		quad2d(p3: new Vector2(start.x, end.y), p1: new Vector2(end.x, start.y), p0: start, p2: end, color: color);
	}

	public void rect2d(Rect rect, Color color)
	{
		Vector2 start = new Vector2(rect.x, rect.y);
		Vector2 end = new Vector2(rect.x + rect.width, rect.y + rect.height);
		rect2d(start, end, color);
	}

	public void start2d()
	{
		mMaterial2d.SetPass(0);
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, Screen.width, Screen.height, 0f);
		GL.MultMatrix(GUI.matrix);
	}

	public void end2d()
	{
		GL.PopMatrix();
	}

	private void createMaterial2d()
	{
		mMaterial2d = new Material("Shader \"Lines/Colored Blended\" {SubShader { Pass {     Tags { \"Queue\" = \"Geometry+1\" \"RenderType\"=\"Transparent\"}    Blend SrcAlpha OneMinusSrcAlpha     ZWrite Off Cull Off Fog { Mode Off }     BindChannels {    Bind \"vertex\", vertex Bind \"color\", color }} } }");
		mMaterial2d.color = new Color(1f, 1f, 1f, 1f);
		mMaterial2d.hideFlags = HideFlags.HideAndDontSave;
		mMaterial2d.shader.hideFlags = HideFlags.HideAndDontSave;
	}
}
