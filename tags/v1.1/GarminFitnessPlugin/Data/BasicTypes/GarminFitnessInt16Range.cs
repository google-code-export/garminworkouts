using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessInt16Range : IPluginSerializable, IXMLSerializable, IComparable<GarminFitnessInt16Range>
    {
        public GarminFitnessInt16Range(Int16 value)
            : this(value, Int16.MinValue, Int16.MaxValue)
        {
        }

        public GarminFitnessInt16Range(Int16 value, Int16 min, Int16 max)
        {
            m_MinimumValue = Math.Min(min, max);
            m_MaximumValue = Math.Max(min, max);

            Value = value;
        }

        public static implicit operator Int16(GarminFitnessInt16Range value)
        {
            return value.m_Value;
        }

#region IComparable Members
        public int CompareTo(GarminFitnessInt16Range obj)
        {
            return m_Value.CompareTo(obj.m_Value);
        }
#endregion

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(this), 0, sizeof(Int16));
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int16)];

            stream.Read(intBuffer, 0, sizeof(Int16));
            Value = BitConverter.ToInt16(intBuffer, 0);
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

        public bool IsInRange(Int16 value)
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
                Value = Int16.Parse(textValue);
                return true;
            }

            return false;
        }

        public Int16 Value
        {
            set
            {
                m_Value = (Int16)Utils.Clamp(value, m_MinimumValue, m_MaximumValue);
            }
        }

        private Int16 m_Value;
        private readonly Int16 m_MinimumValue;
        private readonly Int16 m_MaximumValue;
    }
}
