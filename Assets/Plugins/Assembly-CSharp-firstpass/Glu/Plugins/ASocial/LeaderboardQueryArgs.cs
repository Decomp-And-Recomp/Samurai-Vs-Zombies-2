using System;

namespace Glu.Plugins.ASocial
{
	public class LeaderboardQueryArgs : EventArgs
	{
		public enum QueryStatus
		{
			Invalid = 0,
			Success = 1,
			Failed = 2
		}

		private QueryStatus status { get; set; }

		private string message { get; set; }

		private long score { get; set; }

		private int rank { get; set; }

		public LeaderboardQueryArgs(QueryStatus _status, string _message = "")
		{
			status = _status;
			message = _message;
		}

		public QueryStatus GetStatus()
		{
			return status;
		}

		public string GetMessage()
		{
			return message;
		}

		internal void SetScore(long _score)
		{
			score = _score;
		}

		public long GetLeaderboardScore()
		{
			return score;
		}

		internal void SetRank(int _rank)
		{
			rank = _rank;
		}

		public int GetLeaderboardRank()
		{
			return rank;
		}
	}
}
