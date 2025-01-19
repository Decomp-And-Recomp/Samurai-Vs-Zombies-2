using UnityEngine;

public class GluiLog_Base : MonoBehaviour
{
	private const int initialLogSize = 50;

	public string[] entries = new string[50];

	public string[] Entries
	{
		get
		{
			return entries;
		}
	}

	protected void AddToLog(string message)
	{
	}
}
