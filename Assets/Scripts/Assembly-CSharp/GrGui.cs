using UnityEngine;

public class GrGui : Singleton<GrGui>
{
	private Matrix4x4 mGuiMatrix = Matrix4x4.identity;

	private Vector2 mCursorPosition;

	~GrGui()
	{
	}

	public void init()
	{
	}

	public float getVirtualWidth()
	{
		return 1024f;
	}

	public float getVirtualHeight()
	{
		return getVirtualWidth() * (float)Screen.height / (float)Screen.width;
	}

	public void update()
	{
		mGuiMatrix = Matrix4x4.Scale(new Vector3((float)Screen.width / getVirtualWidth(), (float)Screen.height / getVirtualHeight(), 1f));
		mGuiMatrix.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
		mCursorPosition = screenPosToGuiPos(Input.mousePosition);
	}

	public void render()
	{
		GUI.matrix = mGuiMatrix;
	}

	public Vector2 getCursorPosition()
	{
		return mCursorPosition;
	}

	public Vector2 getGuiTouchPosition(Vector2 touchPos)
	{
		return screenPosToGuiPos(touchPos);
	}

	public Vector2 screenPosToGuiPos(Vector2 screenPos)
	{
		Vector2 vector = screenPos;
		vector.y = (float)Screen.height - vector.y;
		Vector4 vector2 = new Vector4(vector.x, vector.y, 0f, 1f);
		vector2 = mGuiMatrix.inverse * vector2;
		return new Vector2(vector2.x, vector2.y);
	}
}
