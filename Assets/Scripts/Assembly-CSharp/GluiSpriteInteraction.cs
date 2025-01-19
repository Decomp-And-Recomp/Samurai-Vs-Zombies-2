using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("Glui/SpriteInteraction")]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class GluiSpriteInteraction : GluiSprite
{
	public override void HandleInput(InputCrawl inputCrawl, out InputRouter.InputResponse response)
	{
		response = InputRouter.InputResponse.Unhandled;
	}
}
