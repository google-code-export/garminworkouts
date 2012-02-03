using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessUInt32Range : IPluginSerializable, IXMLSerializable, IComparable<GarminFitnessUInt32Range>
    {
        public GarminFitnessUInt32Range(UInt32 value)
            : this(value, UInt32.MinValue, UInt32.MaxValue)
        {
        }

        public GarminFitnessUInt32Range(UInt32 value, UInt32 min, UInt32 max)
        {
            m_MinimumValue = Math.Min(min, max);
            m_MaximumValue = Math.Max(min, max);

            Value = value;
        }

        public static implicit operator UInt32(GarminFitnessUInt32Range value)
        {
            return value.m_Value;
        }

#region IComparable Members
        public int CompareTo(GarminFitnessUInt32Range obj)
        {
            return m_Value.CompareTo(obj.m_Value);
        }
#endregion

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(this), 0, sizeof(UInt32));
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(UInt32)];

            stream.Read(intBuffer, 0, sizeof(UInt32));
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

        public bool IsInRange(UInt32 value)
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
                Value = UInt32.Parse(textValue);
                return true;
            }

            return false;
        }

        public UInt32 Value
        {
            set
            {
                m_Value = (UInt32)Utils.Clamp(value, m_MinimumValue, m_MaximumValue);
            }
        }

        private UInt32 m_Value;
        private readonly UInt32 m_MinimumValue;
        private readonly UInt32 m_MaximumValue;
    }
}
