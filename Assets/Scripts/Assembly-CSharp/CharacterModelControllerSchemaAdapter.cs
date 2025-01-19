using UnityEngine;

public class CharacterModelControllerSchemaAdapter
{
	public static void Deserialize(CharacterModelController controller, CharacterModelControllerSchema data)
	{
		if (controller == null || data == null)
		{
			return;
		}
		controller.animatedWalkSpeed = data.animatedWalkSpeed;
		controller.arrowImpactEffect = data.arrowImpactEffect;
		controller.bladeImpactEffect = data.bladeImpactEffect;
		controller.bladeCriticalImpactEffect = data.bladeCriticalImpactEffect;
		controller.bluntImpactEffect = data.bluntImpactEffect;
		controller.bluntCriticalImpactEffect = data.bluntCriticalImpactEffect;
		controller.snapToGround = data.snapToGround;
		if (data.RandomAnimSets == null)
		{
			return;
		}
		controller.randomAnimSets = new TaggedAnimPlayerController.RandomAnimSet[data.RandomAnimSets.Length];
		for (int i = 0; i < data.RandomAnimSets.Length; i++)
		{
			RandomAnimationClipSetSchema randomAnimationClipSetSchema = data.RandomAnimSets[i];
			TaggedAnimPlayerController.RandomAnimSet randomAnimSet = new TaggedAnimPlayerController.RandomAnimSet();
			controller.randomAnimSets[i] = randomAnimSet;
			randomAnimSet.name = randomAnimationClipSetSchema.name;
			randomAnimSet.onlyRandomizeOnce = randomAnimationClipSetSchema.onlyRandomizeOnce;
			if (randomAnimationClipSetSchema.Clips != null)
			{
				randomAnimSet.clips = new AnimationClip[randomAnimationClipSetSchema.Clips.Length];
				for (int j = 0; j < randomAnimationClipSetSchema.Clips.Length; j++)
				{
					randomAnimSet.clips[j] = randomAnimationClipSetSchema.Clips[j].clip;
				}
			}
		}
		controller.UpdateRandomAnimSets();
	}
}
