using System;

namespace Glu.Plugins.ASocial
{
	public class LeaderboardUpdateArgs : EventArgs
	{
		public enum UpdateStatus
		{
			Invalid = 0,
			Success = 1,
			Failed = 2
		}

		private UpdateStatus status { get; set; }

		private string message { get; set; }

		private string leaderboardID { get; set; }

		public LeaderboardUpdateArgs(UpdateStatus _status, string _leaderboardID, string _message = "")
		{
			status = _status;
			leaderboardID = _leaderboardID;
			message = _message;
		}

		public UpdateStatus GetStatus()
		{
			return status;
		}

		public string GetLeaderBoardID()
		{
			return leaderboardID;
		}

		public string GetMessage()
		{
			return message;
		}
	}
}
