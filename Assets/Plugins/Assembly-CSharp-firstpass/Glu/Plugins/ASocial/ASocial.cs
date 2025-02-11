using System;
using System.Collections.Generic;
using UnityEngine;

namespace Glu.Plugins.ASocial
{
	internal class ASocial
	{
		private static bool isInitialized;

		public static void Init()
		{
			if (!isInitialized)
			{
				isInitialized = true;
				ASocial_Init(Debug.isDebugBuild);
			}
		}

		private static void ASocial_Init(bool debug)
		{
		}
	}
}
