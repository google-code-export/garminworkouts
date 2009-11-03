using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessString : IPluginSerializable, IXMLSerializable
    {
        public GarminFitnessString()
            : this(String.Empty)
        {
        }

        public GarminFitnessString(String value)
            : this(value, UInt16.MaxValue)
        {
        }

        public GarminFitnessString(String value, UInt16 maxLength)
        {
            m_MaxLength = maxLength;
            Value = value;
        }

        public static implicit operator String(GarminFitnessString value)
        {
            return value.m_Value;
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(this)), 0, sizeof(Int32));
            stream.Write(Encoding.UTF8.GetBytes(this), 0, Encoding.UTF8.GetByteCount(this));
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] stringBuffer;
            Int32 stringLength;

            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            Value = Encoding.UTF8.GetString(stringBuffer);
        }

        public void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            XmlNode childNode;

            childNode = document.CreateElement(nodeName);
            childNode.AppendChild(document.CreateTextNode(this));
            parentNode.AppendChild(childNode);
        }

        public void Deserialize(XmlNode node)
        {
            if (node.ChildNodes.Count == 1 && node.FirstChild.GetType() != typeof(XmlText))
            {
                throw new GarminFitnesXmlDeserializationException("Unable to deserialize string node", node);
            }
            else if (node.ChildNodes.Count == 0)
            {
                // No name if no child nodes
                Value = "";
            }
            else
            {
                Value = node.FirstChild.Value;
            }
        }

        public String Value
        {
            set
            {
                Debug.Assert(value.Length <= m_MaxLength);

                m_Value = value;
            }
        }

        private String m_Value;
        private readonly UInt16 m_MaxLength;
    }
}
