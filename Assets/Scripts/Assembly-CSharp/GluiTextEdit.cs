using UnityEngine;

[AddComponentMenu("Glui/TextEdit")]
public class GluiTextEdit : GluiText
{
	private bool showingTextEdit;

	private TouchScreenKeyboard keyboard;

	public string persistTextAs = string.Empty;

	private string defaultText = string.Empty;

	public int characterEntryLimit = 20;

	public bool immediatelyShowKeyboard;

	public bool keyboardShowsShadowedExistingText = true;

	public string actionOnKeyboardExit = string.Empty;

	public string actionOnTextChange = string.Empty;

	protected override void Awake()
	{
		base.Awake();
		base.Usable = true;
		base.Localize = false;
		GetDefaultText();
		base.Text = defaultText;
	}

	public override void Start()
	{
		base.Start();
		if (immediatelyShowKeyboard)
		{
			ShowTextEdit();
		}
	}

	public override void OnDestroy()
	{
		showingTextEdit = false;
		if (keyboard != null)
		{
			keyboard.active = false;
		}
		keyboard = null;
	}

	private void GetDefaultText()
	{
		if (persistTextAs != string.Empty)
		{
			if (Application.isPlaying)
			{
				string text = (string)SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData(persistTextAs);
				if (text != null)
				{
					defaultText = text;
					return;
				}
			}
			PersistText(defaultText);
		}
		defaultText = base.Text;
	}

	public virtual void Update()
	{
		if (showingTextEdit)
		{
			UpdateTextEdit();
		}
	}

	private void ShowTextEdit()
	{
		showingTextEdit = true;
		if (keyboard == null)
		{
			string empty = string.Empty;
			if (!keyboardShowsShadowedExistingText)
			{
				empty = base.Text;
			}
			keyboard = TouchScreenKeyboard.Open(empty, TouchScreenKeyboardType.ASCIICapable, false, false, false, false, defaultText);
		}
	}

	private void UpdateTextEdit()
	{
		if (keyboard == null)
		{
			return;
		}
		string text = keyboard.text;
		ApplyInputLimits(ref text);
		if (keyboard.text != text)
		{
			keyboard.text = text;
		}
		SetText(keyboard.text);
		if (keyboard.done || !keyboard.active)
		{
			if (keyboard.wasCanceled)
			{
				SetText(defaultText);
			}
			HideTextEdit();
			keyboard = null;
		}
	}

	private void HideTextEdit()
	{
		showingTextEdit = false;
		GluiActionSender.SendGluiAction(actionOnKeyboardExit, base.gameObject, null);
	}

	private void PersistText(string newText)
	{
		if (persistTextAs != string.Empty)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(persistTextAs, newText);
		}
	}

	private void ApplyInputLimits(ref string text)
	{
		if (text.Length > characterEntryLimit)
		{
			text = text.Substring(0, characterEntryLimit);
		}
		StripForeignCharacters(ref text);
	}

	private void SetText(string newText)
	{
		if (newText.Trim() == string.Empty)
		{
			newText = defaultText;
		}
		if (base.Text != newText)
		{
			base.Text = newText;
			PersistText(newText);
			GluiActionSender.SendGluiAction(actionOnTextChange, base.gameObject, null);
		}
	}

	private void StripForeignCharacters(ref string text)
	{
		char[] array = text.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ')
			{
				array[i] = '-';
			}
		}
		text = new string(array);
	}

	public override void HandleInput(InputCrawl crawl, out InputRouter.InputResponse response)
	{
		if (!base.AllowInput)
		{
			response = InputRouter.InputResponse.Passthrough;
			return;
		}
		InputEvent.EEventType eventType = crawl.inputEvent.EventType;
		if (eventType == InputEvent.EEventType.OnCursorDown && !showingTextEdit)
		{
			ShowTextEdit();
		}
		response = InputRouter.InputResponse.Handled;
	}
}
