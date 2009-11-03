using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessDate : IPluginSerializable, IXMLSerializable
    {
        public GarminFitnessDate()
            : this(new DateTime(0))
        {
        }

        public GarminFitnessDate(long value)
            : this(new DateTime(value))
        {
        }

        public GarminFitnessDate(DateTime value)
        {
            Value = value;
        }

        public static implicit operator DateTime(GarminFitnessDate value)
        {
            return value.m_Value;
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(m_Value.Ticks), 0, sizeof(long));
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] dateBuffer = new byte[sizeof(long)];

            stream.Read(dateBuffer, 0, sizeof(long));
            Value = new DateTime(BitConverter.ToInt64(dateBuffer, 0));
        }

        public void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            XmlNode childNode;

            childNode = document.CreateElement(nodeName);
            childNode.AppendChild(document.CreateTextNode(m_Value.ToString("yyyy-MM-dd")));
            parentNode.AppendChild(childNode);
        }

        public void Deserialize(XmlNode node)
        {
            if (node.ChildNodes.Count != 1 || node.FirstChild.GetType() != typeof(XmlText))
            {
                throw new GarminFitnesXmlDeserializationException("Unable to deserialize string node", node);
            }

            CultureInfo culture = new CultureInfo("en-us");

            Value = DateTime.ParseExact(node.FirstChild.Value, "yyyy-MM-dd", culture.DateTimeFormat);
        }

        public override int GetHashCode()
        {
            return m_Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(DateTime))
            {
                return m_Value.Equals(obj);
            }
            else if (obj.GetType() == typeof(GarminFitnessDate))
            {
                return m_Value.Equals(((GarminFitnessDate)obj).m_Value);
            }

            return false;
        }

        public DateTime Value
        {
            set
            {
                m_Value = value;
            }
        }

        private DateTime m_Value;
    }
}
