using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessGuid : IPluginSerializable, IXMLSerializable
    {
        public GarminFitnessGuid(Guid value)
        {
            Debug.Assert(value != Guid.Empty);

            Value = value;
        }
        public GarminFitnessGuid()
        {
            Value = Guid.NewGuid();
        }

        public static implicit operator Guid(GarminFitnessGuid value)
        {
            return value.m_Value;
        }

        public override string ToString()
        {
            return m_Value.ToString();
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(m_Value.ToByteArray(), 0, 16);
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] guidBuffer = new byte[16];

            stream.Read(guidBuffer, 0, 16);
            Value = new Guid(guidBuffer);
        }

        public void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            XmlNode childNode;

            childNode = document.CreateElement(nodeName);
            childNode.AppendChild(document.CreateTextNode(this.ToString()));
            parentNode.AppendChild(childNode);
        }

        public void SerializeAttribute(XmlNode parentNode, String attributeName, String namespaceURI, XmlDocument document)
        {
            XmlAttribute attributeNode;

            attributeNode = document.CreateAttribute(attributeName, namespaceURI);
            attributeNode.Value = this.ToString();
            parentNode.Attributes.Append(attributeNode);
        }

        public void SerializeAttribute(XmlNode parentNode, String attributeName, XmlDocument document)
        {
            XmlAttribute attributeNode;

            attributeNode = document.CreateAttribute(attributeName);
            attributeNode.Value = this.ToString();
            parentNode.Attributes.Append(attributeNode);
        }

        public void Deserialize(XmlNode node)
        {
            if (node.ChildNodes.Count != 1 || node.FirstChild.GetType() != typeof(XmlText))
            {
                throw new GarminFitnessXmlDeserializationException("Unable to deserialize bool node", node);
            }

            Value = new Guid(node.FirstChild.Value);
        }

        public Guid Value
        {
            set { m_Value = value; }
        }

        private Guid m_Value;
    }
}
