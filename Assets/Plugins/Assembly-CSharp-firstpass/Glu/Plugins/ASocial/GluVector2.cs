using System;
using UnityEngine;

namespace Glu.Plugins.ASocial
{
	[Serializable]
	internal struct GluVector2
	{
		public float x;

		public float y;

		public GluVector2(Vector2 v)
		{
			x = v.x;
			y = v.y;
		}
	}
}
