using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class RepeatUntilPowerBelowDuration : IRepeatDuration
    {
        public RepeatUntilPowerBelowDuration(RepeatStep parent)
            : base(RepeatDurationType.RepeatUntilPowerBelow, parent)
        {
        }

        public RepeatUntilPowerBelowDuration(UInt16 minPower, bool isPercentFTP, RepeatStep parent)
            : this(parent)
        {
            ValidateValue(minPower, isPercentFTP);

            MinPower = minPower;
            IsPercentFTP = isPercentFTP;
        }

        public RepeatUntilPowerBelowDuration(Stream stream, DataVersion version, RepeatStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_IsPercentFTP.Serialize(stream);
            InternalMinPower.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IRepeatDuration), stream, version);

            m_IsPercentFTP.Deserialize(stream, version);
            InternalMinPower.Deserialize(stream, version);
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
            FITMessageField durationType = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField repeatPower = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.TargetValue);

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilPowerLessThan);

            if (IsPercentFTP)
            {
                repeatPower.SetUInt32((UInt32)MinPower);
            }
            else
            {
                repeatPower.SetUInt32((UInt32)MinPower + 1000);
            }
        }

        private void ValidateValue(UInt16 minPower, bool isPercentFTP)
        {
            if (isPercentFTP)
            {
                Debug.Assert(m_MinPowerPercent.IsInRange(minPower));
            }
            else
            {
                Debug.Assert(m_MinPowerWatts.IsInRange(minPower));
            }
        }

        public override bool ContainsFITOnlyFeatures
        {
            get { return true; }
        }

        public bool IsPercentFTP
        {
            get { return m_IsPercentFTP; }
            set
            {
                if (IsPercentFTP != value)
                {
                    m_IsPercentFTP.Value = value;

                    ValidateValue(MinPower, value);

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("IsPercentFTP"));
                }
            }
        }

        public UInt16 MinPower
        {
            get { return InternalMinPower; }
            set
            {
                if (MinPower != value)
                {
                    ValidateValue(value, IsPercentFTP);

                    InternalMinPower.Value = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("MinPower"));
                }
            }
        }

        private GarminFitnessUInt16Range InternalMinPower
        {
            get
            {
                if (IsPercentFTP)
                {
                    return m_MinPowerPercent;
                }
                else
                {
                    return m_MinPowerWatts;
                }
            }
        }

        private GarminFitnessUInt16Range m_MinPowerWatts = new GarminFitnessUInt16Range(150, Constants.MinPowerInWatts, Constants.MaxPowerWorkoutInWatts);
        private GarminFitnessUInt16Range m_MinPowerPercent = new GarminFitnessUInt16Range(75, Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTP);
        private GarminFitnessBool m_IsPercentFTP = new GarminFitnessBool(false, Constants.PowerReferenceTCXString[1], Constants.PowerReferenceTCXString[0]);
    }
}
