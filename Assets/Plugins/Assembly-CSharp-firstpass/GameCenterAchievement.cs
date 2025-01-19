using System;
using System.Collections;
using System.Collections.Generic;

public class GameCenterAchievement
{
	public string identifier;

	public bool isHidden;

	public bool completed;

	public DateTime lastReportedDate;

	public float percentComplete;

	public GameCenterAchievement(Hashtable ht)
	{
		identifier = ht["identifier"] as string;
		isHidden = (bool)ht["hidden"];
		completed = (bool)ht["completed"];
		percentComplete = float.Parse(ht["percentComplete"].ToString());
		double value = double.Parse(ht["lastReportedDate"].ToString());
		lastReportedDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(value);
	}

	public static List<GameCenterAchievement> fromJSON(string json)
	{
		List<GameCenterAchievement> list = new List<GameCenterAchievement>();
		ArrayList arrayList = json.arrayListFromJson();
		foreach (Hashtable item in arrayList)
		{
			list.Add(new GameCenterAchievement(item));
		}
		return list;
	}

	public override string ToString()
	{
		return string.Format("<Achievement> identifier: {0}, hidden: {1}, completed: {2}, percentComplete: {3}, lastReported: {4}", identifier, isHidden, completed, percentComplete, lastReportedDate);
	}
}
