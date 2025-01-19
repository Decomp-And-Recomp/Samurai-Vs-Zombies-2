using UnityEngine;

public class LegendaryStrikeRaiseDeadHandler : AbilityHandler
{
	public override void Execute(Character executor)
	{
		Hero hero = ((executor == null) ? WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(0) : WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(executor.ownerId));
		Leadership leadership = ((executor == null) ? null : WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(executor.ownerId));
		int num = Random.Range(3, 6);
		int num2 = Random.Range(2, 4);
		int num3 = num - num2;
		if (executor != null && executor.ownerId == 0)
		{
			num3 = 0;
		}
		float spawnOffsetHorizontal = schema.spawnOffsetHorizontal;
		float num4 = ((!leftToRightGameplay) ? spawnOffsetHorizontal : (0f - spawnOffsetHorizontal));
		for (int i = 0; i < num2; i++)
		{
			Vector3 vector = new Vector3(hero.transform.position.x, hero.transform.position.y, hero.transform.position.z + num4 + Random.Range(-1f, 1f));
			vector.x = WeakGlobalInstance<CharactersManager>.Instance.GetBestSpawnXPos(vector, spawnOffsetHorizontal, CharactersManager.ELanePreference.any, false, false, false);
			GameObjectPool.DefaultObjectPool.Acquire(schema.activateFX, vector, Quaternion.identity);
			if (leadership != null)
			{
				string helperID = WeakGlobalMonoBehavior<InGameImpl>.Instance.LegendaryStrikeEnemies[Random.Range(0, WeakGlobalMonoBehavior<InGameImpl>.Instance.LegendaryStrikeEnemies.Count)];
				Character character = leadership.SpawnForFree(helperID, spawnOffsetHorizontal * 0.25f, vector);
				character.dynamicSpawn = true;
			}
			else if (WeakGlobalInstance<WaveManager>.Instance != null)
			{
				WeakGlobalInstance<WaveManager>.Instance.SpawnLegendaryStrikeEnemy(vector, spawnOffsetHorizontal * 0.25f);
			}
		}
		for (int j = 0; j < num3; j++)
		{
			Gate gate = WeakGlobalMonoBehavior<InGameImpl>.Instance.gate;
			if (executor != null)
			{
				gate = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetGate(1 - executor.ownerId);
			}
			if (gate != null)
			{
				Vector3 vector2 = new Vector3(gate.transform.position.x, gate.transform.position.y, gate.transform.position.z + num4);
				vector2.x = WeakGlobalInstance<CharactersManager>.Instance.GetBestSpawnXPos(vector2, spawnOffsetHorizontal, CharactersManager.ELanePreference.any, false, false, false);
				GameObjectPool.DefaultObjectPool.Acquire(schema.activateFX, vector2, Quaternion.identity);
				if (leadership != null)
				{
					string helperID2 = WeakGlobalMonoBehavior<InGameImpl>.Instance.LegendaryStrikeEnemies[Random.Range(0, WeakGlobalMonoBehavior<InGameImpl>.Instance.LegendaryStrikeEnemies.Count)];
					Character character2 = leadership.SpawnForFree(helperID2, spawnOffsetHorizontal * 0.25f, vector2);
					character2.dynamicSpawn = true;
				}
				else if (WeakGlobalInstance<WaveManager>.Instance != null)
				{
					WeakGlobalInstance<WaveManager>.Instance.SpawnRandomEnemy(vector2, spawnOffsetHorizontal * 0.25f);
				}
			}
		}
	}
}
