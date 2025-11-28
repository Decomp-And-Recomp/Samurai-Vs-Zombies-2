using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System;

public class DataBundleDumper
{
    [MenuItem("Data Bundles/Dump All")]
	public static void DumpBundles()
    {
        foreach (string path in Directory.GetDirectories(Application.streamingAssetsPath))
        {
            DumpBundle(path);
        }

        EditorUtility.ClearProgressBar();
    }

    private static void DumpBundle(string path)
    {
        string bundleName = path.Split('/', '\\').Last();

        EditorUtility.DisplayProgressBar("Dumping " + bundleName, "Reading bundle", 0f);
        
        byte[] assetListBytes = File.ReadAllBytes(Path.Combine(path, "BundleAssetInfo.assetlist")),
        dataTableBytes = File.ReadAllBytes(Path.Combine(path, "DataBundle.hashtable")),
		stringListBytes = File.ReadAllBytes(Path.Combine(path, "DataBundle.stringlist"));

        Hashtable hashtable;
        List<string> stringList, assetList;

        BinaryFormatter formatter = new BinaryFormatter();

        using (MemoryStream stream = new MemoryStream(assetListBytes))
		{
			assetList = (List<string>)formatter.Deserialize(stream);
		}

        using (MemoryStream stream = new MemoryStream(dataTableBytes))
		{
			hashtable = (Hashtable)formatter.Deserialize(stream);
		}

		using (MemoryStream stream = new MemoryStream(stringListBytes))
		{
			stringList = (List<string>)formatter.Deserialize(stream);
		}

        EditorUtility.DisplayProgressBar("Dumping " + bundleName, "Parsing bundle", 0.05f);

        // this is unnecessary, since I've removed the unity check
        // (and there's no reason for it to exist)
        hashtable.Remove("UnityVersion");

        List<ushort> types = new List<ushort>();
        List<DataBundle.BundleClass> parsedClasses = new List<DataBundle.BundleClass>();

        foreach (DictionaryEntry entry in hashtable)
        {
            DataBundle.HashCode hash = new DataBundle.HashCode((long)entry.Key);

            if (!types.Contains(hash.type))
            {
                types.Add(hash.type);
                DataBundle.BundleClass bundleClass = new DataBundle.BundleClass(hash.type);

                DataBundle.HashCode typeHash = new DataBundle.HashCode(hash.type, 0, 0, 0);

                foreach (long table in (List<long>)hashtable[typeHash.ToHash()])
                {
                    DataBundle.HashCode tableHash = new DataBundle.HashCode(table);
                    tableHash = new DataBundle.HashCode(hash.type, tableHash.table, 0, 0);

                    bundleClass.tables.Add(tableHash.table, new Dictionary<ushort, List<DataBundle.BundleField>>());
                    
                    foreach (long key in (List<long>)hashtable[tableHash.ToHash()])
                    {
                        DataBundle.HashCode keyHash = new DataBundle.HashCode(key);
                        bundleClass.tables[tableHash.table].Add(keyHash.key, new List<DataBundle.BundleField>());
                    }
                }

                parsedClasses.Add(bundleClass);
            }
        }

        int classIndex = 0;

        foreach (DataBundle.BundleClass parsedClass in parsedClasses)
        {
            float progress = classIndex == 0 ? 0f : (classIndex / (float)parsedClasses.Count * 0.75f);
            classIndex++;

            EditorUtility.DisplayProgressBar("Dumping " + bundleName, "De-Hashing " + stringList[parsedClass.name], 0.05f + progress);

            Type schemaType = DataBundle.schemaAssembly.GetType(stringList[parsedClass.name]);

            FieldInfo[] schemaFields = schemaType.GetFields();
            string[] schemaNames = new string[schemaFields.Length];

            for (int i = 0; i < schemaFields.Length; i++)
            {
                object fieldAttribute = schemaFields[i].GetCustomAttributes(typeof(DataBundleFieldAttribute), false).FirstOrDefault();

                if (fieldAttribute != null)
                {
                    if (((DataBundleFieldAttribute)fieldAttribute).Identifier != -1)
                    {
                        // this is terrible and I hate it
                        schemaNames[i] = ((DataBundleFieldAttribute)fieldAttribute).Identifier.ToString();
                        continue;
                    }
                }

                schemaNames[i] = schemaFields[i].Name;
            }

            hashtable.Remove(new DataBundle.HashCode(parsedClass.name, 0, 0, 0).ToHash());

            foreach (KeyValuePair<ushort, Dictionary<ushort, List<DataBundle.BundleField>>> table in parsedClass.tables)
            {
                hashtable.Remove(new DataBundle.HashCode(parsedClass.name, table.Key, 0, 0).ToHash());

                foreach (KeyValuePair<ushort, List<DataBundle.BundleField>> key in table.Value)
                {
                    for (int i = 0; i < schemaFields.Length; i++)
                    {
                        long hash = new DataBundle.HashCode(parsedClass.name, table.Key, key.Key, (ushort)stringList.IndexOf(schemaNames[i])).ToHash();

                        if (hashtable.ContainsKey(hash))
                        {
                            key.Value.Add(new DataBundle.BundleField(schemaFields[i], hashtable[hash]));
                            hashtable.Remove(hash);
                        }
                    }
                }
            }
        }

        classIndex = 0;

        Directory.CreateDirectory("DataBundles");
        Directory.CreateDirectory(Path.Combine("DataBundles", bundleName));

        Type objectType = typeof(UnityEngine.Object),
        tableType = typeof(DataBundleRecordTable),
        keyType = typeof(DataBundleRecordKey),
        stringType = typeof(string);

        foreach (DataBundle.BundleClass parsedClass in parsedClasses)
        {
            float progress = classIndex == 0 ? 0f : (classIndex / (float)parsedClasses.Count * 0.2f);
            classIndex++;

            EditorUtility.DisplayProgressBar("Dumping " + bundleName, "Writing " + stringList[parsedClass.name], 0.8f + progress);

            using (StreamWriter writer = new StreamWriter(Path.Combine(Path.Combine("DataBundles", bundleName), stringList[parsedClass.name] + ".txt")))
            {
                foreach (KeyValuePair<ushort, Dictionary<ushort, List<DataBundle.BundleField>>> table in parsedClass.tables)
                {
                    writer.WriteLine("[" + stringList[table.Key] + "]");

                    foreach (KeyValuePair<ushort, List<DataBundle.BundleField>> key in table.Value)
                    {
                        writer.WriteLine("  " + stringList[key.Key] + ":");

                        foreach (DataBundle.BundleField field in key.Value)
                        {
                            string value = "NULL";

                            if (field.value != null)
                            {   
                                if (field.info.FieldType.IsSubclassOf(objectType))
                                {
                                    int index = (int)field.value;

                                    if (index == -1)
                                    {
                                        value = string.Empty;
                                    }
                                    else
                                    {
                                        value = assetList[(int)field.value];
                                    }
                                }
                                else if (field.info.FieldType == tableType || field.info.FieldType == keyType)
                                {
                                    value = new DataBundle.HashCode((long)field.value).ToString(stringList);
                                }
                                else if (field.info.FieldType == stringType)
                                {
                                    value = ((string)field.value).Replace("\n", "_NEWLINE_");
                                }
                                else
                                {
                                    value = field.value.ToString();
                                }
                            }

                            writer.WriteLine("    " + field.info.Name + " = " + value);
                        }
                    }
                }
            }
        }
    }
}