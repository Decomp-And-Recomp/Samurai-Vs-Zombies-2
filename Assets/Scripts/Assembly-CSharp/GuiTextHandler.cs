using UnityEngine;

public class GuiTextHandler : MonoBehaviour
{
	public GluiText TextObject;

	public float MaxYPos = 1f;

	public float MinYPos;

	private bool mVisible;

	private bool mOffScreen;

	private GUIText mGuiText;

	private string mOldString;

	private Camera mGluiCamera;

	private void Start()
	{
		mGuiText = base.gameObject.GetComponent<GUIText>();
		if (TextObject == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		TextObject.onTextChanged = UpdateText;
		string textToModify = string.Empty;
		UpdateText(ref textToModify, TextObject.Text);
		mGluiCamera = ObjectUtils.FindFirstCamera(LayerMask.NameToLayer("GLUI"));
		base.transform.parent = null;
	}

	private void Update()
	{
		if (TextObject == null)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			if (!mVisible || !mGluiCamera)
			{
				return;
			}
			Vector3 position = mGluiCamera.WorldToViewportPoint(TextObject.transform.position);
			base.transform.position = position;
			TextObject.Visible = false;
			bool flag = position[1] < MinYPos || position[1] > MaxYPos;
			if (flag != mOffScreen)
			{
				mOffScreen = flag;
				if (flag)
				{
					mGuiText.text = string.Empty;
				}
				else
				{
					mGuiText.text = mOldString;
				}
			}
		}
	}

	private void UpdateText(ref string textToModify, string oldText)
	{
		if (!(oldText != mOldString))
		{
			return;
		}
		mOldString = oldText;
		bool visible = false;
		for (int i = 0; i < mOldString.Length; i++)
		{
			GluiFont.Glyph value;
			if (TextObject != null && TextObject.font != null && TextObject.font.glyphs != null && !TextObject.font.glyphs.TryGetValue(oldText[i], out value))
			{
				visible = true;
				break;
			}
		}
		SetVisible(visible);
	}

	private void SetVisible(bool visible)
	{
		if (visible != mVisible)
		{
			mVisible = visible;
			if (!visible)
			{
				mGuiText.text = string.Empty;
			}
			else
			{
				mGuiText.text = mOldString;
			}
			TextObject.Visible = !visible;
		}
	}
}
