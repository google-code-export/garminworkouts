using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class PowerAboveDuration : IDuration
    {
        public PowerAboveDuration(IStep parent)
            : base(DurationType.PowerAbove, parent)
        {
        }

        public PowerAboveDuration(UInt16 maxPower, bool isPercentFTP, IStep parent)
            : this(parent)
        {
            ValidateValue(maxPower, isPercentFTP);

            MaxPower = maxPower;
            IsPercentFTP = isPercentFTP;
        }

        public PowerAboveDuration(Stream stream, DataVersion version, IStep parent)
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

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField durationType = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField durationValue = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationValue);

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.PowerGreaterThan);
            message.AddField(durationType);

            if (IsPercentFTP)
            {
                durationValue.SetUInt32((UInt32)MaxPower);
            }
            else
            {
                durationValue.SetUInt32((UInt32)MaxPower + 1000);
            }
            message.AddField(durationValue);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);

            m_IsPercentFTP.Deserialize(stream, version);
            InternalMaxPower.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            // Unsupported by TCX
            Debug.Assert(false);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            // Unsupported by TCX
            Debug.Assert(false);
        }

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            // Unsupported by USB
            Debug.Assert(false);
        }

        public override void Deserialize(GarXFaceNet._Workout._Step step)
        {
            // Unsupported by USB
            Debug.Assert(false);
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
                    ValidateValue(value, IsPercentFTP);

                    InternalMaxPower.Value = value;

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
                    ValidateValue(MaxPower, value);
                    m_IsPercentFTP.Value = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("IsPercentFTP"));
                }
            }
        }

        private GarminFitnessUInt16Range m_MaxPowerWatts = new GarminFitnessUInt16Range(250, Constants.MinPowerInWatts, Constants.MaxPowerWorkoutInWatts);
        private GarminFitnessUInt16Range m_MaxPowerPercent = new GarminFitnessUInt16Range(90, Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTP);
        private GarminFitnessBool m_IsPercentFTP = new GarminFitnessBool(false, Constants.PowerReferenceTCXString[1], Constants.PowerReferenceTCXString[0]);
    }
}
