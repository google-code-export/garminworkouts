using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessUInt16Range : IPluginSerializable, IXMLSerializable, IComparable<GarminFitnessUInt16Range>
    {
        public GarminFitnessUInt16Range(UInt16 value)
            : this(value, UInt16.MinValue, UInt16.MaxValue)
        {
        }

        public GarminFitnessUInt16Range(UInt16 value, UInt16 min, UInt16 max)
        {
            m_MinimumValue = Math.Min(min, max);
            m_MaximumValue = Math.Max(min, max);

            Value = value;
        }

        public static implicit operator UInt16(GarminFitnessUInt16Range value)
        {
            return value.m_Value;
        }

#region IComparable Members
        public int CompareTo(GarminFitnessUInt16Range obj)
        {
            return m_Value.CompareTo(obj.m_Value);
        }
#endregion

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(this), 0, sizeof(UInt16));
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(UInt16)];

            stream.Read(intBuffer, 0, sizeof(UInt16));
            Value = BitConverter.ToUInt16(intBuffer, 0);
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

        public bool IsInRange(UInt16 value)
        {
            return value >= m_MinimumValue && value <= m_MaximumValue;
        }

        public override string ToString()
        {
            return m_Value.ToString();
        }

        private bool TryParse(String textValue)
        {
            if (Utils.IsTextIntegerInRange(textValue, m_MinimumValue, m_MaximumValue))
            {
                Value = UInt16.Parse(textValue);
                return true;
            }

            return false;
        }

        public UInt16 Value
        {
            set
            {
                m_Value = (UInt16)Utils.Clamp(value, m_MinimumValue, m_MaximumValue);
            }
        }

        private UInt16 m_Value;
        private readonly UInt16 m_MinimumValue;
        private readonly UInt16 m_MaximumValue;
    }
}
