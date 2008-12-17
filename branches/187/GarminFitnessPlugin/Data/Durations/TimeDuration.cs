using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class TimeDuration : IDuration
    {
        public TimeDuration(IStep parent)
            : base(DurationType.Time, parent)
        {
            TimeInSeconds = 300;
        }

        public TimeDuration(UInt16 timeInSeconds, IStep parent)
            : this(parent)
        {
            TimeInSeconds = timeInSeconds;
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

                    if (child.ChildNodes.Count == 1 && child.FirstChild.GetType() == typeof(XmlText) && Utils.IsTextIntegerInRange(child.FirstChild.Value, Constants.MinTime, UInt16.MaxValue))
                    {
                        TimeInSeconds = Math.Min(UInt16.Parse(child.FirstChild.Value), Constants.MaxTime);
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
                if (TimeInSeconds != value)
                {
                    Debug.Assert(value <= Constants.MaxTime);
                    m_TimeInSeconds = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("TimeInSeconds"));
                }
            }
        }

        public UInt16 Hours
        {
            get { return (UInt16)(TimeInSeconds / Constants.SecondsPerHour); }
        }

        public UInt16 Minutes
        {
            get { return (UInt16)((TimeInSeconds / Constants.SecondsPerMinute) % Constants.MinutesPerHour); }
        }

        public UInt16 Seconds
        {
            get { return (UInt16)(TimeInSeconds % Constants.SecondsPerMinute); }
        }

        private UInt16 m_TimeInSeconds;
    }
}
