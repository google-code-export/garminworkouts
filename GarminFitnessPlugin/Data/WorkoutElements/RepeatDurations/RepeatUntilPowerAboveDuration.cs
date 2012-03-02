using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class RepeatUntilPowerAboveDuration : IRepeatDuration
    {
        public RepeatUntilPowerAboveDuration(RepeatStep parent)
            : base(RepeatDurationType.RepeatUntilPowerAbove, parent)
        {
        }

        public RepeatUntilPowerAboveDuration(UInt16 maxPower, bool isPercentFTP, RepeatStep parent)
            : this(parent)
        {
            ValidateValue(maxPower, isPercentFTP);

            MaxPower = maxPower;
            IsPercentFTP = isPercentFTP;
        }

        public RepeatUntilPowerAboveDuration(Stream stream, DataVersion version, RepeatStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_IsPercentFTP.Serialize(stream);
            InternalMaxPower.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IRepeatDuration), stream, version);

            m_IsPercentFTP.Deserialize(stream, version);
            InternalMaxPower.Deserialize(stream, version);
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

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilPowerGreaterThan);
            message.AddField(durationType);

            if (IsPercentFTP)
            {
                repeatPower.SetUInt32((UInt32)MaxPower);
            }
            else
            {
                repeatPower.SetUInt32((UInt32)MaxPower + 1000);
            }
            message.AddField(repeatPower);
        }

        private void ValidateValue(UInt16 maxPower, bool isPercentFTP)
        {
            if (isPercentFTP)
            {
                Debug.Assert(m_MaxPowerPercent.IsInRange(maxPower));
            }
            else
            {
                Debug.Assert(m_MaxPowerWatts.IsInRange(maxPower));
            }
        }

        public override bool ContainsFITOnlyFeatures
        {
            get { return true; }
        }

        public UInt16 MaxPower
        {
            get { return InternalMaxPower; }
            set
            {
                if (MaxPower != value)
                {
                    InternalMaxPower.Value = value;
                    ValidateValue(value, IsPercentFTP);

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("MaxPower"));
                }
            }
        }

        private GarminFitnessUInt16Range InternalMaxPower
        {
            get
            {
                if (IsPercentFTP)
                {
                    return m_MaxPowerPercent;
                }
                else
                {
                    return m_MaxPowerWatts;
                }
            }
        }

        public bool IsPercentFTP
        {
            get { return m_IsPercentFTP; }
            set
            {
                if (IsPercentFTP != value)
                {
                    m_IsPercentFTP.Value = value;
                    ValidateValue(MaxPower, value);

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("IsPercentFTP"));
                }
            }
        }

        private GarminFitnessUInt16Range m_MaxPowerWatts = new GarminFitnessUInt16Range(250, Constants.MinPowerInWatts, Constants.MaxPowerWorkoutInWatts);
        private GarminFitnessUInt16Range m_MaxPowerPercent = new GarminFitnessUInt16Range(90, Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTP);
        private GarminFitnessBool m_IsPercentFTP = new GarminFitnessBool(false, Constants.PowerReferenceTCXString[1], Constants.PowerReferenceTCXString[0]);
    }
}
