using System;
using System.Collections.Generic;

public class FSMStateData
{
	private List<object> lstProperties;

	private bool allowOnUpdate = true;

	private bool allowOnExit = true;

	public bool DeadState;

	public bool IsClass = true;

	public Action<FSM> InitMethod;

	public Action<FSM, int> OnEnterMethod;

	public Action<FSM> OnUpdateMethod;

	public Action<FSM, int> OnExitMethod;

	public Action<FSM> OnResumeFromInterruptMethod;

	public bool AllowOnUpdate
	{
		get
		{
			return allowOnUpdate;
		}
		set
		{
			allowOnUpdate = value;
		}
	}

	public bool AllowOnExit
	{
		get
		{
			return allowOnExit;
		}
		set
		{
			allowOnExit = value;
		}
	}

	public virtual object GetProperty(int iName)
	{
		if (lstProperties != null && iName < lstProperties.Count)
		{
			return lstProperties[iName];
		}
		return null;
	}

	public virtual void SetProperty(int iName, object val)
	{
		if (lstProperties == null)
		{
			lstProperties = new List<object>(iName + 10);
		}
		if (lstProperties == null)
		{
			return;
		}
		if (lstProperties.Count < iName)
		{
			List<object> list = new List<object>(lstProperties);
			int num = 50;
			int count = list.Count;
			for (int i = count; i < iName + num; i++)
			{
				list.Add(null);
			}
			lstProperties.Clear();
			lstProperties = list;
		}
		lstProperties[iName] = val;
	}
}
