using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessByteRange : IPluginSerializable, IXMLSerializable
    {
        public GarminFitnessByteRange(Byte value)
            : this(value, Byte.MinValue, Byte.MaxValue)
        {
        }

        public GarminFitnessByteRange(Byte value, Byte min, Byte max)
        {
            m_MinimumValue = Math.Min(min, max);
            m_MaximumValue = Math.Max(min, max);

            Value = value;
        }

        public static implicit operator Byte(GarminFitnessByteRange value)
        {
            return value.m_Value;
        }

        public override string ToString()
        {
            return m_Value.ToString();
        }

        public override void Serialize(Stream stream)
        {
            stream.WriteByte(this);
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            Value = (Byte)stream.ReadByte();
        }

        public void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            XmlNode childNode;

            childNode = document.CreateElement(nodeName);
            childNode.AppendChild(document.CreateTextNode(m_Value.ToString()));
            parentNode.AppendChild(childNode);
        }

        public void Deserialize(XmlNode node)
        {
            if (node.ChildNodes.Count != 1 || node.FirstChild.GetType() != typeof(XmlText))
            {
                throw new GarminFitnesXmlDeserializationException("Unable to deserialize string node", node);
            }
            else if (!TryParse(node.FirstChild.Value))
            {
                throw new GarminFitnesXmlDeserializationException("Invalid integer for node", node);
            }
        }

        public bool IsInRange(Byte value)
        {
            return value >= m_MinimumValue && value <= m_MaximumValue;
        }

        private bool TryParse(String textValue)
        {
            if (Utils.IsTextIntegerInRange(textValue, m_MinimumValue, m_MaximumValue))
            {
                Value = Byte.Parse(textValue);
                return true;
            }

            return false;
        }

        public Byte Value
        {
            set
            {
                Debug.Assert(IsInRange(value));

                m_Value = value;
            }
        }

        private Byte m_Value;
        private readonly Byte m_MinimumValue;
        private readonly Byte m_MaximumValue;
    }
}
