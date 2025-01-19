using System.Text;

[DataBundleClass(Category = "Design", Comment = "Misc data ported from SvZ1 text files")]
public class TextDBSchema
{
	[DataBundleKey]
	public string key;

	public string value;

	private static StringBuilder sb = new StringBuilder();

	public static char kChildSeperator
	{
		get
		{
			return '~';
		}
	}

	public static string LevelKey(string key, int level)
	{
		return string.Format("{0:000}{1}{2}", level, kChildSeperator, key);
	}

	public static string ChildKey(params string[] nodes)
	{
		sb.Length = 0;
		foreach (string arg in nodes)
		{
			if (sb.Length > 0)
			{
				sb.AppendFormat("{0}{1}", kChildSeperator, arg);
			}
			else
			{
				sb.Append(arg);
			}
		}
		return sb.ToString();
	}
}
