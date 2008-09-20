using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.Data
{
    class TimeDuration : IDuration
    {
        public TimeDuration(IStep parent)
            : base(DurationType.Time, parent)
        {
            m_TimeInSeconds = 300;
        }

        public TimeDuration(UInt16 timeInSeconds, IStep parent)
            : this(parent)
        {
            m_TimeInSeconds = timeInSeconds;
        }

        public TimeDuration(Stream stream, DataVersion version, IStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.Write(BitConverter.GetBytes(TimeInSeconds), 0, sizeof(UInt16));
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);

            byte[] intBuffer = new byte[sizeof(UInt16)];

            stream.Read(intBuffer, 0, sizeof(UInt16));
            TimeInSeconds = BitConverter.ToUInt16(intBuffer, 0);
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);

            XmlNode childNode;

            childNode = document.CreateElement("Seconds");
            childNode.AppendChild(document.CreateTextNode(TimeInSeconds.ToString()));

            parentNode.AppendChild(childNode);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                if (parentNode.ChildNodes.Count == 1 && parentNode.FirstChild.Name == "Seconds")
                {
                    XmlNode child = parentNode.FirstChild;

                    if (child.ChildNodes.Count == 1 && child.FirstChild.GetType() == typeof(XmlText) && Utils.IsTextIntegerInRange(child.FirstChild.Value, 1, 65535))
                    {
                        TimeInSeconds = Math.Min(UInt16.Parse(child.FirstChild.Value), (UInt16)64799);
                        return true;
                    }
                }
            }

            return false;
        }

        public UInt16 TimeInSeconds
        {
            get { return m_TimeInSeconds; }
            set
            {
                Trace.Assert(value <= 64799);
                m_TimeInSeconds = value;
            }
        }

        public UInt16 Hours
        {
            get { return (UInt16)(m_TimeInSeconds / Constants.SecondsPerHour); }
        }

        public UInt16 Minutes
        {
            get { return (UInt16)((m_TimeInSeconds / Constants.SecondsPerMinute) % Constants.MinutesPerHour); }
        }

        public UInt16 Seconds
        {
            get { return (UInt16)(m_TimeInSeconds % Constants.SecondsPerMinute); }
        }

        private UInt16 m_TimeInSeconds;
    }
}
