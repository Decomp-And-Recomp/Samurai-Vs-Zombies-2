using System;
using UnityEngine;

public class GluiSendMessageSupport
{
	[Serializable]
	public class Argument
	{
		private enum ArgumentType
		{
			Null = 0,
			Bool = 1,
			Int = 2,
			Float = 3,
			String = 4,
			Vector2 = 5,
			Vector3 = 6,
			Vector4 = 7,
			Color = 8,
			Object = 9
		}

		[SerializeField]
		private ArgumentType argType;

		[SerializeField]
		private string argString;

		[SerializeField]
		private UnityEngine.Object argObject;

		private bool BoolVal
		{
			get
			{
				return !string.IsNullOrEmpty(argString) && bool.Parse(argString);
			}
			set
			{
				SetValue(value);
			}
		}

		private int IntVal
		{
			get
			{
				return (!string.IsNullOrEmpty(argString)) ? int.Parse(argString) : 0;
			}
			set
			{
				SetValue(value);
			}
		}

		private float FloatVal
		{
			get
			{
				return (!string.IsNullOrEmpty(argString)) ? float.Parse(argString) : 0f;
			}
			set
			{
				SetValue(value);
			}
		}

		private string StringVal
		{
			get
			{
				return (!string.IsNullOrEmpty(argString)) ? argString : string.Empty;
			}
			set
			{
				SetValue(value);
			}
		}

		private UnityEngine.Object ObjectVal
		{
			get
			{
				return argObject;
			}
			set
			{
				SetValue(value);
			}
		}

		private Vector2 Vector2Val
		{
			get
			{
				return ParseVector2(argString);
			}
			set
			{
				SetValue(value);
			}
		}

		private Vector3 Vector3Val
		{
			get
			{
				return ParseVector3(argString);
			}
			set
			{
				SetValue(value);
			}
		}

		private Vector4 Vector4Val
		{
			get
			{
				return ParseVector4(argString);
			}
			set
			{
				SetValue(value);
			}
		}

		private Color ColorVal
		{
			get
			{
				return ParseColor(argString);
			}
			set
			{
				SetValue(value);
			}
		}

		public Argument()
		{
		}

		public Argument(bool arg)
		{
			SetValue(arg);
		}

		public Argument(int arg)
		{
			SetValue(arg);
		}

		public Argument(float arg)
		{
			SetValue(arg);
		}

		public Argument(string arg)
		{
			SetValue(arg);
		}

		public Argument(Vector2 arg)
		{
			SetValue(arg);
		}

		public Argument(Vector3 arg)
		{
			SetValue(arg);
		}

		public Argument(Vector4 arg)
		{
			SetValue(arg);
		}

		public Argument(Color arg)
		{
			SetValue(arg);
		}

		public Argument(UnityEngine.Object obj)
		{
			SetValue(obj);
		}

		public void Run(Action<object> runAction)
		{
			if (runAction != null)
			{
				runAction(GetValue());
			}
		}

		public object GetValue()
		{
			if (argType == ArgumentType.Null)
			{
				return null;
			}
			if (argType == ArgumentType.Object)
			{
				return ObjectVal;
			}
			if (argType == ArgumentType.Bool)
			{
				return BoolVal;
			}
			if (argType == ArgumentType.Int)
			{
				return IntVal;
			}
			if (argType == ArgumentType.Float)
			{
				return FloatVal;
			}
			if (argType == ArgumentType.String)
			{
				return StringVal;
			}
			if (argType == ArgumentType.Vector2)
			{
				return Vector2Val;
			}
			if (argType == ArgumentType.Vector3)
			{
				return Vector3Val;
			}
			if (argType == ArgumentType.Vector4)
			{
				return Vector4Val;
			}
			if (argType == ArgumentType.Color)
			{
				return ColorVal;
			}
			return null;
		}

		private void SetValue(object val)
		{
			if (val != null)
			{
				argType = GetAndValidateArgType(val);
				if (val is UnityEngine.Object)
				{
					argObject = val as UnityEngine.Object;
				}
				else
				{
					argString = val.ToString();
				}
			}
			else
			{
				argType = ArgumentType.Null;
				argObject = null;
				argString = null;
			}
		}

		private ArgumentType GetAndValidateArgType(object val)
		{
			if (val == null)
			{
				return ArgumentType.Null;
			}
			if (val is UnityEngine.Object)
			{
				return ArgumentType.Object;
			}
			if (val is bool)
			{
				return ArgumentType.Bool;
			}
			if (val is int)
			{
				return ArgumentType.Int;
			}
			if (val is float)
			{
				return ArgumentType.Float;
			}
			if (val is string)
			{
				return ArgumentType.String;
			}
			if (val is Vector2)
			{
				return ArgumentType.Vector2;
			}
			if (val is Vector3)
			{
				return ArgumentType.Vector3;
			}
			if (val is Vector4)
			{
				return ArgumentType.Vector4;
			}
			if (val is Color)
			{
				return ArgumentType.Color;
			}
			return ArgumentType.Null;
		}

		private Vector2 ParseVector2(string s)
		{
			if (s == null)
			{
				return Vector2.zero;
			}
			string[] array = s.Split('(', ',', ' ', ')', '\t');
			if (array.Length != 5)
			{
				return Vector2.zero;
			}
			return new Vector2(float.Parse(array[1]), float.Parse(array[3]));
		}

		private Vector3 ParseVector3(string s)
		{
			if (s == null)
			{
				return Vector3.zero;
			}
			string[] array = s.Split('(', ',', ' ', ')', '\t');
			if (array.Length != 7)
			{
				return Vector3.zero;
			}
			return new Vector3(float.Parse(array[1]), float.Parse(array[3]), float.Parse(array[5]));
		}

		private Vector4 ParseVector4(string s)
		{
			if (s == null)
			{
				return Vector4.zero;
			}
			string[] array = s.Split('(', ',', ' ', ')', '\t');
			if (array.Length != 9)
			{
				return Vector4.zero;
			}
			return new Vector4(float.Parse(array[1]), float.Parse(array[3]), float.Parse(array[5]), float.Parse(array[7]));
		}

		private Color ParseColor(string s)
		{
			if (s == null)
			{
				return Color.white;
			}
			string[] array = s.Split('(', ',', ' ', ')', '\t');
			if (array.Length != 9)
			{
				return Color.white;
			}
			return new Color(float.Parse(array[1]), float.Parse(array[3]), float.Parse(array[5]), float.Parse(array[7]));
		}

		public static implicit operator Argument(bool arg)
		{
			return new Argument(arg);
		}

		public static implicit operator Argument(int arg)
		{
			return new Argument(arg);
		}

		public static implicit operator Argument(float arg)
		{
			return new Argument(arg);
		}

		public static implicit operator Argument(string arg)
		{
			return new Argument(arg);
		}

		public static implicit operator Argument(Vector2 arg)
		{
			return new Argument(arg);
		}

		public static implicit operator Argument(Vector3 arg)
		{
			return new Argument(arg);
		}

		public static implicit operator Argument(Vector4 arg)
		{
			return new Argument(arg);
		}

		public static implicit operator Argument(Color arg)
		{
			return new Argument(arg);
		}

		public static implicit operator Argument(UnityEngine.Object arg)
		{
			return new Argument(arg);
		}

		public static implicit operator bool(Argument arg)
		{
			return arg.BoolVal;
		}

		public static implicit operator int(Argument arg)
		{
			return arg.IntVal;
		}

		public static implicit operator float(Argument arg)
		{
			return arg.FloatVal;
		}

		public static implicit operator string(Argument arg)
		{
			return arg.StringVal;
		}

		public static implicit operator Vector2(Argument arg)
		{
			return arg.Vector2Val;
		}

		public static implicit operator Vector3(Argument arg)
		{
			return arg.Vector3Val;
		}

		public static implicit operator Vector4(Argument arg)
		{
			return arg.Vector4Val;
		}

		public static implicit operator Color(Argument arg)
		{
			return arg.ColorVal;
		}

		public static implicit operator UnityEngine.Object(Argument arg)
		{
			return arg.ObjectVal;
		}
	}

	private static string StripDelegateName(string delegateName)
	{
		string text = delegateName;
		int num = text.IndexOf('(');
		if (num != -1)
		{
			text = delegateName.Substring(0, num);
		}
		return text;
	}

	public static void CallHandler(GameObject target, string delegateName)
	{
		if (!(target == null) && !string.IsNullOrEmpty(delegateName) && Application.isPlaying)
		{
			target.SendMessage(StripDelegateName(delegateName), SendMessageOptions.DontRequireReceiver);
		}
	}

	public static void CallHandler<T>(GameObject target, string delegateName, T args)
	{
		if (!(target == null) && !string.IsNullOrEmpty(delegateName) && Application.isPlaying)
		{
			target.SendMessage(StripDelegateName(delegateName), args, SendMessageOptions.DontRequireReceiver);
		}
	}

	public static void CallHandler(GameObject target, string delegateName, Argument args)
	{
		if (!(target == null) && !string.IsNullOrEmpty(delegateName) && Application.isPlaying)
		{
			object value = ((args != null) ? args.GetValue() : null);
			target.SendMessage(StripDelegateName(delegateName), value, SendMessageOptions.DontRequireReceiver);
		}
	}
}
