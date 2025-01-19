using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

[XmlRoot("SerializableDictionary")]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader xmlReader)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(TKey));
		XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(TValue));
		if (xmlReader.IsEmptyElement)
		{
			xmlReader.Read();
			return;
		}
		xmlReader.Read();
		while (xmlReader.NodeType != XmlNodeType.EndElement)
		{
			xmlReader.ReadStartElement("DictionaryEntry");
			xmlReader.ReadStartElement("Key");
			TKey key = (TKey)xmlSerializer.Deserialize(xmlReader);
			xmlReader.ReadEndElement();
			xmlReader.ReadStartElement("Value");
			TValue value = (TValue)xmlSerializer2.Deserialize(xmlReader);
			xmlReader.ReadEndElement();
			Add(key, value);
			xmlReader.ReadEndElement();
			xmlReader.MoveToContent();
		}
	}

	public void WriteXml(XmlWriter xmlWriter)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(TKey));
		XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(TValue));
		foreach (TKey key in base.Keys)
		{
			xmlWriter.WriteStartElement("DictionaryEntry");
			xmlWriter.WriteStartElement("Key");
			xmlSerializer.Serialize(xmlWriter, key);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("Value");
			xmlSerializer2.Serialize(xmlWriter, this[key]);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
		}
	}
}
