using System;
using UnityEngine;

namespace Glu.Plugins.ASocial
{
	[Serializable]
	internal struct GluVector3
	{
		public float x;

		public float y;

		public float z;

		public GluVector3(Vector3 v)
		{
			x = v.x;
			y = v.y;
			z = v.z;
		}
	}
}
