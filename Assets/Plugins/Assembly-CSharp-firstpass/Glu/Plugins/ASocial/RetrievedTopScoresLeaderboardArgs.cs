using System;
using System.Collections.Generic;

namespace Glu.Plugins.ASocial
{
	public class RetrievedTopScoresLeaderboardArgs : EventArgs
	{
		public enum Status
		{
			Success = 0,
			No_Data = 1,
			Failed = 2
		}

		public string mLeaderboardID;

		public List<PlayGameServices.RetrievedTopLeaderboard> mLeaderboards;

		private Status mStatus;

		public RetrievedTopScoresLeaderboardArgs(string leaderboardID, Status status = Status.Success, List<PlayGameServices.RetrievedTopLeaderboard> leaderboards = null)
		{
			mLeaderboardID = leaderboardID;
			mLeaderboards = leaderboards;
			mStatus = status;
		}

		public Status GetStatus()
		{
			return mStatus;
		}
	}
}
