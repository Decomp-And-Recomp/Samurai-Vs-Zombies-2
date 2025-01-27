using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ULegacyRipper
{
    public class IndentedWriter
    {
        private List<string> lines = new List<string>();

        public int indentationLevel;

        private int indentationMultiplier;

        public IndentedWriter(int indentationMultiplier = 4)
        {
            this.indentationMultiplier = indentationMultiplier;
        }

        public void WriteLine(string line)
        {
            lines.Add(new string(' ', indentationLevel * indentationMultiplier) + line);
        }

        public void WriteLines(List<string> lines)
        {
            foreach (string line in lines)
            {
                this.lines.Add(new string(' ', indentationLevel * indentationMultiplier) + line);//
            }
        }

        public void WriteLines(params string[] lines)
        {
            foreach (string line in lines)
            {
                this.lines.Add(new string(' ', indentationLevel * indentationMultiplier) + line);//
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

        public bool IsAtEnd()
        {
            return lineIndex >= lines.Length - 1;
        }

        public bool HasEnded()
        {
            return lineIndex >= lines.Length;
        }

        public string CurrentLine()
        {
            if (HasEnded())
            {
                return "";
            }

            return lines[lineIndex].Substring(indentationLevel);
        }

        public void RecalculateIndentation()
        {
            if (IsAtEnd())
            {
                return;
            }

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

        public IndentedReader(string[] content)
        {
            lines = content;
            RecalculateIndentation();
        }

        public string ReadLine()
        {
            string line = "";

            while (line == "")
            {
                if (HasEnded())
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
    }
}