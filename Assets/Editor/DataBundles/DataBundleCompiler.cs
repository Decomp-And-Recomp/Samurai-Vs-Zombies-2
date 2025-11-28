using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Reflection;
using System;
using Gamespy.Common;
using System.Runtime.Serialization.Formatters.Binary;

public class DataBundleCompiler : MonoBehaviour
{
    [MenuItem("Data Bundles/Compile English")]
    public static void CompileEnglish()
    {
        CompileBundle(Path.Combine("DataBundles", "English"));
    }

    private static void AddString(List<string> list, string text)
    {
        if (!list.Contains(text))
        {
            list.Add(text);
        }
    }

    private class BundleClass
    {
        public string name;

        public Dictionary<string, Dictionary<string, List<DataBundle.BundleField>>> tables = new Dictionary<string, Dictionary<string, List<DataBundle.BundleField>>>();

        public BundleClass(string name)
        {
            this.name = name;
        }
    }

    private static void CompileBundle(string path)
    {
        string bundleName = path.Split('/', '\\').Last();

        List<string> cachedStrings = new List<string>
        {
            "!" + Application.unityVersion
        },
        cachedAssets = new List<string>();

        List<BundleClass> parsedClasses = new List<BundleClass>();

        string currentTable = "", currentKey = "";

        Type objectType = typeof(UnityEngine.Object),
        tableType = typeof(DataBundleRecordTable),
        keyType = typeof(DataBundleRecordKey),
        integerType = typeof(int),
		floatingType = typeof(float),
		stringType = typeof(string),
		booleanType = typeof(bool),
		enumType = typeof(Enum);

        foreach (string dataClass in Directory.GetFiles(path))
        {
            string typeName = Path.GetFileNameWithoutExtension(dataClass);
            AddString(cachedStrings, typeName);

            string[] typeContent = File.ReadAllLines(dataClass);
            BundleClass currentClass = new BundleClass(typeName);

            FieldInfo[] typeFields = DataBundle.schemaAssembly.GetType(typeName).GetFields();

            foreach (string line in typeContent)
            {
                int startingIndex = 0;

                while (char.IsWhiteSpace(line[startingIndex]))
                {
                    startingIndex++;
                }

                string skippedLine = line.Substring(startingIndex);

                if (string.IsNullOrEmpty(skippedLine))
                {
                    continue;
                }

                if (skippedLine.StartsWith("["))
                {
                    currentTable = skippedLine.Substring(1).Replace("]", "");
                    currentClass.tables.Add(currentTable, new Dictionary<string, List<DataBundle.BundleField>>());

                    AddString(cachedStrings, currentTable);
                }
                else if (skippedLine.EndsWith(":"))
                {
                    currentKey = skippedLine.Replace(":", "");
                    currentClass.tables[currentTable].Add(currentKey, new List<DataBundle.BundleField>());

                    AddString(cachedStrings, currentKey);
                }
                else
                {
                    int valueIndex = skippedLine.IndexOf(" = ");

                    string fieldName = skippedLine.Substring(0, valueIndex);
                    string fieldValue = skippedLine.Substring(valueIndex + 3);

                    FieldInfo fieldInfo = null;

                    for (int i = 0; i < typeFields.Length; i++)
                    {
                        if (typeFields[i].Name == fieldName)
                        {
                            fieldInfo = typeFields[i];
                        }
                    }

                    if (fieldInfo == null)
                    {
                        UnityEngine.Debug.LogError("fieldInfo null! " + fieldName);
                        continue;
                    }

                    object fieldAttribute = fieldInfo.GetCustomAttributes(typeof(DataBundleFieldAttribute), false).FirstOrDefault();

                    if (fieldAttribute != null)
                    {
                        if (((DataBundleFieldAttribute)fieldAttribute).Identifier != -1)
                        {
                            // this is terrible and I hate it
                            fieldName = ((DataBundleFieldAttribute)fieldAttribute).Identifier.ToString();
                        }
                    }
                    
                    object value = null;
                    AddString(cachedStrings, fieldName);

                    if (fieldValue != "NULL")
                    {
                        if (fieldInfo.FieldType.IsSubclassOf(objectType))
                        {
                            AddString(cachedAssets, fieldValue);
                            value = fieldValue;
                        }
                        else if (fieldInfo.FieldType == typeof(DataBundleRecordTable) || fieldInfo.FieldType == typeof(DataBundleRecordKey))
                        {
                            string[] recordHash = fieldValue.Split('.');

                            for (int i = 0; i < recordHash.Length; i++)
                            {
                                AddString(cachedStrings, recordHash[i]);
                            }

                            value = recordHash;
                        }
                        else if (fieldInfo.FieldType == integerType)
                        {
                            value = int.Parse(fieldValue);
                        }
                        else if (fieldInfo.FieldType == floatingType)
                        {
                            value = float.Parse(fieldValue);
                        }
                        else if (fieldInfo.FieldType == stringType)
                        {
                            value = fieldValue.Replace("_NEWLINE_", "\n");
                        }
                        else if (fieldInfo.FieldType == booleanType)
                        {
                            value = bool.Parse(fieldValue);
                        }
                        else if (fieldInfo.FieldType.IsSubclassOf(enumType))
                        {
                            value = Enum.Parse(fieldInfo.FieldType, fieldValue);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("type unknown! " + fieldInfo.FieldType.Name);
                        }
                    }

                    currentClass.tables[currentTable][currentKey].Add(new DataBundle.BundleField(fieldInfo, value));
                }
            }

            parsedClasses.Add(currentClass);
        }

        cachedStrings.Sort();
        cachedAssets.Sort();

        Dictionary<string, ushort> stringToIndex = new Dictionary<string, ushort>();
        Dictionary<string, int> assetToIndex = new Dictionary<string, int>();
        
        for (ushort i = 0; i < cachedStrings.Count; i++)
        {
            stringToIndex.Add(cachedStrings[i], i);
        }

        for (int i = 0; i < cachedAssets.Count; i++)
        {
            assetToIndex.Add(cachedAssets[i], i);
        }

        Hashtable hashtable = new Hashtable
        {
            {"UnityVersion", 0}
        };

        foreach (BundleClass parsedClass in parsedClasses)
        {
            List<long> tables = new List<long>();

            foreach (KeyValuePair<string, Dictionary<string, List<DataBundle.BundleField>>> table in parsedClass.tables)
            {
                List<long> keys = new List<long>();

                foreach (KeyValuePair<string, List<DataBundle.BundleField>> key in table.Value)
                {
                    keys.Add(new DataBundle.HashCode(0, 0, stringToIndex[key.Key], 0).ToHash());

                    foreach (DataBundle.BundleField field in key.Value)
                    {
                        object value = null;

                        if (field.value != null)
                        {
                            if (field.info.FieldType.IsSubclassOf(objectType))
                            {
                                string asset = (string)field.value;

                                if (string.IsNullOrEmpty(asset))
                                {
                                    value = -1;
                                }
                                else
                                {
                                    value = assetToIndex[asset];
                                }
                            }
                            else if (field.info.FieldType == tableType || field.info.FieldType == keyType)
                            {
                                ushort[] hash = new ushort[4];
                                string[] recordHash = (string[])field.value;

                                for (int i = 0; i < 4; i++)
                                {
                                    if (i >= recordHash.Length || string.IsNullOrEmpty(recordHash[i]))
                                    {
                                        hash[i] = 0;
                                    }
                                    else
                                    {
                                        hash[i] = stringToIndex[recordHash[i]];
                                    }
                                }

                                value = new DataBundle.HashCode(hash[0], hash[1], hash[2], hash[3]).ToHash();
                            }
                            else
                            {
                                value = field.value;
                            }
                        }

                        object fieldAttribute = field.info.GetCustomAttributes(typeof(DataBundleFieldAttribute), false).FirstOrDefault();
                        string fieldName = field.info.Name;

                        if (fieldAttribute != null)
                        {
                            if (((DataBundleFieldAttribute)fieldAttribute).Identifier != -1)
                            {
                                // this is terrible and I hate it
                                fieldName = ((DataBundleFieldAttribute)fieldAttribute).Identifier.ToString();
                            }
                        }

                        hashtable.Add(new DataBundle.HashCode(stringToIndex[parsedClass.name], stringToIndex[table.Key], stringToIndex[key.Key], stringToIndex[fieldName]).ToHash(), value);
                    }
                }

                tables.Add(new DataBundle.HashCode(0, stringToIndex[table.Key], 0, 0).ToHash());
                hashtable.Add(new DataBundle.HashCode(stringToIndex[parsedClass.name], stringToIndex[table.Key], 0, 0).ToHash(), keys);
            }

            hashtable.Add(new DataBundle.HashCode(stringToIndex[parsedClass.name], 0, 0, 0).ToHash(), tables);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        string outputPath = Path.Combine(Application.streamingAssetsPath, bundleName);

        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, cachedAssets);
            File.WriteAllBytes(Path.Combine(outputPath, "BundleAssetInfo.assetlist"), stream.ToArray());
        }

        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, hashtable);
            File.WriteAllBytes(Path.Combine(outputPath, "DataBundle.hashtable"), stream.ToArray());
        }

        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, cachedStrings);
            File.WriteAllBytes(Path.Combine(outputPath, "DataBundle.stringlist"), stream.ToArray());
        }
    }
}