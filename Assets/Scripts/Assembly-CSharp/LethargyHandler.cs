using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/LethargyHandler")]
public class LethargyHandler : AbilityHandlerComponent
{
	private float mRemainingDuration;

	private void Start()
	{
		mRemainingDuration = Extrapolate((AbilityLevelSchema als) => als.effectDuration);
		List<Character> playerCharacters = WeakGlobalInstance<CharactersManager>.Instance.GetPlayerCharacters(1 - base.handlerObject.activatingPlayer);
		float speedModifier = Extrapolate((AbilityLevelSchema als) => als.effectModifier);
		float fadeInTime = float.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(schema.id, "ColorEffectFadeIn"));
		float fadeOutTime = float.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(schema.id, "ColorEffectFadeOut"));
		GameObject resultFX = schema.resultFX;
		foreach (Character item in playerCharacters)
		{
			if (!item.isBase)
			{
				item.ApplyBuff(0f, speedModifier, mRemainingDuration, base.gameObject, resultFX, "head_effect");
				Color color = new Color((float)int.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(schema.id, "Red")) / 255f, (float)int.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(schema.id, "Green")) / 255f, (float)int.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(schema.id, "Blue")) / 255f);
				item.MaterialColorFadeInOut(color, fadeInTime, mRemainingDuration, fadeOutTime);
			}
		}
	}

	private void Update()
	{
		if (mRemainingDuration > 0f)
		{
			mRemainingDuration -= Time.deltaTime;
		}
		else
		{
			GameObjectPool.DefaultObjectPool.Release(base.gameObject);
		}
	}
}
