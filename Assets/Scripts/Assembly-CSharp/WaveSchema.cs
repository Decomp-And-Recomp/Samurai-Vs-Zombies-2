using System.Collections.Generic;
using UnityEngine;

[DataBundleClass(Category = "Design")]
public class WaveSchema
{
	private class ExpandedGroupCommand
	{
		public int indexInEnemyGroupList = -1;

		public List<WaveCommandSchema> expandedRawCommands = new List<WaveCommandSchema>();

		public ExpandedGroupCommand(int index)
		{
			indexInEnemyGroupList = index;
		}
	}

	private class EnemyGroupInfo
	{
		public DataBundleRecordTable enemyGroupTable;

		public int numTimesUsed;

		public List<ExpandedGroupCommand> expandedGroupCommands = new List<ExpandedGroupCommand>();

		public EnemyGroupSchema[] schemasFromThisGroup;

		public EnemyGroupInfo(DataBundleRecordTable groupTable)
		{
			enemyGroupTable = groupTable;
			schemasFromThisGroup = enemyGroupTable.InitializeRecords<EnemyGroupSchema>();
		}
	}

	private static int kMaxEnemyTypes = 12;

	[DataBundleKey]
	public int index;

	public string scene;

	public string tutorial;

	public int bell;

	public int villageArchers;

	[DataBundleField(ColumnWidth = 200)]
	[DataBundleSchemaFilter(typeof(UMusicEventSchema), false)]
	public DataBundleRecordKey music;

	[DataBundleSchemaFilter(typeof(WaveRewardSchema), false)]
	public DataBundleRecordKey rewards;

	[DataBundleSchemaFilter(typeof(WaveCommandSchema), false)]
	public DataBundleRecordTable commands;

	[DataBundleSchemaFilter(typeof(HeroSchema), false)]
	public DataBundleRecordKey recommendedHero;

	public bool recommendedHeroIsRequired;

	[DataBundleSchemaFilter(typeof(AIEnemySchema), false)]
	public DataBundleRecordKey VsModeAIOpponent;

	public bool showRateMe;

	public bool awardMysteryBox;

	[DataBundleSchemaFilter(typeof(EnemySchema), false)]
	public DataBundleRecordKey BossEnemy;

	public WaveRewardSchema Rewards { get; set; }

	public WaveCommandSchema[] Commands { get; set; }

	public static WaveSchema Initialize(DataBundleRecordKey record)
	{
		WaveSchema result = null;
		if (!DataBundleRecordKey.IsNullOrEmpty(record))
		{
			result = Initialize(record.InitializeRecord<WaveSchema>());
		}
		return result;
	}

	public static WaveSchema Initialize(WaveSchema schema)
	{
		if (schema != null)
		{
			schema.Rewards = schema.rewards.InitializeRecord<WaveRewardSchema>();
			schema.Commands = schema.commands.InitializeRecords<WaveCommandSchema>();
		}
		return schema;
	}

	public static WaveSchema Initialize(DataBundleRecordKey record, SeededRandomizer randomizer)
	{
		WaveSchema waveSchema = null;
		if (!DataBundleRecordKey.IsNullOrEmpty(record))
		{
			waveSchema = record.InitializeRecord<WaveSchema>();
			randomizer.PushRandomSession(waveSchema);
			List<EnemyGroupInfo> list = new List<EnemyGroupInfo>();
			int num = 0;
			List<string> list2 = new List<string>(kMaxEnemyTypes);
			foreach (WaveCommandSchema item in waveSchema.commands.EnumerateRecords<WaveCommandSchema>())
			{
				if (string.IsNullOrEmpty(item.enemyGroup))
				{
					string text = item.enemy;
					if (!string.IsNullOrEmpty(text) && !list2.Contains(item.enemy) && list2.Count < kMaxEnemyTypes)
					{
						list2.Add(text);
					}
				}
			}
			foreach (WaveCommandSchema item2 in waveSchema.commands.EnumerateRecords<WaveCommandSchema>())
			{
				DataBundleRecordTable enemyGroup = item2.enemyGroup;
				if (!string.IsNullOrEmpty(enemyGroup))
				{
					EnemyGroupInfo enemyGroupInfo = list.Find((EnemyGroupInfo info) => info.enemyGroupTable.RecordTable == enemyGroup.RecordTable);
					if (enemyGroupInfo == null)
					{
						enemyGroupInfo = new EnemyGroupInfo(enemyGroup);
						list.Add(enemyGroupInfo);
					}
					enemyGroupInfo.numTimesUsed++;
					num++;
				}
			}
			if (list.Count > 0)
			{
				int num2 = 0;
				while ((list2.Count < kMaxEnemyTypes || list.FindIndex((EnemyGroupInfo info) => info.expandedGroupCommands.Count <= 0) >= 0) && num2++ < 100)
				{
					list.Sort(delegate(EnemyGroupInfo a, EnemyGroupInfo b)
					{
						float num4 = (float)a.expandedGroupCommands.Count / (float)a.numTimesUsed;
						float value = (float)b.expandedGroupCommands.Count / (float)b.numTimesUsed;
						return num4.CompareTo(value);
					});
					EnemyGroupInfo groupInfoToExpand = list[0];
					ExpandRandomEnemyGroup(waveSchema, randomizer, list2, groupInfoToExpand);
				}
				for (int num3 = list.Count - 1; num3 >= 0; num3--)
				{
					if (list[num3].expandedGroupCommands.Count <= 0 || list[num3].expandedGroupCommands.FindIndex((ExpandedGroupCommand command) => command.expandedRawCommands.Count <= 0) >= 0)
					{
						list.RemoveAt(num3);
					}
				}
			}
			waveSchema = Initialize(waveSchema, randomizer, list);
			randomizer.PopRandomSession(waveSchema);
		}
		return waveSchema;
	}

	private static void ExpandRandomEnemyGroup(WaveSchema schema, SeededRandomizer randomizer, List<string> enemyTypesInUse, EnemyGroupInfo groupInfoToExpand)
	{
		EnemyGroupSchema[] schemasFromThisGroup = groupInfoToExpand.schemasFromThisGroup;
		int num = randomizer.NextRange(0, schemasFromThisGroup.Length - 1, schema);
		bool flag = false;
		for (int i = 0; i < schemasFromThisGroup.Length; i++)
		{
			if (flag)
			{
				break;
			}
			int indexToUse = (num + i) % schemasFromThisGroup.Length;
			EnemyGroupSchema enemyGroupSchema = schemasFromThisGroup[indexToUse];
			if (enemyGroupSchema == null)
			{
				continue;
			}
			int num2 = kMaxEnemyTypes - enemyTypesInUse.Count;
			int num3 = 0;
			if (!string.IsNullOrEmpty(enemyGroupSchema.enemy_1) && !enemyTypesInUse.Contains(enemyGroupSchema.enemy_1))
			{
				num3++;
			}
			if (!string.IsNullOrEmpty(enemyGroupSchema.enemy_2) && !enemyTypesInUse.Contains(enemyGroupSchema.enemy_2))
			{
				num3++;
			}
			if (!string.IsNullOrEmpty(enemyGroupSchema.enemy_3) && !enemyTypesInUse.Contains(enemyGroupSchema.enemy_3))
			{
				num3++;
			}
			if (num3 > num2 || groupInfoToExpand.expandedGroupCommands.FindIndex((ExpandedGroupCommand groupCommand) => groupCommand.indexInEnemyGroupList == indexToUse) >= 0)
			{
				continue;
			}
			flag = true;
			ExpandedGroupCommand expandedGroupCommand = new ExpandedGroupCommand(indexToUse);
			groupInfoToExpand.expandedGroupCommands.Add(expandedGroupCommand);
			List<string> list = new List<string>();
			if (!string.IsNullOrEmpty(enemyGroupSchema.enemy_1))
			{
				if (!string.IsNullOrEmpty(enemyGroupSchema.enemy_1) && !enemyTypesInUse.Contains(enemyGroupSchema.enemy_1))
				{
					enemyTypesInUse.Add(enemyGroupSchema.enemy_1);
				}
				int num4 = Mathf.Max(1, enemyGroupSchema.quantity_1);
				for (int j = 0; j < num4; j++)
				{
					list.Add(enemyGroupSchema.enemy_1);
				}
			}
			if (!string.IsNullOrEmpty(enemyGroupSchema.enemy_2))
			{
				if (!string.IsNullOrEmpty(enemyGroupSchema.enemy_2) && !enemyTypesInUse.Contains(enemyGroupSchema.enemy_2))
				{
					enemyTypesInUse.Add(enemyGroupSchema.enemy_2);
				}
				int num5 = Mathf.Max(1, enemyGroupSchema.quantity_2);
				for (int k = 0; k < num5; k++)
				{
					list.Add(enemyGroupSchema.enemy_2);
				}
			}
			if (!string.IsNullOrEmpty(enemyGroupSchema.enemy_3))
			{
				if (!string.IsNullOrEmpty(enemyGroupSchema.enemy_3) && !enemyTypesInUse.Contains(enemyGroupSchema.enemy_3))
				{
					enemyTypesInUse.Add(enemyGroupSchema.enemy_3);
				}
				int num6 = Mathf.Max(1, enemyGroupSchema.quantity_3);
				for (int l = 0; l < num6; l++)
				{
					list.Add(enemyGroupSchema.enemy_3);
				}
			}
			int count = list.Count;
			for (int m = 0; m < count; m++)
			{
				int num7 = randomizer.NextRange(0, list.Count - 1, schema);
				string text = list[num7];
				WaveCommandSchema waveCommandSchema = new WaveCommandSchema();
				waveCommandSchema.enemy = text;
				expandedGroupCommand.expandedRawCommands.Add(waveCommandSchema);
				waveCommandSchema = new WaveCommandSchema();
				waveCommandSchema.command = enemyGroupSchema.minWaitTime + ", " + enemyGroupSchema.maxWaitTime;
				expandedGroupCommand.expandedRawCommands.Add(waveCommandSchema);
				list.RemoveAt(num7);
			}
			break;
		}
	}

	private static WaveSchema Initialize(WaveSchema schema, SeededRandomizer randomizer, List<EnemyGroupInfo> enemyGroupInfos)
	{
		List<string> list = new List<string>(kMaxEnemyTypes);
		if (schema != null)
		{
			schema.Rewards = schema.rewards.InitializeRecord<WaveRewardSchema>();
			WaveCommandSchema[] array = schema.commands.InitializeRecords<WaveCommandSchema>();
			List<WaveCommandSchema> list2 = new List<WaveCommandSchema>(array.Length);
			foreach (WaveCommandSchema nextPreRandom in array)
			{
				if (!string.IsNullOrEmpty(nextPreRandom.enemyGroup))
				{
					EnemyGroupInfo enemyGroupInfo = enemyGroupInfos.Find((EnemyGroupInfo info) => info.enemyGroupTable.RecordTable == nextPreRandom.enemyGroup.RecordTable);
					if (enemyGroupInfo == null)
					{
						enemyGroupInfo = enemyGroupInfos[randomizer.NextRange(0, enemyGroupInfos.Count - 1, schema)];
					}
					if (enemyGroupInfo != null && enemyGroupInfo.expandedGroupCommands.Count > 0)
					{
						int num = randomizer.NextRange(0, enemyGroupInfo.expandedGroupCommands.Count - 1, schema);
						list2.AddRange(enemyGroupInfo.expandedGroupCommands[num].expandedRawCommands);
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(nextPreRandom.enemy) && !list.Contains(nextPreRandom.enemy))
					{
						list.Add(nextPreRandom.enemy);
					}
					list2.Add(nextPreRandom);
				}
				schema.Commands = list2.ToArray();
			}
		}
		return schema;
	}

	private static int Count(string tableName)
	{
		if (DataBundleRuntime.Instance != null)
		{
			return DataBundleRuntime.Instance.GetRecordTableLength(typeof(WaveSchema), tableName);
		}
		return 0;
	}

	public static string FromIndex(string tableName, int index)
	{
		if (DataBundleRuntime.Instance != null)
		{
			return DataBundleRuntime.Instance.GetRecordKeys(typeof(WaveSchema), tableName, false)[index];
		}
		return string.Empty;
	}

	public static int PickRandomRecord(string tableName)
	{
		return Random.Range(0, Count(tableName));
	}
}
