using System;
using System.IO;

public static class FileUtil
{
	public static string SafeFileOperation(Func<string> fileOperation)
	{
		if (fileOperation == null)
		{
			return null;
		}
		string currentDirectory = string.Copy(Directory.GetCurrentDirectory());
		string result = fileOperation();
		Directory.SetCurrentDirectory(currentDirectory);
		return result;
	}

	public static string GetResourcePath(string path)
	{
		int num = path.IndexOf("Resources/");
		num = ((num != -1) ? (num + "Resources/".Length) : 0);
		int num2 = path.LastIndexOf('.');
		int num3 = ((num2 != -1) ? (path.Length - num2) : 0);
		return path.Substring(num, path.Length - num - num3);
	}
}
