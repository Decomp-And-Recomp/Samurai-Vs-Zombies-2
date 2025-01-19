using System.Collections;
using System.Collections.Generic;

public class GameCenterLeaderboard
{
	public string leaderboardId;

	public string title;

	public GameCenterLeaderboard(string leaderboardId, string title)
	{
		this.leaderboardId = leaderboardId;
		this.title = title;
	}

	public static List<GameCenterLeaderboard> fromJSON(string json)
	{
		List<GameCenterLeaderboard> list = new List<GameCenterLeaderboard>();
		Hashtable hashtable = json.hashtableFromJson();
		foreach (DictionaryEntry item in hashtable)
		{
			list.Add(new GameCenterLeaderboard(item.Value as string, item.Key as string));
		}
		return list;
	}

	public override string ToString()
	{
		return string.Format("<Leaderboard> leaderboardId: {0}, title: {1}", leaderboardId, title);
	}
}
