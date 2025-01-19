[DataBundleClass]
public class DynamicEnum
{
	[DataBundleKey]
	[DataBundleField(ColumnWidth = 300)]
	public string value;

	public static int Count(string enumName)
	{
		if (DataBundleRuntime.Instance != null)
		{
			return DataBundleRuntime.Instance.GetRecordTableLength(typeof(DynamicEnum), enumName);
		}
		return 0;
	}

	public static string FromIndex(string enumName, int index)
	{
		if (DataBundleRuntime.Instance != null)
		{
			return DataBundleRuntime.TableRecordKey(enumName, DataBundleRuntime.Instance.GetRecordKeys(typeof(DynamicEnum), enumName, false)[index]);
		}
		return string.Empty;
	}

	public static int ToIndex(string enumValue)
	{
		if (DataBundleRuntime.Instance != null)
		{
			string[] array = enumValue.Split(DataBundleRuntime.separator);
			if (array.Length == 2)
			{
				return DataBundleRuntime.Instance.GetRecordKeys(typeof(DynamicEnum), array[0], false).IndexOf(array[1]);
			}
		}
		return 0;
	}
}
