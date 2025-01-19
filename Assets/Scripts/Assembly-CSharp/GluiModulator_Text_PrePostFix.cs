using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Glui Modulator/Text PrePostFix")]
public class GluiModulator_Text_PrePostFix : GluiModulator
{
	public string prefix = string.Empty;

	public string postfix = string.Empty;

	protected override void OnTextChanged(ref string text, string originalText)
	{
		text = prefix + text + postfix;
	}
}
