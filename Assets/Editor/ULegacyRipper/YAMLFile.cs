using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ULegacyRipper
{
    public class YAML
    {
        public Dictionary<long, YAMLNode> nodes = new Dictionary<long, YAMLNode>();

        public YAMLNode FindNode(string name)
        {
            foreach (KeyValuePair<long, YAMLNode> node in nodes)
            {
                if (node.Value.name == name)
                {
                    return node.Value;
                }
            }

            throw new Exception("No node matching " + name + " exists!");
        }

        public long Find(string name)
        {
            foreach (KeyValuePair<long, YAMLNode> node in nodes)
            {
                if (node.Value.name == name)
                {
                    return node.Key;
                }
            }

            throw new Exception("No node matching " + name + " exists!");
        }
    }

    public class YAMLNode
    {
        public Dictionary<string, YAMLNode> childNodes = new Dictionary<string, YAMLNode>();

        public Dictionary<string, string> table = new Dictionary<string, string>();

        public List<YAMLNode> nodeList = new List<YAMLNode>();

        public YAMLNode parentNode;

        public string name, value;

        public long type;

        public void CopyTable(YAMLNode source)
        {
            table = new Dictionary<string, string>(source.table);
        }

        public YAMLNode AddNode(string name, string value)
        {
            YAMLNode node = new YAMLNode
            {
                name = name,
                value = value
            };

            childNodes.Add(name, node);
            return node;
        }

        public YAMLNode AddNode(string name)
        {
            YAMLNode node = new YAMLNode
            {
                name = name
            };

            childNodes.Add(name, node);
            return node;
        }

        public YAMLNode CopyNode(string name, YAMLNode destination)
        {
            destination.name = name;
            childNodes.Add(name, destination);

            return childNodes[name];
        }
    }

    public static class YAMLUnityExtensions
    {
        public static YAMLNode AddEmptyFileIDNode(this YAMLNode node, string name)
        {
            YAMLNode childNode = new YAMLNode
            {
                name = name
            };

            childNode.table.Add("fileID", "0");
            node.childNodes.Add(name, childNode);

            return childNode;
        }

        public static YAMLNode AddEmptyVector4(this YAMLNode node, string name)
        {
            YAMLNode childNode = new YAMLNode
            {
                name = name
            };

            childNode.table.Add("x", "0");
            childNode.table.Add("y", "0");
            childNode.table.Add("z", "0");
            childNode.table.Add("w", "0");

            node.childNodes.Add(name, childNode);

            return childNode;
        }

        public static YAMLNode AddEmptyVector4ST(this YAMLNode node, string name)
        {
            YAMLNode childNode = new YAMLNode
            {
                name = name
            };

            childNode.table.Add("x", "1");
            childNode.table.Add("y", "1");
            childNode.table.Add("z", "0");
            childNode.table.Add("w", "0");

            node.childNodes.Add(name, childNode);

            return childNode;
        }
    }

    public static class YAMLWriter
    {
        public static string WriteYAML(YAML yaml)
        {
            IndentedWriter writer = new IndentedWriter(2);

            writer.WriteLines("%YAML 1.1",
            "%TAG !u! tag:unity3d.com,2011:");

            foreach (KeyValuePair<long, YAMLNode> node in yaml.nodes)
            {
                writer.WriteLine("--- !u!" + node.Value.type + " &" + node.Key);
                WriteYAMLNode(node.Value, writer);
            }

            writer.WriteLine(""); //extra newline at the end of unity yamls for some reason

            return writer.ToString();
        }

        private static void WriteYAMLNode(YAMLNode node, IndentedWriter writer, bool isArrayElement = false)
        {
            string line = (isArrayElement ? (string.IsNullOrEmpty(node.name) ? "-" : "- ") : "") + (string.IsNullOrEmpty(node.name) ? "" : (node.name + ":"));

            if (node.table.Count > 0)
            {
                line += " {";
                int index = 0;

                foreach (KeyValuePair<string, string> entry in node.table)
                {
                    if (index >= node.table.Count - 1)
                    {
                        line += entry.Key + ": " + entry.Value + "}";
                    }
                    else
                    {
                        line += entry.Key + ": " + entry.Value + ", ";
                    }

                    index++;
                }

                writer.WriteLine(line);
            }
            else
            {
                if (!string.IsNullOrEmpty(node.value))
                {
                    line += " " + node.value;
                }

                writer.WriteLine(line);

                if (node.nodeList.Count > 0)
                {
                    foreach (YAMLNode childNode in node.nodeList)
                    {
                        WriteYAMLNode(childNode, writer, true);
                    }
                }
            }

            if (node.childNodes.Count > 0)
            {
                writer.indentationLevel++;

                foreach (KeyValuePair<string, YAMLNode> childNode in node.childNodes)
                {
                    WriteYAMLNode(childNode.Value, writer);
                }

                writer.indentationLevel--;
            }
        }
    }

    public static class YAMLReader
    {
        public static YAML ReadYAML(string[] content)
        {
            IndentedReader reader = new IndentedReader(content);
            YAML yaml = new YAML();

            long currentTag = 0;
            long currentType = 0;

            while (!reader.HasEnded())
            {
                string line = reader.ReadLine();

                if (line.StartsWith("---"))
                {
                    currentTag = long.Parse(line.Split(' ')[2].Substring(1));
                    currentType = long.Parse(line.Split(' ')[1].Substring(3));
                }
                else if (!line.StartsWith("%"))
                {
                    reader.lineIndex--;
                    reader.RecalculateIndentation();

                    YAMLNode node = ReadNode(reader);
                    node.type = currentType;

                    yaml.nodes.Add(currentTag, node);
                }
            }

            return yaml;
        }

        private static YAMLNode ReadNode(IndentedReader reader, bool isArrayElement = false)
        {
            int currentIndentation = reader.indentationLevel;

            string line = reader.ReadLine();

            if (line.StartsWith("- ") && !line.Substring(2).StartsWith("{"))
            {
                line = line.Substring(2);
            }

            YAMLNode node = ReadSimpleKeyValue(line, reader);

            while (reader.indentationLevel == currentIndentation + 2 || (reader.CurrentLine().StartsWith("- ") && reader.indentationLevel == currentIndentation))
            {
                if (reader.HasEnded())
                {
                    break;
                }

                if (reader.CurrentLine().StartsWith("- "))
                {
                    if (isArrayElement)
                    {
                        break;
                    }

                    YAMLNode childNode = ReadNode(reader, true);
                    childNode.parentNode = node;

                    node.nodeList.Add(childNode);
                }
                else
                {
                    YAMLNode childNode = ReadNode(reader);
                    childNode.parentNode = node;
                    
                    node.childNodes.Add(childNode.name, childNode);
                }
            }

            return node;
        }

        private static YAMLNode ReadSimpleKeyValue(string line, IndentedReader reader)
        {
            if (line.StartsWith("- ") && line.Substring(2).StartsWith("{"))
            {
                YAMLNode entry = new YAMLNode();

                while (true)
                {
                    if (HandleValue(entry, line.Substring(2), reader))
                    {
                        break;
                    }
                }

                return entry;
            }

            int valueIndex = line.IndexOf(":") + 1;

            if (valueIndex < 1)
            {
                return new YAMLNode()
                {
                    value = line
                };
            }

            YAMLNode node = new YAMLNode()
            {
                name = line.Substring(0, valueIndex - 1)
            };

            if (line.Length > valueIndex)
            {
                while (true)
                {
                    if (HandleValue(node, line.Substring(valueIndex + 1), reader))
                    {
                        break;
                    }
                }
            }

            return node;
        }

        private static bool HandleValue(YAMLNode node, string value, IndentedReader reader)
        {
            if (value.StartsWith("{"))
            {
                if (value != "{}")
                {
                    while (true)
                    {
                        string[] values = value.Replace("{", "").Replace("}", "").Split(new string[1] { ", " }, StringSplitOptions.None);

                        for (int i = 0; i < values.Length; i++)
                        {
                            YAMLNode childNode = ReadSimpleKeyValue(values[i], reader);
                            childNode.parentNode = node;

                            node.table.Add(childNode.name, childNode.value);
                        }

                        if (!value.Contains("}"))
                        {
                            Debug.LogError("well that's a problem"); //implement this if it ever gets logged
                            break; //this wouldn't usually break here but y'know
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                node.value = value;
            }

            return true;
        }
    }
}