using System;
using System.Collections.Generic;
using UnityEngine;

public class GrConsole : Singleton<GrConsole>
{
	private class Message
	{
		public eType mType;

		public string mText;
	}

	public class Hotkey
	{
		public string displayableName = string.Empty;

		public string command = string.Empty;

		public Hotkey[] subRow;

		public float? ButtonWidth { get; set; }

		public bool IsCategory
		{
			get
			{
				return subRow != null;
			}
		}

		public void Setup(string displayableName, string command, bool hasSubRow)
		{
			Setup(displayableName, command, hasSubRow, 12);
		}

		public void Setup(string displayableName, string command, bool hasSubRow, int numButtons)
		{
			this.displayableName = displayableName;
			this.command = command;
			if (hasSubRow)
			{
				subRow = new Hotkey[numButtons];
				for (int i = 0; i < numButtons; i++)
				{
					subRow[i] = new Hotkey();
				}
			}
		}

		public Hotkey SetChildHotkey(int index, string displayableName, string command, bool hasSubRow)
		{
			if (subRow != null && index >= 0 && index < subRow.Length)
			{
				subRow[index].Setup(displayableName, command, hasSubRow);
				return subRow[index];
			}
			return null;
		}
	}

	public enum eType
	{
		Message = 0,
		Warning = 1,
		Error = 2,
		Execute = 3,
		Network = 4,
		Highlight = 5,
		SlightWarning = 6,
		Count = 7
	}

	private const float kButtonWidth = 35f;

	private const float kButtonHeight = 55f;

	private const float kSingleControlHeight = 20f;

	private const float kSingleLineHeight = 12f;

	private const int kMaxTextDisplayList = 1000;

	private const int kHotkeyButtons = 12;

	private const float kHotkeyMaxLevel = 4f;

	private static Color[] sMessageColors = new Color[7]
	{
		Color.white,
		new Color(1f, 1f, 0.2f, 1f),
		new Color(1f, 0.6f, 0.6f, 1f),
		new Color(0.6f, 0.6f, 1f, 1f),
		new Color(0.5f, 1f, 0.5f, 1f),
		new Color(0.4f, 1f, 0.7f, 1f),
		new Color(0.8f, 0.8f, 0.5f, 1f)
	};

	private string mInputText = string.Empty;

	private bool mShowConsole = true;

	private bool mSelectTextEditField;

	private int mIndex;

	private int mLineCount;

	private Vector2 mPositon = new Vector2(0f, 0f);

	private Vector2 mTotalPanelSize = new Vector2(0f, 0f);

	private float mScale = 1f;

	private List<string> mPreviousCommands = new List<string>();

	private int mPreviousCommandIndex = -1;

	private int mPreviousCommandMax = 32;

	private static float kTextFieldHeight = 35f;

	private List<Message> mTextList;

	private List<Message> mTextBuffList;

	private Dictionary<string, Func<string[], string>> mConsoleFunctions;

	private Matrix4x4 mMatWorld;

	private float mHotkeyWidth;

	private Hotkey mHotkeys;

	private int[] mHotkeySelected = new int[4];

	private float mHotkeyBarCurrentHeight;

	private GUIStyle mHotkeyFocusedStyle;

	public float Scale
	{
		set
		{
			mScale = value;
		}
	}

	public bool Visible
	{
		get
		{
			return mShowConsole;
		}
		set
		{
			mShowConsole = value;
			if (mShowConsole)
			{
				mSelectTextEditField = true;
			}
		}
	}

	public Vector2 Size
	{
		get
		{
			return mTotalPanelSize;
		}
		set
		{
			setSize(value);
		}
	}

	public GrConsole()
	{
		ClearConsoleFunctions();
		mTextList = new List<Message>();
		mTextBuffList = new List<Message>();
		mHotkeys = new Hotkey();
		mHotkeys.Setup("root", string.Empty, true);
		for (int i = 0; (float)i < 4f; i++)
		{
			mHotkeySelected[i] = -1;
		}
	}

	public void setPosition(Vector2 position)
	{
		mPositon = position;
	}

	public void setSize(Vector2 size)
	{
		mTotalPanelSize = size;
		mHotkeyWidth = mTotalPanelSize.x / (float)mHotkeys.subRow.Length;
	}

	private void setInputText(string text)
	{
		char[] trimChars = new char[1] { '\n' };
		mInputText = text.Trim(trimChars);
	}

	~GrConsole()
	{
	}

	public void ClearConsoleFunctions()
	{
		mConsoleFunctions = new Dictionary<string, Func<string[], string>>();
	}

	private void RenderMessageText(float topTextAreaPosY)
	{
		Color color = GUI.color;
		for (int i = mIndex; i < mIndex + mLineCount && i < mTextList.Count; i++)
		{
			GUI.color = sMessageColors[(int)mTextList[i].mType];
			GUI.Label(new Rect(4f, topTextAreaPosY + (float)(i - mIndex) * 12f, mTotalPanelSize.x, 24f), mTextList[i].mText);
		}
		GUI.color = color;
	}

	private void RenderBackground()
	{
		GUI.Box(new Rect(0f, 0f, 0f + mTotalPanelSize.x, mTotalPanelSize.y), string.Empty);
		GUI.Box(new Rect(0f, 0f, 0f + mTotalPanelSize.x, mTotalPanelSize.y), string.Empty);
		GUI.Box(new Rect(0f, 0f, 0f + mTotalPanelSize.x, mTotalPanelSize.y), string.Empty);
	}

	private void RenderPreUpdate(KeyCode newKeyPress)
	{
		if (mIndex < 0)
		{
			mIndex = 0;
		}
		if (newKeyPress == KeyCode.Escape)
		{
			mShowConsole = false;
		}
		CreateHotkeyFocusedStyle();
	}

	private void RenderHotkeyBar(int level, Hotkey[] hotkeyRow, float? buttonWidth)
	{
		if ((float)level >= 4f)
		{
			return;
		}
		if (!buttonWidth.HasValue)
		{
			buttonWidth = mHotkeyWidth;
		}
		Rect position = new Rect(0f, 0f + (float)level * 55f, buttonWidth.Value, 55f);
		float num = (float)(level + 1) * 55f;
		if (mHotkeyBarCurrentHeight < num)
		{
			mHotkeyBarCurrentHeight = num;
		}
		for (int i = 0; i < hotkeyRow.Length; i++)
		{
			GUIStyle button;
			if (hotkeyRow[i].IsCategory && mHotkeySelected[level] == i)
			{
				RenderHotkeyBar(level + 1, hotkeyRow[i].subRow, hotkeyRow[i].ButtonWidth);
				button = mHotkeyFocusedStyle;
			}
			else
			{
				button = GUI.skin.button;
			}
			if (GUI.Button(position, hotkeyRow[i].displayableName, button))
			{
				if (hotkeyRow[i].IsCategory)
				{
					if (mHotkeySelected[level] == i)
					{
						mHotkeySelected[level] = -1;
					}
					else
					{
						mHotkeySelected[level] = i;
					}
				}
				else if (hotkeyRow[i].command != string.Empty)
				{
					DoCommand(hotkeyRow[i].command);
				}
			}
			position.xMin += buttonWidth.Value;
			position.xMax += buttonWidth.Value;
		}
	}

	private void DoCommand(string command)
	{
		if (runCommand(command))
		{
			addPreviousCommand(command);
			mInputText = string.Empty;
		}
	}

	public void renderConsole()
	{
		Event current = Event.current;
		KeyCode keyCode = KeyCode.None;
		if (current != null && current.type == EventType.KeyDown)
		{
			keyCode = current.keyCode;
		}
		if (!mShowConsole)
		{
			return;
		}
		RenderPreUpdate(keyCode);
		int fontSize = GUI.skin.textField.fontSize;
		GUI.skin.textField.fontSize = 10;
		GrRenderer instance = Singleton<GrRenderer>.Instance;
		instance.start2d();
		Matrix4x4 matrix = GUI.matrix;
		mMatWorld = Matrix4x4.Scale(new Vector3(mScale, mScale, 1f));
		mMatWorld.SetColumn(3, new Vector4(mPositon.x, mPositon.y, 0f, 1f));
		GUI.matrix = matrix * mMatWorld;
		RenderBackground();
		mHotkeyBarCurrentHeight = 0f;
		RenderHotkeyBar(0, mHotkeys.subRow, mHotkeys.ButtonWidth);
		float num = 0f + mHotkeyBarCurrentHeight;
		float num2 = mTotalPanelSize.y - kTextFieldHeight;
		processBufferedMessages();
		float num3 = num2 - num;
		mLineCount = (int)(num3 / 12f);
		RenderMessageText(num);
		float num4 = (num2 - num) / 2f;
		if (keyCode == KeyCode.PageUp || GUI.Button(new Rect(mTotalPanelSize.x - 35f, num, 35f, num4), "^"))
		{
			mIndex -= mLineCount;
		}
		if (keyCode == KeyCode.PageDown || GUI.Button(new Rect(mTotalPanelSize.x - 35f, num2 - num4, 35f, num4), "v"))
		{
			mIndex += mLineCount;
			if (mIndex > mTextList.Count - mLineCount - 1)
			{
				mIndex = mTextList.Count - mLineCount;
			}
		}
		if (keyCode == KeyCode.UpArrow || GUI.Button(new Rect(0f, num2, 35f, 55f), "<<"))
		{
			changePreviousCommand(1);
		}
		if (keyCode == KeyCode.DownArrow)
		{
			changePreviousCommand(-1);
		}
		if ((keyCode == KeyCode.Return || GUI.Button(new Rect(mTotalPanelSize.x - 35f, num2, 35f, 55f), "GO")) && runCommand(mInputText))
		{
			addPreviousCommand(mInputText);
			mInputText = string.Empty;
		}
		GUI.SetNextControlName("CommandEntryField");
		GUI.skin.textField.fontSize = 22;
		string inputText = GUI.TextField(new Rect(35f, num2, mTotalPanelSize.x - 70f, kTextFieldHeight), mInputText, 100);
		setInputText(inputText);
		if (mSelectTextEditField)
		{
			GUI.FocusControl("CommandEntryField");
			mSelectTextEditField = false;
		}
		GUI.matrix = matrix;
		instance.end2d();
		GUI.skin.textField.fontSize = fontSize;
	}

	private void addPreviousCommand(string command)
	{
		mPreviousCommands.Insert(0, command);
		mPreviousCommandIndex = -1;
		if (mPreviousCommands.Count > mPreviousCommandMax)
		{
			mPreviousCommands.RemoveAt(mPreviousCommandMax);
		}
	}

	private void changePreviousCommand(int change)
	{
		mPreviousCommandIndex += change;
		if (mPreviousCommandIndex >= mPreviousCommands.Count)
		{
			mPreviousCommandIndex = mPreviousCommands.Count - 1;
		}
		if (mPreviousCommandIndex <= -1)
		{
			mPreviousCommandIndex = -1;
			mInputText = string.Empty;
		}
		else
		{
			mInputText = mPreviousCommands[mPreviousCommandIndex];
		}
	}

	private void logCallback(string logString, string stackTrace, LogType type)
	{
		eType type2;
		switch (type)
		{
		case LogType.Error:
			type2 = eType.Error;
			break;
		case LogType.Warning:
			type2 = eType.Warning;
			break;
		case LogType.Assert:
			type2 = eType.Error;
			break;
		case LogType.Exception:
			type2 = eType.Error;
			break;
		default:
			type2 = eType.Message;
			break;
		}
		addMessage(type2, logString);
	}

	public void addMessage(eType type, string str)
	{
		string[] array = str.Split('\n');
		string[] array2 = array;
		foreach (string text in array2)
		{
			char[] trimChars = new char[3] { ' ', '\t', '\n' };
			string text2 = text.TrimEnd(trimChars);
			if (text2.Length != 0)
			{
				Message message = new Message();
				message.mType = type;
				message.mText = text2;
				mTextBuffList.Add(message);
			}
		}
	}

	private void addTextToDisplayList(Message text)
	{
		mTextList.Add(text);
		while (mTextList.Count > 1000)
		{
			mTextList.RemoveAt(0);
		}
	}

	public void Clear()
	{
		mTextList.Clear();
	}

	private void processBufferedMessages()
	{
		if (mTextBuffList.Count == 0)
		{
			return;
		}
		GUIStyle style = GUI.skin.GetStyle("Label");
		while (mTextBuffList.Count > 0)
		{
			eType mType = mTextBuffList[0].mType;
			string mText = mTextBuffList[0].mText;
			string text = mText;
			while (!(style.CalcSize(new GUIContent(text)).x <= mTotalPanelSize.x))
			{
				text = text.Remove(text.Length - 1);
			}
			Message message = new Message();
			message.mType = mType;
			message.mText = text;
			addTextToDisplayList(message);
			string text2 = mText.Substring(text.Length);
			mTextBuffList.RemoveAt(0);
			if (text2.Length > 0)
			{
				Message message2 = new Message();
				message2.mText = text2;
				message2.mType = mType;
				mTextBuffList.Insert(0, message2);
			}
		}
		mIndex = mTextList.Count - mLineCount;
		if (mIndex < 0)
		{
			mIndex = 0;
		}
	}

	public void add(string keyString, Func<string[], string> fName)
	{
		keyString = keyString.ToLower();
		mConsoleFunctions.Add(keyString, fName);
	}

	public void printAllCommands()
	{
		foreach (KeyValuePair<string, Func<string[], string>> mConsoleFunction in mConsoleFunctions)
		{
			addMessage(eType.Message, mConsoleFunction.Key);
		}
	}

	public bool runCommand(string str)
	{
		if (str == null || str.Length == 0)
		{
			addMessage(eType.Error, "Error: No command to run.\n");
			return false;
		}
		string[] array = str.Split(' ', '\t');
		string text = array[0].ToLower();
		if (!mConsoleFunctions.ContainsKey(text))
		{
			addMessage(eType.Error, string.Format("Error: Command '{0}' not found.\n", text));
			return false;
		}
		addMessage(eType.Execute, string.Format("Run: [{0}]\n", str));
		string text2 = mConsoleFunctions[text](array);
		if (text2.Length > 0)
		{
			addMessage(eType.Error, string.Format("Error: Command '{0}' - {1}.\n", text, text2));
			return false;
		}
		return true;
	}

	private void CreateHotkeyFocusedStyle()
	{
		if (mHotkeyFocusedStyle == null)
		{
			mHotkeyFocusedStyle = new GUIStyle(GUI.skin.button);
			mHotkeyFocusedStyle.normal.textColor = Color.green;
			mHotkeyFocusedStyle.focused.textColor = Color.green;
			mHotkeyFocusedStyle.hover.textColor = Color.green;
			mHotkeyFocusedStyle.fontStyle = FontStyle.Bold;
		}
	}

	public Hotkey SetHotkey(int index, string displayableName, string command, bool hasSubRow)
	{
		return mHotkeys.SetChildHotkey(index, displayableName, command, hasSubRow);
	}
}
