using System;
using UnityEngine;

namespace Glu.Plugins.ASocial
{
	[Serializable]
	internal struct GluVector4
	{
		public float w;

		public float x;

		public float y;

		public float z;

		public GluVector4(Vector4 v)
		{
			w = v.w;
			x = v.x;
			y = v.y;
			z = v.z;
		}
	}
}
