using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ULegacyRipper
{
    public class IndentedWriter
    {
        private List<string> lines = new List<string>();

        public int indentationLevel;

        public void WriteLine(string line)
        {
            lines.Add(new string(' ', indentationLevel * 4) + line);
        }

        public void WriteLines(List<string> lines)
        {
            foreach (string line in lines)
            {
                this.lines.Add(new string(' ', indentationLevel * 4) + line);//
            }
        }

        public void BeginBraces()
        {
            WriteLine("{");
            indentationLevel++;
        }

        public void EndBraces()
        {
            indentationLevel--;
            WriteLine("}");
        }

        public void EndBracesStruct()
        {
            indentationLevel--;
            WriteLine("};");
        }

        public override string ToString()
        {
            return string.Join("\n", lines.ToArray());
        }
    }

    public class IndentedReader
    {
        private string[] lines;

        public int indentationLevel, lineIndex;

        private void RecalculateIndentation()
        {
            indentationLevel = 0;

            for (int i = 0; i < lines[lineIndex].Length; i++)
            {
                switch (lines[lineIndex][i])
                {
                    case ' ': indentationLevel++; continue;
                }

                break;
            }
        }

        private void RecalculateIndentation(int index)
        {
            indentationLevel = 0;

            for (int i = 0; i < lines[index].Length; i++)
            {
                switch (lines[index][i])
                {
                    case ' ': indentationLevel++; continue;
                }

                break;
            }
        }

        public IndentedReader(string content)
        {
            lines = content.Split('\n');
            RecalculateIndentation();
        }

        public string ReadLine()
        {
            string line = "";

            while (line == "")
            {
                if (lineIndex >= lines.Length - 1)
                {
                    break;
                }

                line = lines[lineIndex++].Substring(indentationLevel);
                RecalculateIndentation();
            }

            return line;
        }

        public List<string> ReadUntil(string pattern)
        {
            List<string> readLines = new List<string>();
            string line;

            while (true)
            {
                line = lines[lineIndex++].Substring(indentationLevel);
                RecalculateIndentation();

                if (line.StartsWith(pattern))
                {
                    break;
                }

                if (line.Trim() != "")
                {
                    readLines.Add(line);
                }
            }

            return readLines;
        }

        public List<string> ReadUntil(string pattern, string excludePattern)
        {
            List<string> readLines = new List<string>();
            string line;

            while (true)
            {
                line = lines[lineIndex++].Substring(indentationLevel);
                RecalculateIndentation();

                if (line == "")
                {
                    continue;
                }

                if (line.StartsWith(pattern))
                {
                    break;
                }

                if (line.Trim() != "" && !line.StartsWith(excludePattern))//
                {
                    readLines.Add(line);
                }
            }

            return readLines;
        }

        public void Search(string pattern, bool readPast = true)
        {
            string line = "";

            while (!line.StartsWith(pattern))
            {
                line = lines[lineIndex++].Substring(indentationLevel);
                RecalculateIndentation();
            }

            if (!readPast)
            {
                lineIndex--;
            }
        }

        public string SearchFile(string pattern)
        {
            string line = "";
            int index = 0;

            while (!line.StartsWith(pattern))
            {
                line = lines[index++].Substring(indentationLevel);
                RecalculateIndentation(index);
            }

            return line;
        }

        public bool IsFirst(string first, string second)
        {
            int previousIndex = lineIndex;
            ReadUntil(first);

            int firstIndex = lineIndex;
            lineIndex = previousIndex;

            ReadUntil(second);
            int secondIndex = lineIndex;

            lineIndex = previousIndex;

            return firstIndex < secondIndex;
        }
    }
}