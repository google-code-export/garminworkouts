using System;
using System.ComponentModel;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class CaloriesDuration : IDuration
    {
        public CaloriesDuration(IStep parent)
            : base(DurationType.Calories, parent)
        {
            CaloriesToSpend = 100;
        }

        public CaloriesDuration(UInt16 caloriesToSpend, IStep parent)
            : this(parent)
        {
            CaloriesToSpend = caloriesToSpend;
        }

        public CaloriesDuration(Stream stream, DataVersion version, IStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.Write(BitConverter.GetBytes(CaloriesToSpend), 0, sizeof(UInt16));
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);

            byte[] intBuffer = new byte[sizeof(UInt16)];

            stream.Read(intBuffer, 0, sizeof(UInt16));
            CaloriesToSpend = BitConverter.ToUInt16(intBuffer, 0);
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);

            XmlNode childNode;

            childNode = document.CreateElement("Calories");
            childNode.AppendChild(document.CreateTextNode(CaloriesToSpend.ToString()));

            parentNode.AppendChild(childNode);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                if(parentNode.ChildNodes.Count == 1 && parentNode.FirstChild.Name == "Calories")
                {
                    XmlNode child = parentNode.FirstChild;

                    if (child.ChildNodes.Count == 1 && child.FirstChild.GetType() == typeof(XmlText) && Utils.IsTextIntegerInRange(child.FirstChild.Value, 1, 65535))
                    {
                        CaloriesToSpend = UInt16.Parse(child.FirstChild.Value);
                        return true;
                    }
                }
            }

            return false;
        }

        public UInt16 CaloriesToSpend
        {
            get { return m_CaloriesToSpend; }
            set
            {
                if (CaloriesToSpend != value)
                {
                    m_CaloriesToSpend = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("CaloriesToSpend"));
                }
            }
        }

        private UInt16 m_CaloriesToSpend;
    }
}
