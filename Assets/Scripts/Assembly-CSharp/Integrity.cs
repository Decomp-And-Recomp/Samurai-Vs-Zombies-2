using System;
using System.Collections.Generic;

public static class Integrity
{
	public const string KEY_IS_JAILBROKEN = "KEY_IS_JAILBROKEN";

	public const string KEY_IS_ENCRYPTED = "KEY_IS_ENCRYPTED";

	public static Version Version
	{
		get
		{
			return new Version(1, 0, 1);
		}
	}

	public static bool IsTrusted()
	{
		return IsEncrypted() && !IsJailbroken();
	}

	public static Dictionary<string, string> GetDetails()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("KEY_IS_JAILBROKEN", IsJailbroken().ToString());
		dictionary.Add("KEY_IS_ENCRYPTED", IsEncrypted().ToString());
		return dictionary;
	}

	public static bool IsJailbroken()
	{
		return false;
	}

	public static bool IsEncrypted()
	{
		return false;
	}
}
