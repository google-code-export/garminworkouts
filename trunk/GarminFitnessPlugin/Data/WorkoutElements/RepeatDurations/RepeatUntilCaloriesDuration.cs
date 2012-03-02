using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class RepeatUntilCaloriesDuration : IRepeatDuration
    {
        public RepeatUntilCaloriesDuration(RepeatStep parent)
            : base(RepeatDurationType.RepeatUntilCalories, parent)
        {
            CaloriesToSpend = 100;
        }

        public RepeatUntilCaloriesDuration(UInt16 caloriesToSpend, RepeatStep parent)
            : this(parent)
        {
            CaloriesToSpend = caloriesToSpend;
        }

        public RepeatUntilCaloriesDuration(Stream stream, DataVersion version, RepeatStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_CaloriesToSpend.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IRepeatDuration), stream, version);

            m_CaloriesToSpend.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            // XML not supported
            throw new NotSupportedException();
        }

        public override void Deserialize(XmlNode parentNode)
        {
            // XML not supported
            throw new NotSupportedException();
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField durationType = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField repeatPower = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilCalories);
            message.AddField(durationType);
            repeatPower.SetUInt32((UInt32)CaloriesToSpend);
            message.AddField(repeatPower);
        }

        public override bool ContainsFITOnlyFeatures
        {
            get { return true; }
        }

        public UInt16 CaloriesToSpend
        {
            get { return m_CaloriesToSpend; }
            set
            {
                if (CaloriesToSpend != value)
                {
                    m_CaloriesToSpend.Value = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("CaloriesToSpend"));
                }
            }
        }

        private GarminFitnessUInt16Range m_CaloriesToSpend = new GarminFitnessUInt16Range(0, Constants.MinCalories, Constants.MaxCalories);
    }
}
