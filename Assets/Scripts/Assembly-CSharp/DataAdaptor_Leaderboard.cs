using System;

[Serializable]
public class DataAdaptor_Leaderboard : DataAdaptorBase
{
	public GluiText text_playerName;

	public GluiText text_rank;

	public GluiText text_score;

	private GameCenterScore score;

	public override void SetData(object data)
	{
		score = data as GameCenterScore;
		if (score != null)
		{
			if (text_playerName != null)
			{
				text_playerName.Text = score.alias;
			}
			if (text_rank != null)
			{
				text_rank.Localize = false;
				text_rank.Text = score.rank.ToString();
			}
			if (text_score != null)
			{
				text_score.Localize = false;
				text_score.Text = score.value.ToString();
			}
		}
	}
}
