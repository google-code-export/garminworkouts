using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessInt32Range : IPluginSerializable, IXMLSerializable, IComparable<GarminFitnessInt32Range>
    {
        public GarminFitnessInt32Range(Int32 value)
            : this(value, Int32.MinValue, Int32.MaxValue)
        {
        }

        public GarminFitnessInt32Range(Int32 value, Int32 min, Int32 max)
        {
            m_MinimumValue = Math.Min(min, max);
            m_MaximumValue = Math.Max(min, max);

            Value = value;
        }

        public static implicit operator Int32(GarminFitnessInt32Range value)
        {
            return value.m_Value;
        }

#region IComparable Members
        public int CompareTo(GarminFitnessInt32Range obj)
        {
            return m_Value.CompareTo(obj.m_Value);
        }
#endregion

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(this), 0, sizeof(Int32));
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];

            stream.Read(intBuffer, 0, sizeof(Int32));
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
                throw new GarminFitnesXmlDeserializationException("Unable to deserialize string node", node);
            }
            else if (!TryParse(node.FirstChild.Value))
            {
                throw new GarminFitnesXmlDeserializationException("Invalid integer for node", node);
            }
        }

        public bool IsInRange(Int32 value)
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
                Value = Int32.Parse(textValue);
                return true;
            }

            return false;
        }

        public Int32 Value
        {
            set
            {
                Debug.Assert(IsInRange(value));

                m_Value = value;
            }
        }

        private Int32 m_Value;
        private readonly Int32 m_MinimumValue;
        private readonly Int32 m_MaximumValue;
    }
}
