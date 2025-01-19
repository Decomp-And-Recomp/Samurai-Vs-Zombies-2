namespace Gamespy.Matchmaking
{
	public class Key
	{
		private string _keyName;

		private string _keyValue;

		public string KeyName
		{
			get
			{
				return _keyName;
			}
			set
			{
				_keyName = value;
			}
		}

		public string KeyValue
		{
			get
			{
				return _keyValue;
			}
			set
			{
				_keyValue = value;
			}
		}

		public Key()
		{
		}

		public Key(string name, string value)
		{
			_keyName = name;
			_keyValue = value;
		}
	}
}
