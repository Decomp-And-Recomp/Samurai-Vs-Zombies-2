using System;
using System.Collections.Generic;
using UnityEngine;

public static class TaggedAnimPlayerAdapter
{
	public static void Deserialize(TaggedAnimPlayer animPlayer, GameObject model, string animTagRootPath, TaggedAnimPlayerSchema animPlayerData)
	{
		if (animPlayer == null || model == null || animPlayerData == null)
		{
			return;
		}
		Animation component = animPlayer.GetComponent<Animation>();
		Animation component2 = model.GetComponent<Animation>();
		if (!(component != null) || !(component2 != null))
		{
			return;
		}
		string text = FileUtil.GetResourcePath((!string.IsNullOrEmpty(animPlayerData.overrideAnimFolder)) ? animPlayerData.overrideAnimFolder : string.Format("{0}{1}/", animTagRootPath, "_anim_tags"));
		if (!text.EndsWith("/"))
		{
			text += "/";
		}
		List<AnimationClip> list = new List<AnimationClip>();
		foreach (AnimationState item in component2)
		{
			if (item != null)
			{
				string text2 = text + item.name;
				AnimationClip animationClip = ((!Application.isPlaying) ? (Resources.Load(text2) as AnimationClip) : (ResourceCache.GetCachedResource(string.Format("Assets/Game/Resources/{0}.anim", text2), 1).Resource as AnimationClip));
				if (animationClip != null)
				{
					component.AddClip(animationClip, item.name);
					list.Add(animationClip);
				}
			}
		}
		if (!(animPlayer != null))
		{
			return;
		}
		Transform transform = model.transform;
		animPlayer.jointAnimation = component2;
		animPlayer.overrideAnimFolder = animPlayerData.overrideAnimFolder;
		if (animPlayerData == null)
		{
			return;
		}
		animPlayer.defaultBlendSpeed = animPlayerData.defaultBlendSpeed;
		animPlayer.alsoPlayOnAllChildren = animPlayerData.alsoPlayOnAllChildren;
		TaggedAnimSettingsSchema[] specificAnimSettings = animPlayerData.SpecificAnimSettings;
		if (specificAnimSettings == null)
		{
			return;
		}
		animPlayer.specificAnimSettings = new TaggedAnimPlayer.AnimOverrideSettings[specificAnimSettings.Length];
		for (int i = 0; i < specificAnimSettings.Length; i++)
		{
			if (specificAnimSettings[i] == null)
			{
				continue;
			}
			TaggedAnimPlayer.AnimOverrideSettings animOverrideSettings = new TaggedAnimPlayer.AnimOverrideSettings();
			animPlayer.specificAnimSettings[i] = animOverrideSettings;
			if (!string.IsNullOrEmpty(specificAnimSettings[i].clipName))
			{
				animOverrideSettings.clip = list.Find((AnimationClip ac) => ac.name.IndexOf(specificAnimSettings[i].clipName, StringComparison.Ordinal) != -1);
			}
			animOverrideSettings.overrideBlendSpeed = specificAnimSettings[i].overrideBlendSpeed;
			animOverrideSettings.blendSpeed = specificAnimSettings[i].blendSpeed;
			if (!string.IsNullOrEmpty(specificAnimSettings[i].jointMaskRootName))
			{
				animOverrideSettings.jointMaskRoot = ObjectUtils.FindTransformInChildren(transform, (Transform t) => t.name.IndexOf(specificAnimSettings[i].jointMaskRootName, StringComparison.Ordinal) != -1);
			}
			animOverrideSettings.onlyUseSingleJoint = specificAnimSettings[i].onlyUseSingleJoint;
		}
	}
}
