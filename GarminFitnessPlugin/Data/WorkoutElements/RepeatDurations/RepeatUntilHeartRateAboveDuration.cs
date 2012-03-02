using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class RepeatUntilHeartRateAboveDuration : IRepeatDuration
    {
        public RepeatUntilHeartRateAboveDuration(RepeatStep parent)
            : base(RepeatDurationType.RepeatUntilHeartRateAbove, parent)
        {
        }

        public RepeatUntilHeartRateAboveDuration(Byte maxHeartRate, bool isPercentageMaxHeartRate, RepeatStep parent)
            : this(parent)
        {
            ValidateValue(maxHeartRate, isPercentageMaxHeartRate);

            MaxHeartRate = maxHeartRate;
            IsPercentageMaxHeartRate = isPercentageMaxHeartRate;
        }

        public RepeatUntilHeartRateAboveDuration(Stream stream, DataVersion version, RepeatStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_IsPercentageMaxHR.Serialize(stream);
            InternalMaxHeartRate.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IRepeatDuration), stream, version);

            m_IsPercentageMaxHR.Deserialize(stream, version);
            InternalMaxHeartRate.Deserialize(stream, version);
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

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilHeartRateGreaterThan);
            message.AddField(durationType);

            if (IsPercentageMaxHeartRate)
            {
                repeatPower.SetUInt32((UInt32)MaxHeartRate);
            }
            else
            {
                repeatPower.SetUInt32((UInt32)MaxHeartRate + 100);
            }
            message.AddField(repeatPower);
        }

        private void ValidateValue(Byte maxHeartRate, bool isPercentageMaxHeartRate)
        {
            if (isPercentageMaxHeartRate)
            {
                Debug.Assert(m_MaxHeartRatePercent.IsInRange(maxHeartRate));
            }
            else
            {
                Debug.Assert(m_MaxHeartRateBPM.IsInRange(maxHeartRate));
            }
        }

        public override bool ContainsFITOnlyFeatures
        {
            get { return true; }
        }

        public Byte MaxHeartRate
        {
            get { return InternalMaxHeartRate; }
            set
            {
                if (MaxHeartRate != value)
                {
                    InternalMaxHeartRate.Value = value;
                    ValidateValue(value, IsPercentageMaxHeartRate);

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("MaxHeartRate"));
                }
            }
        }

        private GarminFitnessByteRange InternalMaxHeartRate
        {
            get
            {
                if (IsPercentageMaxHeartRate)
                {
                    return m_MaxHeartRatePercent;
                }
                else
                {
                    return m_MaxHeartRateBPM;
                }
            }
        }

        public bool IsPercentageMaxHeartRate
        {
            get { return m_IsPercentageMaxHR; }
            set
            {
                if (IsPercentageMaxHeartRate != value)
                {
                    m_IsPercentageMaxHR.Value = value;
                    ValidateValue(MaxHeartRate, value);

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("IsPercentageMaxHeartRate"));
                }
            }
        }

        private GarminFitnessByteRange m_MaxHeartRateBPM = new GarminFitnessByteRange(150, Constants.MinHRInBPM, Constants.MaxHRInBPM);
        private GarminFitnessByteRange m_MaxHeartRatePercent = new GarminFitnessByteRange(50, Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax);
        private GarminFitnessBool m_IsPercentageMaxHR = new GarminFitnessBool(true, Constants.HeartRateReferenceTCXString[1], Constants.HeartRateReferenceTCXString[0]);
    }
}
