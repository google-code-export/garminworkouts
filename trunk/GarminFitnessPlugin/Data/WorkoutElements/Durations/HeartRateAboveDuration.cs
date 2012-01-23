using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class HeartRateAboveDuration : IDuration
    {
        public HeartRateAboveDuration(IStep parent)
            : base(DurationType.HeartRateAbove, parent)
        {
        }

        public HeartRateAboveDuration(Byte maxHeartRate, bool isPercentageMaxHeartRate, IStep parent)
            : this(parent)
        {
            ValidateValue(maxHeartRate, isPercentageMaxHeartRate);

            m_IsPercentageMaxHR.Value = isPercentageMaxHeartRate;
            InternalMaxHeartRate.Value = maxHeartRate;
        }

        public HeartRateAboveDuration(Stream stream, DataVersion version, IStep parent)
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

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField durationType = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField durationValue = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationValue);

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.HeartRateGreaterThan);
            message.AddField(durationType);

            if (IsPercentageMaxHeartRate)
            {
                durationValue.SetUInt32((UInt32)MaxHeartRate);
            }
            else
            {
                durationValue.SetUInt32((UInt32)MaxHeartRate + 100);
            }
            message.AddField(durationValue);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);

            m_IsPercentageMaxHR.Deserialize(stream, version);
            InternalMaxHeartRate.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            // This node was added by our parent...
            parentNode = parentNode.LastChild;

            XmlNode childNode;

            childNode = document.CreateElement("HeartRate");
            parentNode.AppendChild(childNode);

            m_IsPercentageMaxHR.SerializeAttribute(childNode, Constants.XsiTypeTCXString, Constants.xsins, document);

            InternalMaxHeartRate.Serialize(childNode, Constants.ValueTCXString, document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            if (parentNode.ChildNodes.Count == 1 && parentNode.FirstChild.Name == "HeartRate")
            {
                XmlNode child = parentNode.FirstChild;

                if (child.Attributes.Count == 1 && child.Attributes[0].Name == Constants.XsiTypeTCXString)
                {
                    if (child.Attributes[0].Value == Constants.HeartRateReferenceTCXString[0])
                    {
                        IsPercentageMaxHeartRate = false;
                    }
                    else if (child.Attributes[0].Value == Constants.HeartRateReferenceTCXString[1])
                    {
                        IsPercentageMaxHeartRate = true;
                    }
                    else
                    {
                        throw new GarminFitnessXmlDeserializationException("Invalid heart rate above duration XML node", child);
                    }

                    if (child.ChildNodes.Count != 1 || child.FirstChild.Name != Constants.ValueTCXString)
                    {
                        throw new GarminFitnessXmlDeserializationException("Missing information in heart rate above duration XML node", parentNode);
                    }

                    InternalMaxHeartRate.Deserialize(child.FirstChild);
                }
            }
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

        private GarminFitnessByteRange m_MaxHeartRateBPM = new GarminFitnessByteRange(50, Constants.MinHRInBPM, Constants.MaxHRInBPM);
        private GarminFitnessByteRange m_MaxHeartRatePercent = new GarminFitnessByteRange(50, Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax);
        private GarminFitnessBool m_IsPercentageMaxHR = new GarminFitnessBool(true, Constants.HeartRateReferenceTCXString[1], Constants.HeartRateReferenceTCXString[0]);
    }
}
