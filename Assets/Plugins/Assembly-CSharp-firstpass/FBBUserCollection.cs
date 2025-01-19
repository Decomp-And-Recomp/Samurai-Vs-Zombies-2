using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class FBBUserCollection : Dictionary<string, FBBUser>
{
	private static readonly Regex UserRegex = new Regex("{.*?}");

	public static FBBUserCollection Deserialize(string data)
	{
		FBBUserCollection fBBUserCollection = new FBBUserCollection();
		MatchCollection matchCollection = UserRegex.Matches(data);
		foreach (Match item in matchCollection)
		{
			string value = item.Groups[0].Value;
			FBBUser fBBUser = FBBUser.Deserialize(value);
			Debug.Log("Adding friend to collection: " + fBBUser.Name + " (id: " + fBBUser.ID + ")");
			fBBUserCollection.Add(fBBUser.ID, fBBUser);
		}
		return fBBUserCollection;
	}
}
