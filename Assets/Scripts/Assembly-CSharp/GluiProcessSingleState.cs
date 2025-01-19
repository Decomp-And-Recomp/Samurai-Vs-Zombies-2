public abstract class GluiProcessSingleState : GluiProcessBase
{
	public GluiStatePhase phaseOfStateToAutomaticallyImplement;

	public override bool AnyPhaseHandled()
	{
		return phaseOfStateToAutomaticallyImplement != GluiStatePhase.None;
	}

	public override bool IsPhaseHandled(GluiStatePhase phase)
	{
		return phase == phaseOfStateToAutomaticallyImplement;
	}

	public override GluiStatePhase[] PhasesHandled()
	{
		return new GluiStatePhase[1] { phaseOfStateToAutomaticallyImplement };
	}
}
