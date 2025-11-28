using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class DataBundle
{
    public class HashCode
	{
		public ushort field, key, table, type;

		public HashCode(long hashCode)
		{
			field = (ushort)(hashCode & 0xFFFF);
			key = (ushort)((hashCode >> 16) & 0xFFFF);
			table = (ushort)((hashCode >> 32) & 0xFFFF);
			type = (ushort)((hashCode >> 48) & 0xFFFF);
		}

        public HashCode(ushort type, ushort table, ushort key, ushort field)
        {
            this.type = type;
            this.key = key;
            this.table = table;
            this.field = field;
        }

		public string ToString(List<string> stringList)
		{
            if (type == 0)
            {
                return "";
            }

			string final = stringList[type];

			if (table != 0)
			{
				final += "." + stringList[table];
			}

			if (key != 0)
			{
				final += "." + stringList[key];
			}

			if (field != 0)
			{
				final += "." + stringList[field];
			}

			return final;
		}

        public override string ToString()
        {
			return type + "." + table + "." + key + "." + field;
        }

        public long ToHash()
        {
            return field | ((long)key << 16) | ((long)table << 32) | ((long)type << 48);
        }
	}

    public class BundleClass
    {
        public ushort name;

        public Dictionary<ushort, Dictionary<ushort, List<BundleField>>> tables = new Dictionary<ushort, Dictionary<ushort, List<BundleField>>>();

        public BundleClass(ushort name)
        {
            this.name = name;
        }
    }

    public class BundleField
    {
        public FieldInfo info;

        public object value;

        public BundleField(FieldInfo info, object value)
        {
            this.info = info;
            this.value = value;
        }
    }
    
    public static readonly Assembly schemaAssembly = typeof(AbilitiesListSchema).Assembly;
}
