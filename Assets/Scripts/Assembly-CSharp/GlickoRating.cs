using System;

public static class GlickoRating
{
	public static float q = 0.0057565f;

	public static float qSquared = 3.3136E-05f;

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

	public static int InitialRating
	{
		get
		{
			return 1500;
		}
	}

	public static int InitialRatingsDeviation
	{
		get
		{
			return 350;
		}
	}

	public static float fInitialRating
	{
		get
		{
			return 1500f;
		}
	}

	public static float fInitialRatingsDeviation
	{
		get
		{
			return 350f;
		}
	}

	public static int CalculateRating(int r, int rj, float RD, float RDj, float outcome)
	{
		float num = 1f / (RD * RD) + 1f / d2(r, rj, RDj);
		float num2 = g(RDj) * (outcome - E(r, rj, RDj));
		float num3 = (float)r + q / num * num2;
		return (int)num3;
	}

	public static float CalculateRatingsDeviation(int r, int rj, float RD, float RDj, float minRD)
	{
		float num = 1f / (RD * RD) + 1f / d2(r, rj, RDj);
		return Math.Max((float)Math.Sqrt(1.0 / (double)num), minRD);
	}

	public static float OnsetRatingsDeviation(float RD, int t, float c)
	{
		return Math.Min((float)Math.Sqrt(RD * RD + c * c * (float)t), 350f);
	}

	public static float RecommendedOnsetConstant(float typicalRD, int periodsToUnreliable)
	{
		return (float)Math.Sqrt((122500f - typicalRD * typicalRD) / (float)periodsToUnreliable);
	}

	public static float g(float RD)
	{
		float num = 3f * (qSquared * (RD * RD));
		double d = 1.0 + (double)num / 9.869604401089358;
		return 1f / (float)Math.Sqrt(d);
	}

	public static float E(int r, int rj, float RDj)
	{
		double y = -10f * g(RDj) * (float)(r - rj) / 400f;
		return 1f / (1f + (float)Math.Pow(10.0, y));
	}

	public static float d2(int r, int rj, float RDj)
	{
		float num = E(r, rj, RDj);
		float num2 = g(RDj);
		return 1f / (qSquared * (num2 * num2) * (num * (1f - num)));
	}
}
