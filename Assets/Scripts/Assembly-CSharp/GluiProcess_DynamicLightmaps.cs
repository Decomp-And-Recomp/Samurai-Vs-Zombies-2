using System;
using UnityEngine;

[AddComponentMenu("Glui Process/Process Dynamic Lightmaps")]
public class GluiProcess_DynamicLightmaps : GluiProcessStartAndStop
{
	public Texture2D[] lightmapsToLoad;

	private bool loaded;

	public override bool ProcessStart(GluiStatePhase phase)
	{
		switch (phase)
		{
		case GluiStatePhase.Init:
			DoPhaseInit();
			break;
		case GluiStatePhase.Exit:
			DoPhaseExit();
			break;
		}
		return false;
	}

	private void DoPhaseInit()
	{
		LightmapData[] array = LightmapSettings.lightmaps;
		int num = array.Length;
		Array.Resize(ref array, array.Length + lightmapsToLoad.Length);
		for (int i = 0; i < lightmapsToLoad.Length; i++)
		{
			LightmapData lightmapData = new LightmapData();
			lightmapData.lightmapDir = lightmapsToLoad[i];
			lightmapData.lightmapColor = lightmapsToLoad[i];
			array[num + i] = lightmapData;
		}
		LightmapSettings.lightmaps = array;
		loaded = true;
	}

	private void DoPhaseExit()
	{
		if (!loaded)
		{
			return;
		}
		LightmapData[] array = LightmapSettings.lightmaps;
		Array.Resize(ref array, array.Length - lightmapsToLoad.Length);
		int num = 0;
		LightmapData[] lightmaps = LightmapSettings.lightmaps;
		foreach (LightmapData lightmapData in lightmaps)
		{
			if (!LightmapWasLoaded(lightmapData))
			{
				array[num] = lightmapData;
				num++;
			}
		}
		LightmapSettings.lightmaps = array;
		loaded = false;
	}

	private bool LightmapWasLoaded(LightmapData lightmap)
	{
		Texture2D[] array = lightmapsToLoad;
		foreach (Texture2D texture2D in array)
		{
			if (texture2D == lightmap.lightmapDir)
			{
				return true;
			}
		}
		return false;
	}

	public override void ProcessInterrupt()
	{
		base.ProcessInterrupt();
		DoPhaseExit();
	}
}
