using System;

namespace Glu.Plugins.ASocial
{
	public class AchievementsQueryArgs : EventArgs
	{
		public enum QueryStatus
		{
			Invalid = 0,
			Success = 1,
			Failed = 2
		}

		private QueryStatus status { get; set; }

		private string message { get; set; }

		private float percentileComplete { get; set; }

		private bool achievementHidden { get; set; }

		private bool achievementUnlocked { get; set; }

		public AchievementsQueryArgs(QueryStatus _status, string _message = "")
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

		internal void SetPercentileComplete(float _percentComplete)
		{
			percentileComplete = _percentComplete;
		}

		public float GetPercentileComplete()
		{
			return percentileComplete;
		}

		internal void SetHidden(bool _hidden)
		{
			achievementHidden = _hidden;
		}

		public bool IsAchievementHidden()
		{
			return achievementHidden;
		}

		internal void SetUnlocked(bool _unlocked)
		{
			achievementUnlocked = _unlocked;
		}

		public bool IsAchievementUnlocked()
		{
			return achievementUnlocked;
		}
	}
}
