using System.Collections.Generic;

namespace Gamespy.Common
{
	public class Record
	{
		private List<Field> _fields = new List<Field>();

		public List<Field> Fields
		{
			get
			{
				return _fields;
			}
		}

		public Record(List<Field> fields)
		{
			_fields = fields;
		}
	}
}
