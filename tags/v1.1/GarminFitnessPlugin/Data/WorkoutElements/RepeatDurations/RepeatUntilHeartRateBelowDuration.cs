using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class RepeatUntilHeartRateBelowDuration : IRepeatDuration
    {
        public RepeatUntilHeartRateBelowDuration(RepeatStep parent)
            : base(RepeatDurationType.RepeatUntilHeartRateBelow, parent)
        {
        }

        public RepeatUntilHeartRateBelowDuration(Byte minHeartRate, bool isPercentageMaxHeartRate, RepeatStep parent)
            : this(parent)
        {
            ValidateValue(minHeartRate, isPercentageMaxHeartRate);

            MinHeartRate = minHeartRate;
            IsPercentageMaxHeartRate = isPercentageMaxHeartRate;
        }

        public RepeatUntilHeartRateBelowDuration(Stream stream, DataVersion version, RepeatStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_IsPercentageMaxHR.Serialize(stream);
            InternalMinHeartRate.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IRepeatDuration), stream, version);

            m_IsPercentageMaxHR.Deserialize(stream, version);
            InternalMinHeartRate.Deserialize(stream, version);
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

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilHeartRateLessThan);

            if (IsPercentageMaxHeartRate)
            {
                repeatPower.SetUInt32((UInt32)MinHeartRate);
            }
            else
            {
                repeatPower.SetUInt32((UInt32)MinHeartRate + 100);
            }
        }

        private void ValidateValue(Byte minHeartRate, bool isPercentageMaxHeartRate)
        {
            if (isPercentageMaxHeartRate)
            {
                Debug.Assert(m_MinHeartRatePercent.IsInRange(minHeartRate));
            }
            else
            {
                Debug.Assert(m_MinHeartRateBPM.IsInRange(minHeartRate));
            }
        }

        public override bool ContainsFITOnlyFeatures
        {
            get { return true; }
        }

        public bool IsPercentageMaxHeartRate
        {
            get { return m_IsPercentageMaxHR; }
            set
            {
                if (IsPercentageMaxHeartRate != value)
                {
                    ValidateValue(MinHeartRate, value);
                    m_IsPercentageMaxHR.Value = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("IsPercentageMaxHeartRate"));
                }
            }
        }

        public Byte MinHeartRate
        {
            get { return InternalMinHeartRate; }
            set
            {
                if (MinHeartRate != value)
                {
                    InternalMinHeartRate.Value = value;

                    ValidateValue(value, m_IsPercentageMaxHR);

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("MinHeartRate"));
                }
            }
        }

        private GarminFitnessByteRange InternalMinHeartRate
        {
            get
            {
                if (IsPercentageMaxHeartRate)
                {
                    return m_MinHeartRatePercent;
                }
                else
                {
                    return m_MinHeartRateBPM;
                }
            }
        }

        private GarminFitnessByteRange m_MinHeartRateBPM = new GarminFitnessByteRange(170, Constants.MinHRInBPM, Constants.MaxHRInBPM);
        private GarminFitnessByteRange m_MinHeartRatePercent = new GarminFitnessByteRange(70, Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax);
        private GarminFitnessBool m_IsPercentageMaxHR = new GarminFitnessBool(true, Constants.HeartRateReferenceTCXString[1], Constants.HeartRateReferenceTCXString[0]);
    }
}
