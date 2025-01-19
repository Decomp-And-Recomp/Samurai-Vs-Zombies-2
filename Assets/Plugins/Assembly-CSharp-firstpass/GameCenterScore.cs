using System;
using System.Collections;
using System.Collections.Generic;

public class GameCenterScore
{
	public string category;

	public string formattedValue;

	public int value;

	public DateTime date;

	public string playerId;

	public int rank;

	public bool isFriend;

	public string alias;

	public GameCenterScore(Hashtable ht)
	{
		category = ht["category"] as string;
		formattedValue = ht["formattedValue"] as string;
		value = int.Parse(ht["value"].ToString());
		playerId = ht["playerId"] as string;
		rank = int.Parse(ht["rank"].ToString());
		isFriend = (bool)ht["isFriend"];
		alias = ht["alias"] as string;
		double num = double.Parse(ht["date"].ToString());
		date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(num);
	}

	public static List<GameCenterScore> fromJSON(string json)
	{
		List<GameCenterScore> list = new List<GameCenterScore>();
		ArrayList arrayList = json.arrayListFromJson();
		foreach (Hashtable item in arrayList)
		{
			list.Add(new GameCenterScore(item));
		}
		return list;
	}

	public override string ToString()
	{
		return string.Format("<Score> category: {0}, formattedValue: {1}, date: {2}, rank: {3}, alias: {4}", category, formattedValue, date, rank, alias);
	}
}
