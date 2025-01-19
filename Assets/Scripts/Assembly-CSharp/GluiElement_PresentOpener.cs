using UnityEngine;

public class GluiElement_PresentOpener : GluiElement_DataAdaptor<DataAdaptor_PresentOpener>
{
	private float mTimer = -1f;

	private GluiPopupQueueMachine mPopupMachine;

	private void Start()
	{
		mPopupMachine = GameObject.Find("Machine_Popups").GetComponent<GluiPopupQueueMachine>();
	}

	private void Update()
	{
		if ((mPopupMachine == null || mPopupMachine.IsCurrentDefaultState) && mTimer > 0f)
		{
			mTimer -= GluiTime.deltaTime;
			if (mTimer <= 0f)
			{
				adaptor.OpenPresent();
			}
		}
	}

	public void SetOpeningTimer(float timer)
	{
		mTimer = timer;
	}
}
