using System;

public abstract class GluiTransition : GluiBase
{
	public enum Position
	{
		Start = 0,
		End = 1,
		Other = 2,
		None = 3
	}

	public enum TransitionSet
	{
		Forward_Only = 0,
		Backward_Only = 1,
		Forward_And_Back_Same = 2,
		Forward_And_Back_Separate = 3
	}

	public enum TransitionToUse
	{
		None = 0,
		Main = 1,
		Second = 2
	}

	public Action<Position> onDoneCallback;

	public TransitionSet transitionSet = TransitionSet.Forward_And_Back_Same;

	public abstract bool IsDone { get; }

	public abstract bool Transition_Forward();

	public abstract bool Transition_Forward(bool reverse);

	public abstract bool Transition_Back();

	public abstract bool Transition_Back(bool reverse);

	protected virtual void GetTransitionImplemented(Position positionTarget, out TransitionToUse transitionToUse, out bool pathDirectionForward)
	{
		transitionToUse = TransitionToUse.None;
		pathDirectionForward = true;
		switch (transitionSet)
		{
		case TransitionSet.Forward_Only:
			if (positionTarget == Position.End)
			{
				transitionToUse = TransitionToUse.Main;
			}
			break;
		case TransitionSet.Backward_Only:
			if (positionTarget == Position.Start)
			{
				transitionToUse = TransitionToUse.Main;
			}
			break;
		case TransitionSet.Forward_And_Back_Same:
			if (positionTarget == Position.End)
			{
				transitionToUse = TransitionToUse.Main;
			}
			if (positionTarget == Position.Start)
			{
				pathDirectionForward = false;
				transitionToUse = TransitionToUse.Main;
			}
			break;
		case TransitionSet.Forward_And_Back_Separate:
			if (positionTarget == Position.End)
			{
				transitionToUse = TransitionToUse.Main;
			}
			if (positionTarget == Position.Start)
			{
				transitionToUse = TransitionToUse.Second;
			}
			break;
		}
	}
}
