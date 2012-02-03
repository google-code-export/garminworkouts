using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessSByteRange : IPluginSerializable, IXMLSerializable
    {
        public GarminFitnessSByteRange(SByte value)
            : this(value, SByte.MinValue, SByte.MaxValue)
        {
        }

        public GarminFitnessSByteRange(SByte value, SByte min, SByte max)
        {
            m_MinimumValue = Math.Min(min, max);
            m_MaximumValue = Math.Max(min, max);

            Value = value;
        }

        public static implicit operator SByte(GarminFitnessSByteRange value)
        {
            return value.m_Value;
        }

        public override string ToString()
        {
            return m_Value.ToString();
        }

        public override void Serialize(Stream stream)
        {
            Byte[] buffer = new Byte[1];

            buffer[0] = (Byte)m_Value;
            stream.Write(buffer, 0, 1);
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            Byte[] buffer = new Byte[1];

            stream.Read(buffer, 0, 1);
            Value = (SByte)buffer[0];
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
                throw new GarminFitnessXmlDeserializationException("Unable to deserialize string node", node);
            }
            else if (!TryParse(node.FirstChild.Value))
            {
                throw new GarminFitnessXmlDeserializationException("Invalid integer for node", node);
            }
        }

        public bool IsInRange(SByte value)
        {
            return value >= m_MinimumValue && value <= m_MaximumValue;
        }

        private bool TryParse(String textValue)
        {
            if (Utils.IsTextIntegerInRange(textValue, m_MinimumValue, m_MaximumValue))
            {
                Value = SByte.Parse(textValue);
                return true;
            }

            return false;
        }

        public SByte Value
        {
            set
            {
                m_Value = (SByte)Utils.Clamp(value, m_MinimumValue, m_MaximumValue);
            }
        }

        private SByte m_Value;
        private readonly SByte m_MinimumValue;
        private readonly SByte m_MaximumValue;
    }
}
