using System;

public static class EloRating
{
	public static float Win
	{
		get
		{
			return 1f;
		}
	}

	public static float Draw
	{
		get
		{
			return 0.5f;
		}
	}

	public static float Loss
	{
		get
		{
			return 0f;
		}
	}

	public static float ExpectedScore(int ratingA, int ratingB)
	{
		return 1f / (1f + (float)Math.Pow(10.0, (double)(ratingB - ratingA) / 400.0));
	}

	public static int CalculateRating(int originalRanking, float expectedScore, float actualScore, int kFactor)
	{
		return originalRanking + (int)((float)kFactor * (actualScore - expectedScore));
	}
}
