using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class PowerBelowDuration : IDuration
    {
        public PowerBelowDuration(IStep parent)
            : base(DurationType.PowerBelow, parent)
        {
        }

        public PowerBelowDuration(UInt16 minPower, bool isPercentFTP, IStep parent)
            : this(parent)
        {
            ValidateValue(minPower, isPercentFTP);

            m_IsPercentFTP.Value = isPercentFTP;
            InternalMinPower.Value = minPower;
        }

        public PowerBelowDuration(Stream stream, DataVersion version, IStep parent)
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

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField durationType = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField durationValue = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.DurationValue);

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.PowerLessThan);

            if (IsPercentFTP)
            {
                durationValue.SetUInt32((UInt32)MinPower);
            }
            else
            {
                durationValue.SetUInt32((UInt32)MinPower + 1000);
            }
        }

        public void Deserialize_V17(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);

            m_IsPercentFTP.Deserialize(stream, version);
            InternalMinPower.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            // Unsupported by TCX
            throw new NotSupportedException();
        }

        public override void Deserialize(XmlNode parentNode)
        {
            // Unsupported by TCX
            throw new NotSupportedException();
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
                    InternalMinPower.Value = value;
                    ValidateValue(value, IsPercentFTP);

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
