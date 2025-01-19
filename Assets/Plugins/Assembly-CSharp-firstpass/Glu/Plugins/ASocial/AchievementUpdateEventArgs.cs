using System;

namespace Glu.Plugins.ASocial
{
	public class AchievementUpdateEventArgs : EventArgs
	{
		public enum UpdateStatus
		{
			Invalid = 0,
			Success = 1,
			Failed = 2
		}

		private UpdateStatus status { get; set; }

		private string message { get; set; }

		private string achievementID { get; set; }

		public AchievementUpdateEventArgs(UpdateStatus _status, string _achievementID, string _message = "")
		{
			status = _status;
			achievementID = _achievementID;
			message = _message;
		}

		public UpdateStatus GetStatus()
		{
			return status;
		}

		public string GetAchievementID()
		{
			return achievementID;
		}

		public string GetMessage()
		{
			return message;
		}
	}
}
