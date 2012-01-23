using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class TimeDuration : IDuration
    {
        public TimeDuration(IStep parent)
            : base(DurationType.Time, parent)
        {
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

            m_TimeInSeconds.Serialize(stream);
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField durationType = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField durationValue = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationValue);

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.Time);
            message.AddField(durationType);

            durationValue.SetUInt32((UInt32)TimeInSeconds * 1000);
            message.AddField(durationValue);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);

            m_TimeInSeconds.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            // This node was added by our parent...
            parentNode = parentNode.LastChild;

            m_TimeInSeconds.Serialize(parentNode, "Seconds", document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            if (parentNode.ChildNodes.Count != 1 || parentNode.FirstChild.Name != "Seconds")
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in time duration XML node", parentNode);
            }

            m_TimeInSeconds.Deserialize(parentNode.FirstChild);
        }

        public UInt16 TimeInSeconds
        {
            get { return m_TimeInSeconds; }
            set
            {
                if (TimeInSeconds != value)
                {
                    Debug.Assert(value <= Constants.MaxTime);
                    m_TimeInSeconds.Value = value;

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

        private GarminFitnessUInt16Range m_TimeInSeconds = new GarminFitnessUInt16Range(300, Constants.MinTime, Constants.MaxTime);
    }
}
