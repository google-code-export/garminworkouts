using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class HeartRateBelowDuration : IDuration
    {
        public HeartRateBelowDuration(IStep parent)
            : base(DurationType.HeartRateBelow, parent)
        {
        }

        public HeartRateBelowDuration(Byte minHeartRate, bool isPercentageMaxHeartRate, IStep parent)
            : this(parent)
        {
            ValidateValue(minHeartRate, isPercentageMaxHeartRate);

            m_IsPercentageMaxHR.Value = isPercentageMaxHeartRate;
            InternalMinHeartRate.Value = minHeartRate;
        }

        public HeartRateBelowDuration(Stream stream, DataVersion version, IStep parent)
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

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField durationType = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField durationValue = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationValue);

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.HeartRateLessThan);
            message.AddField(durationType);

            if (IsPercentageMaxHeartRate)
            {
                durationValue.SetUInt32((UInt32)MinHeartRate);
            }
            else
            {
                durationValue.SetUInt32((UInt32)MinHeartRate + 100);
            }
            message.AddField(durationValue);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);

            m_IsPercentageMaxHR.Deserialize(stream, version);
            InternalMinHeartRate.Deserialize(stream, version);
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

            InternalMinHeartRate.Serialize(childNode, Constants.ValueTCXString, document);
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
                        throw new GarminFitnessXmlDeserializationException("Invalid heart rate below duration XML node", child);
                    }

                    if (child.ChildNodes.Count != 1 || child.FirstChild.Name != Constants.ValueTCXString)
                    {
                        throw new GarminFitnessXmlDeserializationException("Missing information in heart rate below duration XML node", parentNode);
                    }

                    InternalMinHeartRate.Deserialize(child.FirstChild);
                }
            }
        }

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            step.SetDurationType(GarXFaceNet._Workout._Step.DurationTypes.HeartRateGreaterThan);

            if (IsPercentageMaxHeartRate)
            {
                step.SetDurationValue(InternalMinHeartRate);
            }
            else
            {
                step.SetDurationValue((UInt16)(InternalMinHeartRate + 100));
            }
        }

        public override void Deserialize(GarXFaceNet._Workout._Step step)
        {
            UInt16 duration = (UInt16)step.GetDurationValue();

            if (duration <= 100)
            {
                m_IsPercentageMaxHR.Value = true;
                m_MinHeartRatePercent.Value = (Byte)duration;
            }
            else
            {
                m_IsPercentageMaxHR.Value = false;
                m_MinHeartRateBPM.Value = (Byte)(duration - 100);
            }

            ValidateValue(MinHeartRate, IsPercentageMaxHeartRate);
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

        public bool IsPercentageMaxHeartRate
        {
            get { return m_IsPercentageMaxHR; }
            set
            {
                if (IsPercentageMaxHeartRate != value)
                {
                    m_IsPercentageMaxHR.Value = value;
                    ValidateValue(MinHeartRate, value);

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

        private GarminFitnessByteRange m_MinHeartRateBPM = new GarminFitnessByteRange(70, Constants.MinHRInBPM, Constants.MaxHRInBPM);
        private GarminFitnessByteRange m_MinHeartRatePercent = new GarminFitnessByteRange(70, Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax);
        private GarminFitnessBool m_IsPercentageMaxHR = new GarminFitnessBool(true, Constants.HeartRateReferenceTCXString[1], Constants.HeartRateReferenceTCXString[0]);
    }
}
