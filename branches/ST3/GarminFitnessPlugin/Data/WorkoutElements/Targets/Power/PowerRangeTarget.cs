using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class PowerRangeTarget : BasePowerTarget.IConcretePowerTarget
    {
        public PowerRangeTarget(BasePowerTarget baseTarget)
            : base(PowerTargetType.Range, baseTarget)
        {
        }

        public PowerRangeTarget(Byte minPower, Byte maxPower,
                                bool isPercentFTP, BasePowerTarget baseTarget)
            : this(baseTarget)
        {
            SetValues(minPower, maxPower, isPercentFTP);
        }

        public PowerRangeTarget(Stream stream, DataVersion version, BasePowerTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_IsPercentFTP.Serialize(stream);
            InternalMinPower.Serialize(stream);
            InternalMaxPower.Serialize(stream);
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField powerZone = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);
            FITMessageField minPower = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueLow);
            FITMessageField maxPower = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueHigh);

            powerZone.SetUInt32((Byte)0);
            message.AddField(powerZone);

            if (IsPercentFTP)
            {
                minPower.SetUInt32((UInt32)MinPower);
                maxPower.SetUInt32((UInt32)MaxPower);
            }
            else
            {
                minPower.SetUInt32((UInt32)MinPower + 1000);
                maxPower.SetUInt32((UInt32)MaxPower + 1000);
            }

            message.AddField(minPower);
            message.AddField(maxPower);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BasePowerTarget.IConcretePowerTarget), stream, version);

            m_MinPowerWatts.Deserialize(stream, version);
            m_MaxPowerWatts.Deserialize(stream, version);
        }

        public void Deserialize_V18(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BasePowerTarget.IConcretePowerTarget), stream, version);

            // Inverted the data order in V10 so we can use the Internal... functions
            m_IsPercentFTP.Deserialize(stream, version);
            InternalMinPower.Deserialize(stream, version);
            InternalMaxPower.Deserialize(stream, version);
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
            attribute.Value = "CustomPowerZone_t";
            parentNode.Attributes.Append(attribute);

            // Low
            childNode = document.CreateElement("Low");
            parentNode.AppendChild(childNode);
            m_IsPercentFTP.SerializeAttribute(childNode, Constants.XsiTypeTCXString, Constants.xsins, document);
            m_MinPowerWatts.Serialize(childNode, Constants.ValueTCXString, document);

            // High
            childNode = document.CreateElement("High");
            parentNode.AppendChild(childNode);
            m_IsPercentFTP.SerializeAttribute(childNode, Constants.XsiTypeTCXString, Constants.xsins, document);
            m_MaxPowerWatts.Serialize(childNode, Constants.ValueTCXString, document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            bool isPercentFTP = false;
            int isPercentFTPReadCount = 0;
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

                    bool percentRead = m_IsPercentFTP.GetTextValue(child.Attributes[0].Value);

                    if (isPercentFTPReadCount > 0 && percentRead != isPercentFTP)
                    {
                        throw new GarminFitnessXmlDeserializationException("Inconsistent heart rate range attribute in XML node", parentNode);
                    }

                    isPercentFTPReadCount++;
                    isPercentFTP = percentRead;

                    if (isPercentFTP)
                    {
                        m_MinPowerPercent.Deserialize(child.FirstChild);
                    }
                    else
                    {
                        m_MinPowerWatts.Deserialize(child.FirstChild);
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

                    bool percentRead = m_IsPercentFTP.GetTextValue(child.Attributes[0].Value);

                    if (isPercentFTPReadCount > 0 && percentRead != isPercentFTP)
                    {
                        throw new GarminFitnessXmlDeserializationException("Inconsistent heart rate range attribute in XML node", parentNode);
                    }

                    isPercentFTPReadCount++;
                    isPercentFTP = percentRead;

                    if (isPercentFTP)
                    {
                        m_MaxPowerPercent.Deserialize(child.FirstChild);
                    }
                    else
                    {
                        m_MaxPowerWatts.Deserialize(child.FirstChild);
                    }
                    maxRead = true;
                }
            }

            if (isPercentFTPReadCount != 2 || !minRead || !maxRead)
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in power range target XML node", parentNode);
            }

            SetValues(Math.Min(MinPower, MaxPower), Math.Max(MinPower, MaxPower), isPercentFTP);
        }

        public void SetMinPower(UInt16 minPower)
        {
            ValidateValue(minPower, MaxPower, IsPercentFTP);

            MinPower = minPower;
        }

        public void SetMaxPower(UInt16 maxPower)
        {
            ValidateValue(MinPower, maxPower, IsPercentFTP);

            MaxPower = maxPower;
        }

        public void SetValues(UInt16 minPower, UInt16 maxPower, bool isPercentFTP)
        {
            ValidateValue(minPower, maxPower, isPercentFTP);

            IsPercentFTP = isPercentFTP;
            MinPower = minPower;
            MaxPower = maxPower;
        }

        private void ValidateValue(UInt16 minPower, UInt16 maxPower, bool isPercentFTP)
        {
            if (IsPercentFTP)
            {
                Debug.Assert(m_MinPowerPercent.IsInRange(minPower));
                Debug.Assert(m_MaxPowerPercent.IsInRange(maxPower));
            }
            else
            {
                Debug.Assert(m_MinPowerWatts.IsInRange(minPower));
                Debug.Assert(m_MaxPowerWatts.IsInRange(maxPower));
            }

            Debug.Assert(minPower <= maxPower);
        }

        public bool IsPercentFTP
        {
            get { return m_IsPercentFTP; }
            private set
            {
                if (IsPercentFTP != value)
                {
                    m_IsPercentFTP.Value = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("IsPercentFTP"));
                }
            }
        }

        public UInt16 MinPower
        {
            get { return InternalMinPower; }
            private set
            {
                if (MinPower != value)
                {
                    InternalMinPower.Value = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MinPower"));
                }
            }
        }

        public UInt16 MaxPower
        {
            get { return InternalMaxPower; }
            private set
            {
                if (MaxPower != value)
                {
                    InternalMaxPower.Value = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MaxPower"));
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

        public override bool IsDirty
        {
            get { return false; }
            set { Debug.Assert(false); }
        }

        private GarminFitnessUInt16Range m_MinPowerPercent = new GarminFitnessUInt16Range(70, Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTP);
        private GarminFitnessUInt16Range m_MaxPowerPercent = new GarminFitnessUInt16Range(90, Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTP);
        private GarminFitnessUInt16Range m_MinPowerWatts = new GarminFitnessUInt16Range(75, Constants.MinPowerInWatts, Constants.MaxPowerWorkout);
        private GarminFitnessUInt16Range m_MaxPowerWatts = new GarminFitnessUInt16Range(165, Constants.MinPowerInWatts, Constants.MaxPowerWorkout);
        private GarminFitnessBool m_IsPercentFTP = new GarminFitnessBool(false, Constants.PowerReferenceTCXString[1], Constants.PowerReferenceTCXString[0]);
    }
}
