using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class GarminFitnessBool : IPluginSerializable, IXMLSerializable
    {
        public GarminFitnessBool(bool value)
            : this(value, Boolean.TrueString, Boolean.FalseString)
        {
        }

        public GarminFitnessBool(bool value, String trueText, String falseText)
        {
            Value = value;
            m_TrueString = trueText;
            m_FalseString = falseText;
        }

        public static implicit operator bool(GarminFitnessBool value)
        {
            return value.m_Value;
        }

        public override string ToString()
        {
            if (this)
            {
                return m_TrueString;
            }
            else
            {
                return m_FalseString;
            }
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(this), 0, sizeof(bool));
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] boolBuffer = new byte[sizeof(bool)];

            stream.Read(boolBuffer, 0, sizeof(bool));
            Value = BitConverter.ToBoolean(boolBuffer, 0);
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

            Value = (node.FirstChild.Value == m_TrueString);
        }

        public bool GetTextValue(String text)
        {
            Debug.Assert(text == m_TrueString || text == m_FalseString);

            return text == m_TrueString;
        }

        public bool Value
        {
            set { m_Value = value; }
        }

        private bool m_Value;
        private readonly String m_TrueString;
        private readonly String m_FalseString;
    }
}
