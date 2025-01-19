using System;
using System.Collections.Generic;
using UnityEngine;

public class FSM
{
	public enum StateCallBackType
	{
		OnPreEnterCallBack = 0,
		OnPostEnterCallBack = 1,
		OnPreExitCallBack = 2,
		OnPostExitCallBack = 3,
		TOTAL = 4
	}

	public bool EnableFSM = true;

	private IFSMState mQueuedState;

	private IFSMState mCurrentState;

	private bool mForceStateChange;

	private bool mbInterrupted;

	private GameObject mGameObject;

	private object mOwner;

	private List<IFSMState> mRegisterStateList;

	private bool DebugMode { get; set; }

	public FSM()
	{
		Init(null);
	}

	public FSM(GameObject go)
	{
		Init(go);
	}

	public void Init(object owner)
	{
		mOwner = owner;
		if (owner != null && owner is GameObject)
		{
			mGameObject = (GameObject)mOwner;
		}
		mQueuedState = null;
		mCurrentState = null;
		mRegisterStateList = new List<IFSMState>();
	}

	public void UpdateFSM()
	{
		if (!EnableFSM)
		{
			return;
		}
		if (mCurrentState != null)
		{
			FSMStateData stateData = mCurrentState.GetStateData();
			if (stateData != null && stateData.DeadState)
			{
				return;
			}
		}
		if (mQueuedState != null && (mQueuedState != mCurrentState || mForceStateChange))
		{
			FSMStateData fSMStateData = null;
			if (mCurrentState != null && mQueuedState != null)
			{
				fSMStateData = mCurrentState.GetStateData();
				if (fSMStateData != null && fSMStateData.AllowOnExit)
				{
					GenericUtils.TryInvoke(mCurrentState.GetCallBack(StateCallBackType.OnPreExitCallBack), null);
					GenericUtils.TryInvoke(fSMStateData.OnExitMethod, this, GetStateName(mQueuedState));
					GenericUtils.TryInvoke(mCurrentState.GetCallBack(StateCallBackType.OnPostExitCallBack), null);
				}
			}
			int stateName = GetStateName(mCurrentState);
			mCurrentState = mQueuedState;
			mQueuedState = null;
			fSMStateData = mCurrentState.GetStateData();
			GenericUtils.TryInvoke(mCurrentState.GetCallBack(StateCallBackType.OnPreEnterCallBack), null);
			if (fSMStateData != null)
			{
				GenericUtils.TryInvoke(fSMStateData.OnEnterMethod, this, stateName);
			}
			GenericUtils.TryInvoke(mCurrentState.GetCallBack(StateCallBackType.OnPostEnterCallBack), null);
			if (!DebugMode)
			{
			}
		}
		else if (mQueuedState == mCurrentState)
		{
			mQueuedState = null;
		}
		if (mCurrentState == null)
		{
			return;
		}
		FSMStateData stateData2 = mCurrentState.GetStateData();
		if (stateData2 != null)
		{
			if (WasInterrupted())
			{
				ResetInterrupt();
				GenericUtils.TryInvoke(stateData2.OnResumeFromInterruptMethod, this);
			}
			else if (stateData2.AllowOnUpdate)
			{
				GenericUtils.TryInvoke(stateData2.OnUpdateMethod, this);
			}
		}
	}

	public void QueueState(int iName)
	{
		QueueState(iName, false);
	}

	public void QueueState<T>(T iName)
	{
		QueueState((int)(object)iName, false);
	}

	public void QueueState(int iName, bool bForceStateChange)
	{
		mForceStateChange = bForceStateChange;
		mQueuedState = mRegisterStateList[iName];
	}

	public void QueueState<T>(T iName, bool bForceStateChange)
	{
		QueueState((int)(object)iName, bForceStateChange);
	}

	public int GetQueuedState()
	{
		return GetStateName(mQueuedState);
	}

	public int GetCurrentState()
	{
		return GetStateName(mCurrentState);
	}

	public int GetStateName(IFSMState stateToFind)
	{
		for (int i = 0; i < mRegisterStateList.Count; i++)
		{
			IFSMState iFSMState = mRegisterStateList[i];
			if (iFSMState == stateToFind)
			{
				return i;
			}
		}
		return -1;
	}

	public IFSMState GetStateByName(int iName)
	{
		return mRegisterStateList[iName];
	}

	public void RegisterStateCallback<T>(T iName, StateCallBackType cbType, Action callBack)
	{
		IFSMState stateByName = GetStateByName((int)(object)iName);
		if (stateByName != null)
		{
			stateByName.SetCallBack(cbType, callBack);
		}
	}

	public void RegisterState<T>(T iName, Action<FSM, int> OnEnter, Action<FSM> OnUpdate, Action<FSM, int> OnExit)
	{
		RegisterState(iName, null, OnEnter, OnUpdate, OnExit);
	}

	public void RegisterState<T>(T iName, Action<FSM> Init, Action<FSM, int> OnEnter, Action<FSM> OnUpdate, Action<FSM, int> OnExit)
	{
		RegisterState((int)(object)iName, Init, OnEnter, OnUpdate, OnExit, null);
	}

	public void RegisterState<T>(T iName, Action<FSM> Init, Action<FSM, int> OnEnter, Action<FSM> OnUpdate, Action<FSM, int> OnExit, Action<FSM> OnResumeFromInterrupt)
	{
		DoNothingState doNothingState = new DoNothingState();
		FSMStateData fSMStateData = doNothingState.GetStateData();
		if (fSMStateData == null)
		{
			fSMStateData = new FSMStateData();
		}
		fSMStateData.IsClass = false;
		fSMStateData.InitMethod = Init;
		fSMStateData.OnEnterMethod = OnEnter;
		fSMStateData.OnUpdateMethod = OnUpdate;
		fSMStateData.OnExitMethod = OnExit;
		fSMStateData.OnResumeFromInterruptMethod = OnResumeFromInterrupt;
		RegisterState((int)(object)iName, doNothingState, fSMStateData);
	}

	public void RegisterState<T>(T iName, IFSMState state)
	{
		RegisterState((int)(object)iName, state, null);
	}

	public void RegisterState(int iName, IFSMState state)
	{
		RegisterState(iName, state, null);
	}

	public void RegisterState(int iName, IFSMState state, bool DeadState)
	{
		if (state != null)
		{
			FSMStateData fSMStateData = state.GetStateData();
			if (fSMStateData == null)
			{
				fSMStateData = new FSMStateData();
			}
			fSMStateData.DeadState = DeadState;
			RegisterState(iName, state, fSMStateData);
		}
	}

	public void RegisterState(int iName, IFSMState state, FSMStateData data)
	{
		if (state != null)
		{
			if (mRegisterStateList.Count <= iName)
			{
				int num = iName - mRegisterStateList.Count + 1;
				IFSMState[] collection = new IFSMState[num];
				mRegisterStateList.AddRange(collection);
			}
			if (data == null)
			{
				data = new FSMStateData();
			}
			if (data.IsClass)
			{
				data.InitMethod = state.Init;
				data.OnEnterMethod = state.OnEnter;
				data.OnUpdateMethod = state.OnUpdate;
				data.OnExitMethod = state.OnExit;
				data.OnResumeFromInterruptMethod = state.OnResumeFromInterrupt;
			}
			state.SetStateData(data);
			mRegisterStateList[iName] = state;
			GenericUtils.TryInvoke(data.InitMethod, this);
		}
	}

	public bool IsCurrentOrQueuedState<T>(T iName)
	{
		return IsCurrentOrQueuedState((int)(object)iName);
	}

	public bool IsCurrentOrQueuedState(int iName)
	{
		if (GetCurrentState() == iName || GetQueuedState() == iName)
		{
			return true;
		}
		return false;
	}

	public object GetOwnerObject()
	{
		return mOwner;
	}

	public GameObject GetGameObject()
	{
		return mGameObject;
	}

	public string CurrentStateName()
	{
		if (mCurrentState == null)
		{
			return "null";
		}
		return mCurrentState.GetType().FullName;
	}

	public void Interrupt()
	{
		mbInterrupted = true;
	}

	private void ResetInterrupt()
	{
		mbInterrupted = false;
	}

	private bool WasInterrupted()
	{
		return mbInterrupted;
	}
}
