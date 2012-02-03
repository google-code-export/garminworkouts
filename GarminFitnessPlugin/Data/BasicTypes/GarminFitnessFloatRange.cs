using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessFloatRange : IPluginSerializable, IXMLSerializable, IComparable<GarminFitnessFloatRange>
    {
        public GarminFitnessFloatRange(float value)
            : this(value, float.MinValue, float.MaxValue)
        {
        }

        public GarminFitnessFloatRange(float value, float minValue, float maxValue)
        {
            m_MinimumValue = minValue;
            m_MaximumValue = maxValue;

            Value = value;
        }

        public static implicit operator float(GarminFitnessFloatRange value)
        {
            return value.m_Value;
        }

#region IComparable Members
        public int CompareTo(GarminFitnessFloatRange obj)
        {
            return m_Value.CompareTo(obj.m_Value);
        }
#endregion

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(this), 0, sizeof(float));
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] floatBuffer = new byte[sizeof(float)];

            stream.Read(floatBuffer, 0, sizeof(float));
            Value = BitConverter.ToSingle(floatBuffer, 0);
        }

        public void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            XmlNode childNode;
            CultureInfo culture = new CultureInfo("en-us");

            childNode = document.CreateElement(nodeName);
            childNode.AppendChild(document.CreateTextNode(m_Value.ToString("0.00000", culture.NumberFormat)));
            parentNode.AppendChild(childNode);
        }

        public void Deserialize(XmlNode node)
        {
            CultureInfo culture = new CultureInfo("en-us");

            if (node.ChildNodes.Count != 1 || node.FirstChild.GetType() != typeof(XmlText))
            {
                throw new GarminFitnessXmlDeserializationException("Unable to deserialize double node", node);
            }
            else if (!Utils.IsTextFloatInRange(node.FirstChild.Value, m_MinimumValue, m_MaximumValue, culture))
            {
                throw new GarminFitnessXmlDeserializationException("Invalid float for node", node);
            }

            Value = float.Parse(node.FirstChild.Value, NumberStyles.Float, culture.NumberFormat);
        }

        public bool IsInRange(float value)
        {
            return value >= (m_MinimumValue - Constants.Delta) && value <= (m_MaximumValue + Constants.Delta);
        }

        public float Value
        {
            set
            {
                m_Value = (float)Utils.Clamp(value, m_MinimumValue, m_MaximumValue);
            }
        }

        private float m_Value;
        private readonly float m_MinimumValue;
        private readonly float m_MaximumValue;
    }
}
