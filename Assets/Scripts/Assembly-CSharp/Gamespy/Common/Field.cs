namespace Gamespy.Common
{
	public class Field
	{
		private string _name;

		private FieldType _type;

		private string _valueString;

		private byte[] _valueArray;

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public FieldType Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		public string ValueString
		{
			get
			{
				return _valueString;
			}
			set
			{
				_valueString = value;
			}
		}

		public byte[] ValueArray
		{
			get
			{
				return _valueArray;
			}
			set
			{
				_valueArray = value;
			}
		}

		public Field()
		{
		}

		public Field(string name, FieldType type, string value)
		{
			_name = name;
			_type = type;
			_valueString = value;
		}

		public Field(string name, FieldType type, byte[] value)
		{
			_name = name;
			_type = type;
			_valueArray = value;
		}
	}
}
