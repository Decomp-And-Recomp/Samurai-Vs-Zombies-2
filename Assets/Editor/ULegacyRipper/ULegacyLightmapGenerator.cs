using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ULegacyRipper
{
	public static class ULegacyLightmapGenerator
	{
		public static void GenerateLightmap(string scenePath)
		{
			//you should probably make a yaml WRITER at some point yk
			ULegacyUtils.Debug("tf?");
			YAML yaml = YAMLReader.Read(File.ReadAllLines(scenePath));
			IndentedWriter writer = new IndentedWriter(2);

			writer.WriteLines("%YAML 1.1",
			"%TAG !u! tag:unity3d.com,2011:",
			"--- !u!1120 &112000000",
			"LightingDataAsset:");

			writer.indentationLevel++;

			writer.WriteLines("serializedVersion: 4",
			"m_ObjectHideFlags: 0",
			"m_PrefabParentObject: {fileID: 0}",
			"m_PrefabInternal: {fileID: 0}",
			"m_Name: LightingData",
			"m_Scene: {fileID: 102900000, guid: " + AssetDatabase.AssetPathToGUID(scenePath) + ", type: 3}",
			"m_Lightmaps:");

			YAMLObject lightmaps = yaml[yaml.Find("LightmapSettings")];

			for (int i = 0; i < lightmaps.ArrayCount("m_Lightmap"); i++)
			{
				writer.WriteLine("- serializedVersion: 2");
				writer.indentationLevel++;

				string originalPath = AssetDatabase.GUIDToAssetPath((string)lightmaps.ArrayValue("m_Lightmap", i)["guid"].value);

				Texture2D texture = new Texture2D(1, 1, TextureFormat.DXT5, false);
				texture.LoadImage(File.ReadAllBytes(originalPath));

				string newPath = AssetDatabase.GUIDToAssetPath((string)lightmaps.ArrayValue("m_Lightmap", i)["guid"].value).Replace(".png", ".asset");
				AssetDatabase.CreateAsset(texture, newPath);

				File.Delete(originalPath);

				writer.WriteLines("m_Lightmap: {fileID: 2800000, guid: " + AssetDatabase.AssetPathToGUID(AssetDatabase.GUIDToAssetPath((string)lightmaps.ArrayValue("m_Lightmap", i)["guid"].value).Replace(".png", ".asset")) + ", type: 2}",
				"m_DirLightmap: {fileID: 0}",
				"m_ShadowMask: {fileID: 0}");

				writer.indentationLevel--;
			}
			
			if (lightmaps["m_LightProbes"].HasObject("guid"))
			{
				writer.WriteLine("m_LightProbes: {fileID: " + lightmaps["m_LightProbes"]["fileID"] + ", guid: " + lightmaps["m_LightProbes"]["guid"] + ", type: 2}");
			}
			else
			{
				writer.WriteLine("m_LightProbes: {fileID: 0}");
			}

			writer.WriteLines("m_LightmapsMode: 0",
			"m_BakedAmbientProbeInLinear:");

			writer.indentationLevel++;

			for (int i = 0; i < 27; i++)
			{
				writer.WriteLine("sh[" + (i < 10 ? " " : "") + i + "]: 0");
			}

			writer.indentationLevel--;
			writer.WriteLine("m_LightmappedRendererData:");

			List<long> rendererIDs = new List<long>();
			
			foreach (KeyValuePair<long, YAMLObject> kvp in yaml)
			{
				if (kvp.Value.name.EndsWith("Renderer") && kvp.Value.HasObject("m_LightmapIndex") && ((long)kvp.Value["m_LightmapIndex"].value) < 255)
				{
					writer.WriteLine("- uvMesh: {fileID: 0}");
					writer.indentationLevel++;

					writer.WriteLines("terrainDynamicUVST: {x: 0, y: 0, z: 0, w: 0}",
					"terrainChunkDynamicUVST: {x: 0, y: 0, z: 0, w: 0}",
					"lightmapIndex: " + ((long)kvp.Value["m_LightmapIndex"].value).ToString(),
					"lightmapIndexDynamic: 65535",
					"lightmapST: {x: " + kvp.Value["m_LightmapTilingOffset"]["x"].TryGetFloat() + ", y: " + kvp.Value["m_LightmapTilingOffset"]["y"].TryGetFloat() + ", z: "
					+ kvp.Value["m_LightmapTilingOffset"]["z"].TryGetFloat() + ", w: " + kvp.Value["m_LightmapTilingOffset"]["w"].TryGetFloat() + "}",
					"lightmapSTDynamic: {x: 1, y: 1, z: 0, w: 0}");

					writer.indentationLevel--;
					rendererIDs.Add(kvp.Key);
				}
			}

			writer.WriteLine("m_LightmappedRendererDataIDs:");

			foreach (long id in rendererIDs)
			{
				writer.WriteLine("- targetObject: " + id);

				writer.indentationLevel++;
				writer.WriteLine("targetPrefab: 0");

				writer.indentationLevel--;
			}

			writer.WriteLine("m_EnlightenSceneMapping:");
			writer.indentationLevel++;

			writer.WriteLines("m_Renderers: []",
			"m_Systems: []",
			"m_Probesets: []",
			"m_SystemAtlases: []",
			"m_TerrainChunks: []");

			writer.indentationLevel--;
			writer.WriteLines("m_EnlightenSceneMappingRendererIDs: []",
			"m_Lights:");

			foreach (KeyValuePair<long, YAMLObject> kvp in yaml)
			{
				if (kvp.Value.name == "Light")
				{
					writer.WriteLine("- targetObject: " + kvp.Key);

					writer.indentationLevel++;
					writer.WriteLine("targetPrefab: 0");

					writer.indentationLevel--;
				}
			}

			//I really hope this isn't important because I have no clue
			writer.WriteLine("m_LightBakingOutputs: []");

			writer.WriteLines("m_BakedReflectionProbeCubemaps: []",
			"m_BakedReflectionProbes: []",
			"m_EnlightenData:",
			"m_EnlightenDataVersion: 112");	

			string lightingDataPath = Path.Combine(Path.Combine(Path.GetDirectoryName(scenePath), Path.GetFileNameWithoutExtension(scenePath)), "LightingData.asset");
			File.WriteAllText(lightingDataPath, writer.ToString());

			List<string> sceneLines = File.ReadAllLines(scenePath).ToList();
			sceneLines.Insert(sceneLines.IndexOf("LightmapSettings:") + 1, "  m_LightingDataAsset: {fileID: 112000000, guid: " + AssetDatabase.AssetPathToGUID(lightingDataPath) + ", type: 2}");

			File.WriteAllLines(scenePath, sceneLines.ToArray());
		}
	}
}