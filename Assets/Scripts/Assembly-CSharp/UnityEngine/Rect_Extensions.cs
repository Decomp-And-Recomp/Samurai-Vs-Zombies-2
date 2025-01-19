namespace UnityEngine
{
	public static class Rect_Extensions
	{
		public static bool Intersects(this Rect a, Rect b)
		{
			return ((a.xMin >= b.xMin && a.xMin <= b.xMax) || (a.xMax >= b.xMin && a.xMax <= b.xMax) || (b.xMin >= a.xMin && b.xMin <= a.xMax) || (b.xMax >= a.xMin && b.xMax <= a.xMax)) && ((a.yMin >= b.yMin && a.yMin <= b.yMax) || (a.yMax >= b.yMin && a.yMax <= b.yMax) || (b.yMin >= a.yMin && b.yMin <= a.yMax) || (b.yMax >= a.yMin && b.yMax <= a.yMax));
		}

		public static Rect DisplacedBy(this Rect a, Vector2 delta)
		{
			return new Rect(a.xMin + delta.x, a.yMin + delta.y, a.xMax + delta.x, a.yMax + delta.y);
		}
	}
}
