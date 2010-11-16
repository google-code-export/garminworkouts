using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessDoubleRange : IPluginSerializable, IXMLSerializable, IComparable<GarminFitnessDoubleRange>
    {
        public GarminFitnessDoubleRange(double value)
            : this(value, double.MinValue, double.MaxValue)
        {
        }

        public GarminFitnessDoubleRange(double value, double minValue, double maxValue)
        {
            m_MinimumValue = minValue;
            m_MaximumValue = maxValue;

            Value = value;
        }

        public static implicit operator double(GarminFitnessDoubleRange value)
        {
            return value.m_Value;
        }

#region IComparable Members
        public int CompareTo(GarminFitnessDoubleRange obj)
        {
            return m_Value.CompareTo(obj.m_Value);
        }
#endregion

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(this), 0, sizeof(double));
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] doubleBuffer = new byte[sizeof(double)];

            stream.Read(doubleBuffer, 0, sizeof(double));
            Value = BitConverter.ToDouble(doubleBuffer, 0);
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
                throw new GarminFitnessXmlDeserializationException("Invalid double for node", node);
            }

            Value = double.Parse(node.FirstChild.Value, NumberStyles.Float, culture.NumberFormat);
        }

        public bool IsInRange(double value)
        {
            return value >= (m_MinimumValue - Constants.Delta) && value <= (m_MaximumValue + Constants.Delta);
        }

        public double Value
        {
            set
            {
                Debug.Assert(IsInRange(value));

                m_Value = value;
            }
        }

        private double m_Value;
        private readonly double m_MinimumValue;
        private readonly double m_MaximumValue;
    }
}
