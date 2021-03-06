using System;
using System.ComponentModel;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class RepeatCountDuration : IRepeatDuration
    {
        public RepeatCountDuration(RepeatStep parent)
            : base(RepeatDurationType.RepeatCount, parent)
        {
        }

        public RepeatCountDuration(Stream stream, DataVersion version, RepeatStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_RepetitionCount.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IRepeatDuration), stream, version);

            m_RepetitionCount.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            m_RepetitionCount.Serialize(parentNode, nodeName, document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            m_RepetitionCount.Deserialize(parentNode);
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField durationType = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField repeatCount = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.TargetValue);

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatCount);
            repeatCount.SetUInt32((UInt32)RepetitionCount);
        }

        public Byte RepetitionCount
        {
            get { return m_RepetitionCount; }
            set
            {
                if (m_RepetitionCount != value)
                {
                    m_RepetitionCount.Value = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("RepetitionCount"));
                }
            }
        }

        private GarminFitnessByteRange m_RepetitionCount = new GarminFitnessByteRange(Constants.MinRepeats, Constants.MinRepeats, Constants.MaxRepeats);
    }
}
