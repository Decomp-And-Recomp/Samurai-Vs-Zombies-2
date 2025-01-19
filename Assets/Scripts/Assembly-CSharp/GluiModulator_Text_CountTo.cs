using UnityEngine;

[AddComponentMenu("Glui Modulator/Text CountTo")]
public class GluiModulator_Text_CountTo : GluiModulator_CountTo_Base
{
	public override void Awake()
	{
		base.Awake();
		if (!skipCountingOnFirstChange)
		{
			string text = GetText();
			OnTextChanged(ref text, text);
		}
	}

	protected override void OnTextChanged(ref string text, string originalText)
	{
		float result;
		if (float.TryParse(originalText, out result))
		{
			OnValueChanged(ref result, 0f);
			SetText(previousValue.ToString());
		}
	}
}
