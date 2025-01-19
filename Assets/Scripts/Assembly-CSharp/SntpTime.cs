using System;
using DaveyM69.Components;
using DaveyM69.Components.SNTP;

public class SntpTime
{
	private SNTPClient mSntpClient;

	private static TimeSpan mTimeOffset;

	private bool mSNTPSuccessful;

	public bool SNTPSuccessful
	{
		get
		{
			return mSNTPSuccessful;
		}
	}

	public static DateTime CurrentTime
	{
		get
		{
			return DateTime.Now + mTimeOffset;
		}
	}

	public static DateTime UniversalTime
	{
		get
		{
			return CurrentTime.ToUniversalTime();
		}
	}

	public void UpdateTimeOffset()
	{
		if (mSntpClient == null)
		{
			mSntpClient = new SNTPClient();
			mSntpClient.QueryServerCompleted += TimeServerCallback;
			mSntpClient.QueryServerAsync();
		}
	}

	private void TimeServerCallback(object sntpClient, QueryServerCompletedEventArgs args)
	{
		if (args.Succeeded)
		{
			mTimeOffset = new TimeSpan(0, 0, 0, (int)args.Data.LocalClockOffset);
			mSNTPSuccessful = true;
		}
		else
		{
			mTimeOffset = default(TimeSpan);
			mSNTPSuccessful = false;
		}
		if (mSntpClient != null)
		{
			mSntpClient.QueryServerCompleted -= TimeServerCallback;
			mSntpClient = null;
		}
	}
}
