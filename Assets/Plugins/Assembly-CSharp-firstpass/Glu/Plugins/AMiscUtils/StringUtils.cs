namespace Glu.Plugins.AMiscUtils
{
	public static class StringUtils
	{
		public static string Fmt(this string format, params object[] args)
		{
			return string.Format(format, args);
		}

		public static bool IsEmpty(this string s)
		{
			return string.IsNullOrEmpty(s);
		}
	}
}
