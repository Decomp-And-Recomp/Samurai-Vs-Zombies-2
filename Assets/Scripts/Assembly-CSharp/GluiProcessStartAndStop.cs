public abstract class GluiProcessStartAndStop : GluiProcessBase
{
	public override bool AnyPhaseHandled()
	{
		return true;
	}

	public override bool IsPhaseHandled(GluiStatePhase phase)
	{
		return phase == GluiStatePhase.Init || phase == GluiStatePhase.Exit;
	}

	public override GluiStatePhase[] PhasesHandled()
	{
		return new GluiStatePhase[2]
		{
			GluiStatePhase.Init,
			GluiStatePhase.Exit
		};
	}
}
