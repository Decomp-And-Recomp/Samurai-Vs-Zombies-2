using UnityEngine;

public class ExplosionForce : MonoBehaviour
{
	public float Radius = 500f;

	public float ExplosionPower = 100000f;

	public float UpwardsModifier = 3000f;

	public float DebrisMinLifeTime = 3f;

	public float DebrisMaxLifeTime = 5f;

	private void Start()
	{
		Vector3 position = base.transform.position;
		Collider[] array = Physics.OverlapSphere(position, Radius);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if ((bool)collider && (bool)collider.GetComponent<Rigidbody>())
			{
				collider.GetComponent<Rigidbody>().AddExplosionForce(ExplosionPower, position, Radius, UpwardsModifier);
				float num = Random.Range(DebrisMinLifeTime, DebrisMaxLifeTime);
				Object.Destroy(collider, num);
				Object.Destroy(collider.gameObject, num + 1f);
			}
		}
		Object.Destroy(base.gameObject, DebrisMaxLifeTime + 2f);
	}
}
