public class HandInfo
{
	private int iMaxNumberOfFingers;

	public FingerInfo[] fingers;

	public HandInfo(int fingerCount)
	{
		iMaxNumberOfFingers = 3;
		fingers = new FingerInfo[iMaxNumberOfFingers];
		for (int i = 0; i < fingers.Length; i++)
		{
			fingers[i] = new FingerInfo();
			fingers[i].ResetVariables();
		}
	}
}
