using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace ULegacyRipper
{
	public static class ULegacyLightmapGenerator
	{
		public static void GenerateLightmap(string scenePath)
		{
			YAML yaml = YAMLReader.ReadYAML(File.ReadAllLines(scenePath));

			YAML lightingDataYaml = new YAML();
			YAMLNode lightingDataAsset = new YAMLNode
			{
				name = "LightingDataAsset",
				type = 1120
			};

			lightingDataAsset.AddNode("serializedVersion", "4");
			lightingDataAsset.AddNode("m_ObjectHideFlags", "0");

			lightingDataAsset.AddEmptyFileIDNode("m_PrefabParentObject");
			lightingDataAsset.AddEmptyFileIDNode("m_PrefabInternal");

			lightingDataAsset.AddNode("m_Name", "LightingData");

			YAMLNode sceneNode = lightingDataAsset.AddNode("m_Scene");

			sceneNode.table.Add("fileID", "102900000");
			sceneNode.table.Add("guid", AssetDatabase.AssetPathToGUID(scenePath));
			sceneNode.table.Add("type", "3");

			YAMLNode lightmapsNode = lightingDataAsset.AddNode("m_Lightmaps");
			YAMLNode oldLightmaps = yaml.FindNode("LightmapSettings");

			foreach (YAMLNode lightmapNode in oldLightmaps.childNodes["m_Lightmaps"].nodeList)
			{
				YAMLNode entry = new YAMLNode
				{
					name = "serializedVersion",
					value = "2"
				};

				YAMLNode newLightmapNode = entry.AddNode("m_Lightmap");

				string originalPath = AssetDatabase.GUIDToAssetPath(lightmapNode.table["guid"]);

				Texture2D texture = new Texture2D(1, 1, TextureFormat.DXT1, false);
				texture.LoadImage(File.ReadAllBytes(originalPath));

				string newPath = AssetDatabase.GUIDToAssetPath(lightmapNode.table["guid"]).Replace(".png", ".asset");
				AssetDatabase.CreateAsset(texture, newPath);

				File.Delete(originalPath);
				File.WriteAllText(newPath, File.ReadAllText(newPath).Replace("m_LightmapFormat: 0", "m_LightmapFormat: 1"));

				newLightmapNode.table.Add("fileID","2800000");
				newLightmapNode.table.Add("type", "2");
				newLightmapNode.table.Add("guid", AssetDatabase.AssetPathToGUID(AssetDatabase.GUIDToAssetPath(lightmapNode.table["guid"]).Replace(".png", ".asset")));
				
				entry.AddEmptyFileIDNode("m_DirLightmap");
				entry.AddEmptyFileIDNode("m_ShadowMask");

				lightmapsNode.nodeList.Add(entry);
			}

			lightingDataAsset.AddNode("m_LightProbes").CopyTable(oldLightmaps.childNodes["m_LightProbes"]);

			List<Vector3> probePositions = null;

			if (lightingDataAsset.childNodes["m_LightProbes"].table.ContainsKey("guid"))
			{
				probePositions = UpgradeLightProbes(AssetDatabase.GUIDToAssetPath(lightingDataAsset.childNodes["m_LightProbes"].table["guid"]));
			}

			lightingDataAsset.AddNode("m_LightmapsMode", "0");
			YAMLNode bakedAmbientProbe = lightingDataAsset.AddNode("m_BakedAmbientProbeInLinear");

			for (int i = 0; i < 27; i++)
			{
				bakedAmbientProbe.AddNode("sh[" + (i < 10 ? " " : "") + i + "]", "0");
			}

			YAMLNode lightmapRendererData = lightingDataAsset.AddNode("m_LightmappedRendererData");
			List<long> rendererIDs = new List<long>();

			foreach (KeyValuePair<long, YAMLNode> node in yaml.nodes)
			{
				if (node.Value.name.EndsWith("Renderer") && node.Value.childNodes.ContainsKey("m_LightmapIndex") && long.Parse(node.Value.childNodes["m_LightmapIndex"].value) < 255)
				{
					YAMLNode uvMesh = new YAMLNode()
					{
						name = "uvMesh"
					};

					uvMesh.table.Add("fileID", "0");

					uvMesh.AddEmptyVector4("terrainDynamicUVST");
					uvMesh.AddEmptyVector4("terrainChunkDynamicUVST");
				
					uvMesh.AddNode("lightmapIndex", node.Value.childNodes["m_LightmapIndex"].value);
					uvMesh.AddNode("lightmapIndexDynamic", "65535");
					
					uvMesh.AddNode("lightmapST").CopyTable(node.Value.childNodes["m_LightmapTilingOffset"]);
					uvMesh.AddEmptyVector4ST("lightmapSTDynamic");

					lightmapRendererData.nodeList.Add(uvMesh);
					rendererIDs.Add(node.Key);
				}
			}

			YAMLNode lightmapRendererDataIDs = lightingDataAsset.AddNode("m_LightmappedRendererDataIDs");

			foreach (long id in rendererIDs)
			{
				YAMLNode targetObject = new YAMLNode
				{
					name = "targetObject",
					value = id.ToString()
				};

				targetObject.AddNode("targetPrefab", "0");
				lightmapRendererDataIDs.nodeList.Add(targetObject);
			}

			YAMLNode enlightenMapping = lightingDataAsset.AddNode("m_EnlightenSceneMapping");

			enlightenMapping.AddNode("m_Renderers", "[]");
			enlightenMapping.AddNode("m_Systems", "[]");
			enlightenMapping.AddNode("m_Probesets", "[]"); //worried probesets is important and I have no idea what they do or how to generate them
			enlightenMapping.AddNode("m_SystemAtlases", "[]");
			enlightenMapping.AddNode("m_TerrainChunks", "[]");

			lightingDataAsset.AddNode("m_EnlightenSceneMappingRendererIDs", "[]");

			YAMLNode lights = lightingDataAsset.AddNode("m_Lights");
			
			foreach (KeyValuePair<long, YAMLNode> node in yaml.nodes)
			{
				if (node.Value.name == "Light")
				{
					YAMLNode targetObject = new YAMLNode
					{
						name = "targetObject",
						value = node.Key.ToString()
					};

					targetObject.AddNode("targetPrefab", "0");
					lights.nodeList.Add(targetObject);
				}
			}

			lightingDataAsset.AddNode("m_LightBakingOutputs", "[]");
			lightingDataAsset.AddNode("m_BakedReflectionProbeCubemaps", "[]");
			lightingDataAsset.AddNode("m_BakedReflectionProbes", "[]");

			lightingDataAsset.AddNode("m_EnlightenData");
			lightingDataAsset.AddNode("m_EnlightenDataVersion", "0");


			lightingDataYaml.nodes.Add(112000000, lightingDataAsset);

			string lightingDataPath = Path.Combine(Path.Combine(Path.GetDirectoryName(scenePath), Path.GetFileNameWithoutExtension(scenePath)), "LightingData.asset");
			File.WriteAllText(lightingDataPath, YAMLWriter.WriteYAML(lightingDataYaml));

			AssetDatabase.Refresh();

			YAMLNode addedLightingData = oldLightmaps.AddNode("m_LightingDataAsset");

			addedLightingData.table.Add("fileID", "112000000");
			addedLightingData.table.Add("guid", AssetDatabase.AssetPathToGUID(lightingDataPath));
			addedLightingData.table.Add("type", "2");

			if (probePositions != null)
			{
				YAMLNode lightProbeGroup = yaml.FindNode("LightProbeGroup");

				if (lightProbeGroup != null)
				{
					Vector3 offset = Vector3.zero;
					YAMLNode lightProbeComponents = yaml.nodes[long.Parse(lightProbeGroup.childNodes["m_GameObject"].table["fileID"])].childNodes["m_Component"];

					foreach (YAMLNode childNode in lightProbeComponents.nodeList)
					{
						if (childNode.name == "4")
						{
							long currentID = long.Parse(childNode.table["fileID"]);

							while (true)
							{
								YAMLNode transform = yaml.nodes[currentID];
								YAMLNode position = transform.childNodes["m_LocalPosition"];

								offset -= new Vector3(float.Parse(position.table["x"]), float.Parse(position.table["y"]), float.Parse(position.table["z"]));

								if (transform.childNodes["m_Father"].table["fileID"] == "0")
								{
									break;
								}

								currentID = long.Parse(transform.childNodes["m_Father"].table["fileID"]);
							}
						}
					}

					lightProbeGroup.value = "";
					YAMLNode positions = lightProbeGroup.childNodes["m_SourcePositions"];

					positions.value = "";

					foreach (Vector3 position in probePositions)
					{
						Vector3 offsetPosition = position + offset;
						YAMLNode probePosition = new YAMLNode();

						probePosition.table.Add("x", offsetPosition.x.ToString());
						probePosition.table.Add("y", offsetPosition.y.ToString());
						probePosition.table.Add("z", offsetPosition.z.ToString());

						positions.nodeList.Add(probePosition);
					}
				}
			}

			File.WriteAllText(scenePath, YAMLWriter.WriteYAML(yaml));
		}

		private static List<Vector3> UpgradeLightProbes(string probePath)
		{
			YAML probeYaml = YAMLReader.ReadYAML(File.ReadAllLines(probePath));

			YAML newProbeYaml = new YAML();
			YAMLNode lightProbes = new YAMLNode
			{
				name = "LightProbes",
				type = 258
			};

			lightProbes.AddNode("m_ObjectHideFlags", "0");

			lightProbes.AddEmptyFileIDNode("m_PrefabParentObject");
			lightProbes.AddEmptyFileIDNode("m_PrefabInternal");

			lightProbes.AddNode("m_Name", "LightingData"); //internally named LightingData??
			
			YAMLNode data = lightProbes.AddNode("m_Data");
			YAMLNode tetrahedralization = data.AddNode("m_Tetrahedralization");

			YAMLNode oldProbes = probeYaml.FindNode("LightProbes");
			
			tetrahedralization.CopyNode("m_Tetrahedra", oldProbes.childNodes["tetrahedra"]);
			tetrahedralization.CopyNode("m_HullRays", oldProbes.childNodes["hullRays"]);

			YAMLNode probeSets = data.AddNode("m_ProbeSets");

			YAMLNode hash = new YAMLNode
			{
				name = "m_Hash"
			};

			hash.AddNode("serializedVersion", "2");

			byte[] randomBytes = new byte[16];
			new System.Random().NextBytes(randomBytes);

			hash.AddNode("Hash", BitConverter.ToString(randomBytes).Replace("-", "").ToLower()); //it's probably random? I don't know

			probeSets.nodeList.Add(hash);

			probeSets.AddNode("m_Offset", "0");
			probeSets.AddNode("m_Size", oldProbes.childNodes["bakedPositions"].nodeList.Count.ToString()); 

			data.CopyNode("m_Positions", oldProbes.childNodes["bakedPositions"]);
			List<Vector3> positions = new List<Vector3>();

			foreach (YAMLNode bakedPosition in oldProbes.childNodes["bakedPositions"].nodeList)
			{
				positions.Add(new Vector3(float.Parse(bakedPosition.table["x"]), float.Parse(bakedPosition.table["y"]), float.Parse(bakedPosition.table["z"])));
			}

			data.AddNode("m_NonTetrahedralizedProbeSetIndexMap", "{}");
			YAMLNode bakedCoefficients = lightProbes.AddNode("m_BakedCoefficients");

			foreach (YAMLNode bakedCoefficient in oldProbes.childNodes["bakedCoefficients"].nodeList)
			{
				YAMLNode coefficient = new YAMLNode
				{
					name = "sh[ 0]",
					value = bakedCoefficient.value
				};

				for (int i = 1; i < 27; i++)
				{
					coefficient.AddNode("sh[ " + i + "]", bakedCoefficient.childNodes["sh[" + i + "]"].value);
				}

				bakedCoefficients.nodeList.Add(coefficient);
			}

			lightProbes.AddNode("m_BakedLightOcclusion", "[]"); //I have no clue
			newProbeYaml.nodes.Add(25800000, lightProbes);

			File.WriteAllText(probePath, YAMLWriter.WriteYAML(newProbeYaml));

			return positions;
		}
	}
}