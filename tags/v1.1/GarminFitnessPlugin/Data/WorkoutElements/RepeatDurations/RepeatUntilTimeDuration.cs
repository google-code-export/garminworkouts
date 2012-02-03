using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class RepeatUntilTimeDuration : IRepeatDuration
    {
        public RepeatUntilTimeDuration(RepeatStep parent)
            : base(RepeatDurationType.RepeatUntilTime, parent)
        {
        }

        public RepeatUntilTimeDuration(UInt16 timeInSeconds, RepeatStep parent)
            : this(parent)
        {
            TimeInSeconds = timeInSeconds;
        }

        public RepeatUntilTimeDuration(Stream stream, DataVersion version, RepeatStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_TimeInSeconds.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IRepeatDuration), stream, version);

            m_TimeInSeconds.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            // XML not supported
            Debug.Assert(false);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            // XML not supported
            Debug.Assert(false);
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField durationType = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField repeatTime = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilTime);
            message.AddField(durationType);
            repeatTime.SetUInt32((UInt32)TimeInSeconds * 1000);
            message.AddField(repeatTime);
        }

        public override bool ContainsFITOnlyFeatures
        {
            get { return true; }
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
