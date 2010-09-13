using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class HeartRateAboveDuration : IDuration
    {
        public HeartRateAboveDuration(IStep parent)
            : base(DurationType.HeartRateAbove, parent)
        {
        }

        public HeartRateAboveDuration(Byte maxHeartRate, bool isPercentageMaxHeartRate, IStep parent)
            : this(parent)
        {
            ValidateValue(maxHeartRate, isPercentageMaxHeartRate);

            MaxHeartRate = maxHeartRate;
            IsPercentageMaxHeartRate = isPercentageMaxHeartRate;
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

        public override void SerializetoFIT(Stream stream)
        {
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

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            step.SetDurationType(GarXFaceNet._Workout._Step.DurationTypes.HeartRateLessThan);

            if (IsPercentageMaxHeartRate)
            {
                step.SetDurationValue(MaxHeartRate);
            }
            else
            {
                step.SetDurationValue((UInt32)(MaxHeartRate + 100));
            }
        }

        public override void Deserialize(GarXFaceNet._Workout._Step step)
        {
            UInt16 duration = (UInt16)step.GetDurationValue();

            if (duration <= 100)
            {
                m_IsPercentageMaxHR.Value = true;
                m_MaxHeartRatePercent.Value = (Byte)duration;
            }
            else
            {
                m_IsPercentageMaxHR.Value = false;
                m_MaxHeartRateBPM.Value = (Byte)(duration - 100);
            }

            ValidateValue(MaxHeartRate, IsPercentageMaxHeartRate);
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
                    ValidateValue(value, IsPercentageMaxHeartRate);

                    InternalMaxHeartRate.Value = value;

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
                    ValidateValue(MaxHeartRate, value);
                    m_IsPercentageMaxHR.Value = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("IsPercentageMaxHeartRate"));
                }
            }
        }

        private GarminFitnessByteRange m_MaxHeartRateBPM = new GarminFitnessByteRange(50, Constants.MinHRInBPM, Constants.MaxHRInBPM);
        private GarminFitnessByteRange m_MaxHeartRatePercent = new GarminFitnessByteRange(50, Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax);
        private GarminFitnessBool m_IsPercentageMaxHR = new GarminFitnessBool(true, Constants.HeartRateReferenceTCXString[1], Constants.HeartRateReferenceTCXString[0]);
    }
}
