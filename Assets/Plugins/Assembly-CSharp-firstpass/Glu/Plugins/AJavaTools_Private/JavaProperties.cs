using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Glu.Plugins.AMiscUtils;

namespace Glu.Plugins.AJavaTools_Private
{
	internal class JavaProperties
	{
		public static IDictionary<string, string> Load(string path)
		{
			path.ArgumentNotNull("path");
			using (FileStream stream = File.OpenRead(path))
			{
				return Load(stream);
			}
		}

		public static IDictionary<string, string> Load(Stream stream)
		{
			stream.ArgumentNotNull("stream");
			Regex regex = new Regex("^\\s*(?<key>[^\\s!#][^\\s=]*)\\s*(?:=\\s*)?(?<val>.*)$");
			StreamReader streamReader = new StreamReader(stream);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			while (!streamReader.EndOfStream)
			{
				string input = streamReader.ReadLine();
				Match match = regex.Match(input);
				if (match.Success)
				{
					dictionary[match.Groups["key"].Value] = match.Groups["val"].Value;
				}
			}
			return dictionary;
		}

		public static void Save(string path, IDictionary<string, string> props)
		{
			path.ArgumentNotNull("path");
			props.ArgumentNotNull("props");
			if (props.ContainsKey(string.Empty))
			{
				throw new ArgumentException("Empty property key is not supported", "props").Throw();
			}
			using (FileStream stream = File.Create(path))
			{
				Save(stream, props);
			}
		}

		public static void Save(Stream stream, IDictionary<string, string> props)
		{
			stream.ArgumentNotNull("stream");
			props.ArgumentNotNull("props");
			if (props.ContainsKey(string.Empty))
			{
				throw new ArgumentException("Empty property key is not supported", "props").Throw();
			}
			Encoding encoding = new UTF8Encoding(false);
			StreamWriter streamWriter = new StreamWriter(stream, encoding);
			try
			{
				foreach (KeyValuePair<string, string> prop in props)
				{
					streamWriter.WriteLine("{0}={1}".Fmt(prop.Key, prop.Value));
				}
			}
			finally
			{
				streamWriter.Flush();
			}
		}
	}
}
