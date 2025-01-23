using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ULegacyRipper
{
    public class YAML : IEnumerable<KeyValuePair<long, YAMLObject>>
    {
        public Dictionary<long, YAMLObject> objects = new Dictionary<long, YAMLObject>();

        public YAMLObject this[long tag]
        {
            get
            {
                return objects[tag];
            }
            set
            {
                if (!objects.ContainsKey(tag))
                {
                    objects.Add(tag, value);
                }
                else
                {
                    objects[tag] = value;
                }
            }
        }

        public long Find(string key)
        {
            foreach (KeyValuePair<long, YAMLObject> kvp in objects)
            {
                if (kvp.Value.name == key)
                {
                    return kvp.Key;
                }
            }

            return 0;
        }

        public IEnumerator<KeyValuePair<long, YAMLObject>> GetEnumerator()
        {
            return objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class YAMLObject : IEnumerable<KeyValuePair<string, YAMLObject>>
    {
        public string name;

        public object value;

        public Dictionary<string, YAMLObject> values = new Dictionary<string, YAMLObject>();

        private Dictionary<string, int> arrayCounts = new Dictionary<string, int>();

        public YAMLObject this[string key]
        {
            get
            {
                return values[key];
            }
            set
            {
                if (!values.ContainsKey(key))
                {
                    values.Add(key, value);
                }
                else
                {
                    values[key] = value;
                }
            }
        }

        public float TryGetFloat()
        {
            if (value.GetType() == typeof(string))
            {
                throw new Exception("a string is not a float");
            }
            else if (value.GetType() == typeof(long))
            {
                return (long)value;
            }

            return (float)value;
        }

        public bool HasObject(string name)
        {
            return values.ContainsKey(name);
        }

        public int ArrayCount(string key)
        {
            if (!arrayCounts.ContainsKey("ARR_" + key))
            {
                return 0;
            }

            return arrayCounts["ARR_" + key] + 1;
        }

        public YAMLObject ArrayValue(string key, int index)
        {
            return this["ARR_" + key + "_" + index];
        }

        public void ArrayAdd(YAMLObject obj, string name)
        {
            if (!arrayCounts.ContainsKey(name))
            {
                arrayCounts.Add(name, 0);
            }
            else
            {
                arrayCounts[name]++;
            }
            
            this[name + "_" + arrayCounts[name]] = obj;
        }

        public IEnumerator<KeyValuePair<string, YAMLObject>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static class YAMLReader
    {
        public static YAML Read(string[] content)
        {
            IndentedReader reader = new IndentedReader(content);
            YAML yaml = new YAML();

            while (!reader.IsAtEnd())
            {
                string line = reader.ReadLine();

                if (line.StartsWith("---"))
                {
                    yaml[long.Parse(line.Split(' ')[2].Substring(1))] = ReadYAMLObject(reader);
                }
            }

            return yaml;
        }

        private static YAMLObject ReadYAMLObject(IndentedReader reader)
        {
            string line = reader.ReadLine();
            List<string> values = line.Split(new string[1] { ": " }, System.StringSplitOptions.None).ToList();

            if (!line.Contains(" "))
            {
                values = new List<string> { line.Replace(":", "") };
            }

            bool isArray = values[0].StartsWith("-");

            if (isArray)
            {
                values[0] = values[0].Substring(2);
            }

            YAMLObject yamlObject = new YAMLObject
            {
                name = ((isArray ? "ARR_" : "") + values[0]).Replace("{", "")
            };

            if (reader.LastIndentationIsLess() || reader.NextLine().StartsWith("- "))
            {
                do
                {
                    YAMLObject childObject = ReadYAMLObject(reader);

                    if (childObject.name.StartsWith("ARR_"))
                    {
                        yamlObject.ArrayAdd(childObject, childObject.name);
                    }
                    else
                    {
                        yamlObject[childObject.name] = childObject;
                    }
                }
                while (reader.LastIndentationIsEqual());
            }

            if (values.Count > 1)
            {
                if (values[1].StartsWith("{"))
                {
                    while (true)
                    {
                        string[] objectValues = line.Substring(values[0].Length + 3).Split(new string[1] { ", " }, System.StringSplitOptions.None);

                        for (int i = 0; i < objectValues.Length; i++)
                        {
                            if (objectValues[i].Replace("}", "") == "")
                            {
                                //idk why this would happen but it did so
                                continue;
                            }

                            string[] objectKeyValue = objectValues[i].Replace("}", "").Split(new string[1] { ": " }, System.StringSplitOptions.None);

                            if (objectKeyValue.Length < 2)
                            {
                                //IDK WHY THIS IS HAPPENING
                                continue;
                            }
                            
                            YAMLObject childObject = new YAMLObject
                            {
                                name = objectKeyValue[0],
                                value = StringToYamlObject(objectKeyValue[1])
                            };

                            yamlObject[childObject.name] = childObject;
                        }

                        if (!line.Contains("}"))
                        {
                            line = reader.ReadLine();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    yamlObject.value = StringToYamlObject(values[1]);
                }
            }

            return yamlObject;
        }

        private static object StringToYamlObject(string value)
        {
            long num;
            float dec;

            if (long.TryParse(value, out num))
            {
                return num;
            }
            else if (float.TryParse(value, out dec))
            {
                return dec;
            }
            else
            {
                return value;
            }
        }
    }
}