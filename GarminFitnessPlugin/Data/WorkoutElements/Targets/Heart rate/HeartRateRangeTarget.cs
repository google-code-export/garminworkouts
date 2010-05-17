using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class HeartRateRangeTarget : BaseHeartRateTarget.IConcreteHeartRateTarget
    {
        public HeartRateRangeTarget(BaseHeartRateTarget baseTarget)
            : base(HeartRateTargetType.Range, baseTarget)
        {
        }

        public HeartRateRangeTarget(Byte minHR, Byte maxHR, bool isPercentMax, BaseHeartRateTarget baseTarget)
            : this(baseTarget)
        {
            SetValues(minHR, maxHR, isPercentMax);
        }

        public HeartRateRangeTarget(Stream stream, DataVersion version, BaseHeartRateTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_IsPercentMaxHeartRate.Serialize(stream);
            InternalMinHeartRate.Serialize(stream);
            InternalMaxHeartRate.Serialize(stream);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseHeartRateTarget.IConcreteHeartRateTarget), stream, version);

            byte min;
            byte max;

            min = (Byte)stream.ReadByte();
            max = (Byte)stream.ReadByte();

            m_IsPercentMaxHeartRate.Deserialize(stream, version);

            SetValues(min, max, IsPercentMaxHeartRate);
        }

        public void Deserialize_V10(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseHeartRateTarget.IConcreteHeartRateTarget), stream, version);

            // Inverted the data order in V10 so we can use the Internal... functions
            m_IsPercentMaxHeartRate.Deserialize(stream, version);
            InternalMinHeartRate.Deserialize(stream, version);
            InternalMaxHeartRate.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            // This node was added by our parent...
            parentNode = parentNode.LastChild;

            XmlAttribute attribute;
            XmlNode childNode;

            // Type
            attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            attribute.Value = "CustomHeartRateZone_t";
            parentNode.Attributes.Append(attribute);

            // Low
            childNode = document.CreateElement("Low");
            parentNode.AppendChild(childNode);
            m_IsPercentMaxHeartRate.SerializeAttribute(childNode, Constants.XsiTypeTCXString, Constants.xsins, document);
            InternalMinHeartRate.Serialize(childNode, Constants.ValueTCXString, document);

            // High
            childNode = document.CreateElement("High");
            parentNode.AppendChild(childNode);
            m_IsPercentMaxHeartRate.SerializeAttribute(childNode, Constants.XsiTypeTCXString, Constants.xsins, document);
            InternalMaxHeartRate.Serialize(childNode, Constants.ValueTCXString, document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            bool isPercentMax = false;
            int isPercentMaxReadCount = 0;
            bool minRead = false;
            bool maxRead = false;

            for(int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode child = parentNode.ChildNodes[i];

                if (child.Name == "Low" && child.ChildNodes.Count == 1 &&
                    child.FirstChild.Name == Constants.ValueTCXString)
                {
                    if (child.Attributes.Count != 1 && child.Attributes[0].Name != Constants.XsiTypeTCXString)
                    {
                        throw new GarminFitnessXmlDeserializationException("Invalid heart rate range attribute in XML node", parentNode);
                    }

                    bool percentRead = m_IsPercentMaxHeartRate.GetTextValue(child.Attributes[0].Value);

                    if (isPercentMaxReadCount > 0 && percentRead != isPercentMax)
                    {
                        throw new GarminFitnessXmlDeserializationException("Inconsistent heart rate range attribute in XML node", parentNode);
                    }

                    isPercentMaxReadCount++;
                    isPercentMax = percentRead;

                    if (isPercentMax)
                    {
                        m_MinHeartRatePercent.Deserialize(child.FirstChild);
                    }
                    else
                    {
                        m_MinHeartRateBPM.Deserialize(child.FirstChild);
                    }
                    minRead = true;
                }
                else if (child.Name == "High" && child.ChildNodes.Count == 1 &&
                    child.FirstChild.Name == Constants.ValueTCXString)
                {
                    if (child.Attributes.Count != 1 && child.Attributes[0].Name != Constants.XsiTypeTCXString)
                    {
                        throw new GarminFitnessXmlDeserializationException("Invalid heart rate range attribute in XML node", parentNode);
                    }

                    bool percentRead = m_IsPercentMaxHeartRate.GetTextValue(child.Attributes[0].Value);

                    if (isPercentMaxReadCount > 0 && percentRead != isPercentMax)
                    {
                        throw new GarminFitnessXmlDeserializationException("Inconsistent heart rate range attribute in XML node", parentNode);
                    }

                    isPercentMaxReadCount++;
                    isPercentMax = percentRead;

                    if (isPercentMax)
                    {
                        m_MaxHeartRatePercent.Deserialize(child.FirstChild);
                    }
                    else
                    {
                        m_MaxHeartRateBPM.Deserialize(child.FirstChild);
                    }
                    maxRead = true;
                }
            }

            if(isPercentMaxReadCount != 2 || !minRead || !maxRead)
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in heart rate range target XML node", parentNode);
            }

            // Make sure min and max are in the right order, GTC doesn't enforce
            IsPercentMaxHeartRate = isPercentMax;
            SetValues(Math.Min(InternalMinHeartRate, InternalMaxHeartRate), Math.Max(InternalMinHeartRate, InternalMaxHeartRate), isPercentMax);
        }

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            step.SetTargetType(1);
            step.SetTargetValue(0);

            if (IsPercentMaxHeartRate)
            {
                step.SetTargetCustomZoneLow(MinHeartRate);
                step.SetTargetCustomZoneHigh(MaxHeartRate);
            }
            else
            {
                step.SetTargetCustomZoneLow(MinHeartRate + 100);
                step.SetTargetCustomZoneHigh(MaxHeartRate + 100);
            }
        }

        public override void Deserialize(GarXFaceNet._Workout._Step step)
        {
            float minHR = step.GetTargetCustomZoneLow();
            float maxHR = step.GetTargetCustomZoneHigh();

            if (minHR <= 100 && maxHR <= 100)
            {
                m_IsPercentMaxHeartRate.Value = true;
                m_MinHeartRatePercent.Value = (Byte)minHR;
                m_MaxHeartRatePercent.Value = (Byte)maxHR;
            }
            else if (minHR > 100 && maxHR > 100)
            {
                m_IsPercentMaxHeartRate.Value = false;
                m_MinHeartRateBPM.Value = (Byte)(minHR - 100);
                m_MaxHeartRateBPM.Value = (Byte)(maxHR - 100);
            }
            else
            {
                Debug.Assert(false, "both min & max cadence should be either in percent max or BPM, cannot mix & match");
            }

            ValidateValue(MinHeartRate, MaxHeartRate, IsPercentMaxHeartRate);
        }

        public void SetMinHeartRate(Byte minHeartRate)
        {
            ValidateValue(minHeartRate, MaxHeartRate, IsPercentMaxHeartRate);

            MinHeartRate = minHeartRate;
        }

        public void SetMaxHeartRate(Byte maxHeartRate)
        {
            ValidateValue(MinHeartRate, maxHeartRate, IsPercentMaxHeartRate);

            MaxHeartRate = maxHeartRate;
        }

        public void SetValues(Byte minHeartRate, Byte maxHeartRate, bool isPercentMax)
        {
            ValidateValue(minHeartRate, maxHeartRate, isPercentMax);

            IsPercentMaxHeartRate = isPercentMax;
            MinHeartRate = minHeartRate;
            MaxHeartRate = maxHeartRate;
        }

        private void ValidateValue(Byte minHeartRate, Byte maxHeartRate, bool isPercentageMaxHeartRate)
        {
            if (isPercentageMaxHeartRate)
            {
                Debug.Assert(m_MinHeartRatePercent.IsInRange(minHeartRate));
                Debug.Assert(m_MaxHeartRatePercent.IsInRange(maxHeartRate));
            }
            else
            {
                Debug.Assert(m_MinHeartRateBPM.IsInRange(minHeartRate));
                Debug.Assert(m_MaxHeartRateBPM.IsInRange(maxHeartRate));
            }

            Debug.Assert(minHeartRate <= maxHeartRate);
        }

        public bool IsPercentMaxHeartRate
        {
            get { return m_IsPercentMaxHeartRate; }
            private set
            {
                if (IsPercentMaxHeartRate != value)
                {
                    m_IsPercentMaxHeartRate.Value = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("IsPercentageMaxHeartRate"));
                }
            }
        }

        public Byte MinHeartRate
        {
            get { return InternalMinHeartRate; }
            private set
            {
                if (MinHeartRate != value)
                {
                    InternalMinHeartRate.Value = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MinHeartRate"));
                }
            }
        }

        public Byte MaxHeartRate
        {
            get { return InternalMaxHeartRate; }
            private set
            {
                if (MaxHeartRate != value)
                {
                    InternalMaxHeartRate.Value = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MaxHeartRate"));
                }
            }
        }

        private GarminFitnessByteRange InternalMinHeartRate
        {
            get
            {
                if (IsPercentMaxHeartRate)
                {
                    return m_MinHeartRatePercent;
                }
                else
                {
                    return m_MinHeartRateBPM;
                }
            }
        }

        private GarminFitnessByteRange InternalMaxHeartRate
        {
            get
            {
                if (IsPercentMaxHeartRate)
                {
                    return m_MaxHeartRatePercent;
                }
                else
                {
                    return m_MaxHeartRateBPM;
                }
            }
        }

        public override bool IsDirty
        {
            get { return false; }
            set { Debug.Assert(false); }
        }

        private GarminFitnessByteRange m_MinHeartRatePercent = new GarminFitnessByteRange(80, Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax);
        private GarminFitnessByteRange m_MaxHeartRatePercent = new GarminFitnessByteRange(90, Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax);
        private GarminFitnessByteRange m_MinHeartRateBPM = new GarminFitnessByteRange(80, Constants.MinHRInBPM, Constants.MaxHRInBPM);
        private GarminFitnessByteRange m_MaxHeartRateBPM = new GarminFitnessByteRange(90, Constants.MinHRInBPM, Constants.MaxHRInBPM);
        private GarminFitnessBool m_IsPercentMaxHeartRate = new GarminFitnessBool(true, Constants.HeartRateReferenceTCXString[1], Constants.HeartRateReferenceTCXString[0]);
    }
}
