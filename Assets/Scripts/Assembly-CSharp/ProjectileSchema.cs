using UnityEngine;

[DataBundleClass(Category = "Design")]
public class ProjectileSchema
{
	[DataBundleKey(Schema = typeof(DynamicEnum), Table = "Projectile", ColumnWidth = 200)]
	public DataBundleRecordKey id;

	[DataBundleField(StaticResource = true, ColumnWidth = 200)]
	public GameObject prefab;

	[DataBundleField(StaticResource = true, ColumnWidth = 200)]
	public Material material;

	public bool arcs;

	public bool needsBothHands;

	public bool shownWhileAiming;

	[DataBundleSchemaFilter(typeof(USoundThemeSetSchema), false)]
	public DataBundleRecordKey soundTheme;

	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	[DataBundleRecordTableFilter("SoundThemeEnum")]
	public DataBundleRecordKey onLoadedSoundEvent;

	[DataBundleDefaultValue(1f)]
	public float velocityModifier;
}
