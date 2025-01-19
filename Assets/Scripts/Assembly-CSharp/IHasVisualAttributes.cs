using UnityEngine;

public interface IHasVisualAttributes
{
	float alpha { get; set; }

	Vector2 position { get; set; }

	float priority { get; set; }

	Vector2 scale { get; set; }

	bool visible { get; set; }
}
