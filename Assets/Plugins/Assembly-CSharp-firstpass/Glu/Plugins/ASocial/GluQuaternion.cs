using System;
using UnityEngine;

namespace Glu.Plugins.ASocial
{
	[Serializable]
	internal struct GluQuaternion
	{
		public float w;

		public float x;

		public float y;

		public float z;

		public GluQuaternion(Quaternion q)
		{
			w = q.w;
			x = q.x;
			y = q.y;
			z = q.z;
		}
	}
}
