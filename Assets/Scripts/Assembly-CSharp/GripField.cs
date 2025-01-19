using System;
using Gamespy.Common;

public class GripField : ICloneable
{
	public enum GripFieldType
	{
		Byte = 0,
		Short = 1,
		Int = 2,
		Float = 3,
		AsciiString = 4,
		UnicodeString = 5,
		Boolean = 6,
		DateAndTime = 7,
		BinaryData = 8,
		Int64 = 9,
		Null = 10
	}

	public string mName;

	public GripFieldType mType;

	public bool mDirty;

	public sbyte? mByte;

	public short? mShort;

	public int? mInt;

	public float? mFloat;

	public string mString;

	public bool? mBoolean;

	public DateTime? mDateAndTime;

	public byte[] mBinaryData;

	public long? mInt64;

	public GripField()
	{
	}

	public GripField(string name, GripFieldType type)
	{
		mName = name;
		mType = type;
	}

	public static void SakeFieldToGripField(Field sakeField, GripField gripField)
	{
		gripField.mName = sakeField.Name;
		switch (sakeField.Type)
		{
		case FieldType.asciiStringValue:
			gripField.mString = sakeField.ValueString;
			gripField.mType = GripFieldType.AsciiString;
			break;
		case FieldType.unicodeStringValue:
			gripField.mString = sakeField.ValueString;
			gripField.mType = GripFieldType.UnicodeString;
			break;
		case FieldType.shortValue:
			gripField.mShort = short.Parse(sakeField.ValueString);
			gripField.mType = GripFieldType.Short;
			break;
		case FieldType.intValue:
			gripField.mInt = int.Parse(sakeField.ValueString);
			gripField.mType = GripFieldType.Int;
			break;
		case FieldType.floatValue:
			gripField.mFloat = float.Parse(sakeField.ValueString);
			gripField.mType = GripFieldType.Float;
			break;
		case FieldType.byteValue:
			gripField.mByte = sbyte.Parse(sakeField.ValueString);
			gripField.mType = GripFieldType.Byte;
			break;
		case FieldType.binaryDataValue:
			gripField.mBinaryData = sakeField.ValueArray;
			gripField.mType = GripFieldType.BinaryData;
			break;
		case FieldType.booleanValue:
			gripField.mBoolean = bool.Parse(sakeField.ValueString);
			gripField.mType = GripFieldType.Boolean;
			break;
		case FieldType.dateAndTimeValue:
		{
			DateTime result = DateTime.MinValue;
			gripField.mDateAndTime = result;
			DateTime.TryParse(sakeField.ValueString, out result);
			gripField.mDateAndTime = result;
			gripField.mType = GripFieldType.DateAndTime;
			break;
		}
		case FieldType.int64Value:
			gripField.mInt64 = long.Parse(sakeField.ValueString);
			gripField.mType = GripFieldType.Int64;
			break;
		case FieldType.nullValue:
			break;
		default:
			throw new Exception("Unhandled conversion in SakeFieldToGripField");
		}
	}

	public static void GripFieldToSakeField(GripField gripField, Field sakeField)
	{
		sakeField.Name = gripField.mName;
		switch (gripField.mType)
		{
		case GripFieldType.AsciiString:
			sakeField.ValueString = gripField.mString ?? string.Empty;
			sakeField.Type = FieldType.asciiStringValue;
			break;
		case GripFieldType.UnicodeString:
			sakeField.ValueString = gripField.mString ?? string.Empty;
			sakeField.Type = FieldType.unicodeStringValue;
			break;
		case GripFieldType.Short:
			sakeField.ValueString = gripField.mShort.GetValueOrDefault().ToString();
			sakeField.Type = FieldType.shortValue;
			break;
		case GripFieldType.Int:
			sakeField.ValueString = gripField.mInt.GetValueOrDefault().ToString();
			sakeField.Type = FieldType.intValue;
			break;
		case GripFieldType.Float:
			sakeField.ValueString = gripField.mFloat.GetValueOrDefault().ToString();
			sakeField.Type = FieldType.floatValue;
			break;
		case GripFieldType.Byte:
			sakeField.ValueString = gripField.mByte.GetValueOrDefault().ToString();
			sakeField.Type = FieldType.byteValue;
			break;
		case GripFieldType.BinaryData:
			sakeField.ValueArray = gripField.mBinaryData ?? new byte[0];
			sakeField.Type = FieldType.binaryDataValue;
			break;
		case GripFieldType.Boolean:
			if (gripField.mBoolean.GetValueOrDefault())
			{
				sakeField.ValueString = "1";
			}
			else
			{
				sakeField.ValueString = "0";
			}
			sakeField.Type = FieldType.booleanValue;
			break;
		case GripFieldType.DateAndTime:
			sakeField.ValueString = gripField.mDateAndTime.GetValueOrDefault().ToString("s");
			sakeField.Type = FieldType.dateAndTimeValue;
			break;
		case GripFieldType.Int64:
			sakeField.ValueString = gripField.mInt64.GetValueOrDefault().ToString();
			sakeField.Type = FieldType.int64Value;
			break;
		case GripFieldType.Null:
			sakeField.Type = FieldType.nullValue;
			break;
		}
	}

	public object Clone()
	{
		GripField gripField = (GripField)MemberwiseClone();
		if (mBinaryData != null)
		{
			gripField.mBinaryData = new byte[mBinaryData.Length];
			Array.Copy(mBinaryData, gripField.mBinaryData, mBinaryData.Length);
		}
		return gripField;
	}

	public T GetField<T>()
	{
		object obj = GetField();
		if (obj == null)
		{
			obj = string.Empty;
		}
		if (obj.GetType() != typeof(T))
		{
			return default(T);
		}
		return (T)obj;
	}

	public object GetField()
	{
		switch (mType)
		{
		case GripFieldType.Byte:
			return mByte.Value;
		case GripFieldType.Short:
			return mShort.Value;
		case GripFieldType.Int:
			return mInt.Value;
		case GripFieldType.Float:
			return mFloat.Value;
		case GripFieldType.AsciiString:
			return mString;
		case GripFieldType.UnicodeString:
			return mString;
		case GripFieldType.Boolean:
			return mBoolean.Value;
		case GripFieldType.DateAndTime:
			return mDateAndTime.Value;
		case GripFieldType.BinaryData:
			return mBinaryData;
		case GripFieldType.Int64:
			return mInt64.Value;
		default:
			return null;
		}
	}

	public void SetField(object fieldValue)
	{
		object obj = fieldValue ?? string.Empty;
		switch (mType)
		{
		case GripFieldType.Byte:
			if (obj.GetType() == typeof(sbyte))
			{
				mByte = (sbyte)obj;
			}
			break;
		case GripFieldType.Short:
			if (obj.GetType() == typeof(short))
			{
				mShort = (short)obj;
			}
			break;
		case GripFieldType.Int:
			if (obj.GetType() == typeof(int))
			{
				mInt = (int)obj;
			}
			break;
		case GripFieldType.Float:
			if (obj.GetType() == typeof(float))
			{
				mFloat = (float)obj;
			}
			break;
		case GripFieldType.AsciiString:
			if (obj.GetType() == typeof(string))
			{
				mString = (string)obj;
			}
			break;
		case GripFieldType.UnicodeString:
			if (obj.GetType() == typeof(string))
			{
				mString = (string)obj;
			}
			break;
		case GripFieldType.Boolean:
			if (obj.GetType() == typeof(bool))
			{
				mBoolean = (bool)obj;
			}
			break;
		case GripFieldType.DateAndTime:
			if (obj.GetType() == typeof(DateTime))
			{
				mDateAndTime = (DateTime)obj;
			}
			break;
		case GripFieldType.BinaryData:
			if (obj.GetType() == typeof(byte[]))
			{
				mBinaryData = (byte[])obj;
			}
			break;
		case GripFieldType.Int64:
			if (obj.GetType() == typeof(long))
			{
				mInt64 = (long)obj;
			}
			break;
		}
	}
}
