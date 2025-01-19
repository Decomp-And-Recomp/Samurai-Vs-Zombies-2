using UnityEngine;

[AddComponentMenu("Effect Maestro/Effect Conductor - Mover Glui")]
public class EffectConductor_Mover_Glui : EffectConductor
{
	public override void Start()
	{
		base.Start();
		effectContainer.EffectEnable();
		UpdatePosition();
	}

	protected void Update()
	{
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		GluiWidget gluiWidget = effectContainer.Owner.GetComponent(typeof(GluiWidget)) as GluiWidget;
		if ((bool)gluiWidget)
		{
			effectContainer.EffectMove(effectContainer.OwnerCamera.WorldToScreenPoint(gluiWidget.EffectAttachPoint));
		}
	}
}
