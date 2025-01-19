using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DataAdaptor_EnemyInfo : DataAdaptorBase
{
	private const float minEnemySpeed = 0.85f;

	private const float maxEnemySpeed = 2.5f;

	public GluiText name;

	public GameObject[] speedStars;

	public GluiText health;

	public GluiText damage;

	public GameObject descriptionParentObject;

	public GluiText description;

	public GameObject selectionArrow;

	public override void SetData(object data)
	{
		if (data == null || !(data is Character))
		{
			return;
		}
		Character character = (Character)data;
		EnemySchema enemySchema = null;
		HelperSchema helperSchema = null;
		if (Singleton<EnemiesDatabase>.Instance.Contains(character.id))
		{
			enemySchema = Singleton<EnemiesDatabase>.Instance[character.id];
		}
		else
		{
			if (!Singleton<HelpersDatabase>.Instance.Contains(character.id))
			{
				return;
			}
			helperSchema = Singleton<HelpersDatabase>.Instance[character.id];
		}
		if (name != null)
		{
			if (enemySchema != null)
			{
				name.Text = StringUtils.GetStringFromStringRef(enemySchema.displayName);
			}
			else
			{
				name.Text = StringUtils.GetStringFromStringRef(helperSchema.displayName);
			}
		}
		if (speedStars != null && speedStars.Length > 0)
		{
			int num = 0;
			if (enemySchema != null)
			{
				float num2 = Mathf.Clamp((enemySchema.speedMin + enemySchema.speedMax) / 2f, 0.85f, 2.5f) - 0.85f;
				num = (int)Mathf.Clamp(num2 / 1.65f * (float)speedStars.Length + 1f, 1f, speedStars.Length);
			}
			for (int i = 0; i < speedStars.Length; i++)
			{
				speedStars[i].SetActive(i < num);
			}
		}
		if (health != null)
		{
			if (enemySchema != null)
			{
				health.Text = StringUtils.FormatAmountString(enemySchema.health);
			}
			else
			{
				health.Text = "* TODO!!! *";
			}
		}
		if (damage != null)
		{
			if (enemySchema != null)
			{
				damage.Text = StringUtils.FormatAmountString(Mathf.Max(enemySchema.meleeDamage, enemySchema.bowDamage));
			}
			else
			{
				damage.Text = "* TODO!!! *";
			}
		}
		if (description != null)
		{
			bool active = false;
			if (enemySchema != null)
			{
				if (enemySchema.specialsDesc != null && !string.IsNullOrEmpty(enemySchema.specialsDesc))
				{
					string stringFromStringRef = StringUtils.GetStringFromStringRef(enemySchema.specialsDesc);
					if (!string.IsNullOrEmpty(stringFromStringRef))
					{
						active = true;
						description.Text = stringFromStringRef;
					}
				}
				else
				{
					Dictionary<string, string> dictionary = CharacterDisplaySchema.PropertyLookups();
					string text = string.Empty;
					bool flag = true;
					if (enemySchema.boss)
					{
						text += dictionary["Boss"];
						flag = false;
					}
					if (enemySchema.gateRusher)
					{
						text = text + ((!flag) ? ", " : string.Empty) + dictionary["GateRusher"];
						flag = false;
					}
					if (enemySchema.flying)
					{
						text = text + ((!flag) ? ", " : string.Empty) + dictionary["Flying"];
						flag = false;
					}
					if (enemySchema.exploseOnMelee)
					{
						text = text + ((!flag) ? ", " : string.Empty) + dictionary["Explodes"];
						flag = false;
					}
					if (!DataBundleRecordKey.IsNullOrEmpty(enemySchema.spawnOnDeath) || enemySchema.spawnOnDeathCount > 0)
					{
						text = text + ((!flag) ? ", " : string.Empty) + dictionary["Spawns"];
						flag = false;
					}
					else if (WeakGlobalInstance<WaveManager>.Instance != null)
					{
						DataBundleTableHandle<EnemySwapSchema> deathSwapData = WeakGlobalInstance<WaveManager>.Instance.GetDeathSwapData();
						EnemySwapSchema[] data2 = deathSwapData.Data;
						EnemySwapSchema[] array = data2;
						foreach (EnemySwapSchema enemySwapSchema in array)
						{
							string text2 = enemySwapSchema.swapFrom.Key.ToString();
							if (text2 == enemySchema.id)
							{
								text = text + ((!flag) ? ", " : string.Empty) + dictionary["Spawns"];
								flag = false;
								break;
							}
						}
					}
					if (enemySchema.eatCooldown > 0f)
					{
						text = text + ((!flag) ? ", " : string.Empty) + dictionary["Eats"];
						flag = false;
					}
					if (enemySchema.damageBuffPercent > 0f)
					{
						text = text + ((!flag) ? ", " : string.Empty) + dictionary["Inspires"];
						flag = false;
					}
					if (!DataBundleRecordKey.IsNullOrEmpty(enemySchema.projectile))
					{
						switch (enemySchema.projectile.Key)
						{
						case "EvilHealBolt":
							text = text + ((!flag) ? ", " : string.Empty) + dictionary["Heals"];
							flag = false;
							break;
						case "Corruption":
							text = text + ((!flag) ? ", " : string.Empty) + dictionary["Corrupts"];
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						active = true;
						description.Text = text;
					}
				}
				descriptionParentObject.SetActive(active);
			}
		}
		if (selectionArrow != null)
		{
			Vector3 position = selectionArrow.transform.position;
			position.x = character.rootObject.transform.position.x;
			selectionArrow.transform.position = position;
		}
	}
}
