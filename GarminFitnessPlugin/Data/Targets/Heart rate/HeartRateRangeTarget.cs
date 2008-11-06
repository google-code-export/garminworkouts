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
            m_MinHeartRate = 80;
            m_MaxHeartRate = 90;
            m_IsPercentageMaxHeartRate = true;
        }

        public HeartRateRangeTarget(Byte minHR, Byte maxHR, bool isPercentMax, BaseHeartRateTarget baseTarget)
            : this(baseTarget)
        {
            ValidateValue(minHR, maxHR, isPercentMax);
         
            m_MinHeartRate = minHR;
            m_MaxHeartRate = maxHR;
            m_IsPercentageMaxHeartRate = isPercentMax;
        }

        public HeartRateRangeTarget(Stream stream, DataVersion version, BaseHeartRateTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.WriteByte(MinHeartRate);
            stream.WriteByte(MaxHeartRate);
            stream.Write(BitConverter.GetBytes(IsPercentageMaxHeartRate), 0, sizeof(bool));
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseHeartRateTarget.IConcreteHeartRateTarget), stream, version);

            byte[] boolBuffer = new byte[sizeof(bool)];
            byte min;
            byte max;
            bool isPercentMax;

            min = (Byte)stream.ReadByte();
            max = (Byte)stream.ReadByte();

            stream.Read(boolBuffer, 0, sizeof(bool));
            isPercentMax = BitConverter.ToBoolean(boolBuffer, 0);

            SetValues(min, max, isPercentMax);
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);

            XmlAttribute attribute;
            XmlNode childNode;
            XmlNode valueNode;

            // Type
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = "CustomHeartRateZone_t";
            parentNode.Attributes.Append(attribute);

            // Low
            childNode = document.CreateElement("Low");
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = Constants.HeartRateReferenceTCXString[IsPercentageMaxHeartRate ? 1 : 0];
            childNode.Attributes.Append(attribute);
            valueNode = document.CreateElement("Value");
            valueNode.AppendChild(document.CreateTextNode(MinHeartRate.ToString()));
            childNode.AppendChild(valueNode);
            parentNode.AppendChild(childNode);

            // High
            childNode = document.CreateElement("High");
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = Constants.HeartRateReferenceTCXString[IsPercentageMaxHeartRate ? 1 : 0];
            childNode.Attributes.Append(attribute);
            valueNode = document.CreateElement("Value");
            valueNode.AppendChild(document.CreateTextNode(MaxHeartRate.ToString()));
            childNode.AppendChild(valueNode);
            parentNode.AppendChild(childNode);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                int min = -1;
                int max = -1;
                bool isPercentMax = false;
                int isPercentMaxReadCount = 0;

                for(int i = 0; i < parentNode.ChildNodes.Count; ++i)
                {
                    XmlNode child = parentNode.ChildNodes[i];

                    if (child.Name == "Low" && child.ChildNodes.Count == 1 &&
                        child.FirstChild.Name == "Value")
                    {
                        if (child.Attributes.Count == 1 && child.Attributes[0].Name == "xsi:type")
                        {
                            bool percentRead = child.Attributes[0].Value == Constants.HeartRateReferenceTCXString[1];

                            if (isPercentMaxReadCount > 0 && percentRead != isPercentMax)
                            {
                                return false;
                            }
                            else
                            {
                                isPercentMaxReadCount++;
                                isPercentMax = percentRead;
                            }
                        }

                        XmlNode valueNode = child.FirstChild;

                        if (valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                        {
                            if ((isPercentMax && Utils.IsTextIntegerInRange(valueNode.FirstChild.Value, 0, 100)) ||
                                (!isPercentMax && Utils.IsTextIntegerInRange(valueNode.FirstChild.Value, 30, 240)))
                            {
                                min = Byte.Parse(valueNode.FirstChild.Value);
                            }
                        }
                    }
                    else if (child.Name == "High" && child.ChildNodes.Count == 1 &&
                        child.FirstChild.Name == "Value")
                    {
                        if (child.Attributes.Count == 1 && child.Attributes[0].Name == "xsi:type")
                        {
                            bool percentRead = child.Attributes[0].Value == Constants.HeartRateReferenceTCXString[1];

                            if (isPercentMaxReadCount > 0 && percentRead != isPercentMax)
                            {
                                return false;
                            }
                            else
                            {
                                isPercentMaxReadCount++;
                                isPercentMax = percentRead;
                            }
                        }

                        XmlNode valueNode = child.FirstChild;

                        if (valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                        {
                            if ((isPercentMax && Utils.IsTextIntegerInRange(valueNode.FirstChild.Value, 0, 100)) ||
                                (!isPercentMax && Utils.IsTextIntegerInRange(valueNode.FirstChild.Value, 30, 240)))
                            {
                                max = Byte.Parse(valueNode.FirstChild.Value);
                            }
                        }
                    }
                }

                if(isPercentMaxReadCount == 2 && min != -1 && max != -1)
                {
                    // Make sure min and max are in the right order, GTC doesn't enforce
                    if (min > max)
                    {
                        SetValues((Byte)max, (Byte)min, isPercentMax);
                    }
                    else
                    {
                        SetValues((Byte)min, (Byte)max, isPercentMax);
                    }

                    return true;
                }
            }

            return false;
        }

        public void SetMinHeartRate(Byte minHeartRate)
        {
            ValidateValue(minHeartRate, MaxHeartRate, IsPercentageMaxHeartRate);

            MinHeartRate = minHeartRate;
        }

        public void SetMaxHeartRate(Byte maxHeartRate)
        {
            ValidateValue(MinHeartRate, maxHeartRate, IsPercentageMaxHeartRate);

            MaxHeartRate = maxHeartRate;
        }

        public void SetValues(Byte minHeartRate, Byte maxHeartRate, bool isPercentMax)
        {
            ValidateValue(minHeartRate, maxHeartRate, isPercentMax);

            MinHeartRate = minHeartRate;
            MaxHeartRate = maxHeartRate;
            IsPercentageMaxHeartRate = isPercentMax;
        }

        private void ValidateValue(Byte minHeartRate, Byte maxHeartRate, bool isPercentageMaxHeartRate)
        {
            if (isPercentageMaxHeartRate)
            {
                Trace.Assert(minHeartRate >= 0 && minHeartRate <= 100);
                Trace.Assert(maxHeartRate >= 0 && maxHeartRate <= 100);
            }
            else
            {
                Trace.Assert(minHeartRate >= 30 && minHeartRate <= 240);
                Trace.Assert(maxHeartRate >= 30 && maxHeartRate <= 240);
            }

            Trace.Assert(minHeartRate <= maxHeartRate);
        }

        public bool IsPercentageMaxHeartRate
        {
            get { return m_IsPercentageMaxHeartRate; }
            set
            {
                if (IsPercentageMaxHeartRate != value)
                {
                    ValidateValue(MinHeartRate, MaxHeartRate, value);
                    m_IsPercentageMaxHeartRate = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("IsPercentageMaxHeartRate"));
                }
            }
        }

        public Byte MinHeartRate
        {
            get { return m_MinHeartRate; }
            private set
            {
                if (MinHeartRate != value)
                {
                    m_MinHeartRate = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MinHeartRate"));
                }
            }
        }

        public Byte MaxHeartRate
        {
            get { return m_MaxHeartRate; }
            private set
            {
                if (m_MaxHeartRate != value)
                {
                    m_MaxHeartRate = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MaxHeartRate"));
                }
            }
        }

        public override bool IsDirty
        {
            get { return false; }
            set { Trace.Assert(false); }
        }

        private Byte m_MinHeartRate;
        private Byte m_MaxHeartRate;
        private bool m_IsPercentageMaxHeartRate;
    }
}
