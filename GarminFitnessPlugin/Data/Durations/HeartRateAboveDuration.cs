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
            MaxHeartRate = 50;
            IsPercentageMaxHeartRate = true;
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

            stream.Write(BitConverter.GetBytes(IsPercentageMaxHeartRate), 0, sizeof(bool));
            stream.Write(BitConverter.GetBytes(MaxHeartRate), 0, sizeof(Byte));
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);

            byte[] byteBuffer = new byte[sizeof(Byte)];
            byte[] boolBuffer = new byte[sizeof(bool)];

            stream.Read(boolBuffer, 0, sizeof(bool));
            IsPercentageMaxHeartRate = BitConverter.ToBoolean(boolBuffer, 0);

            stream.Read(byteBuffer, 0, sizeof(Byte));
            MaxHeartRate = byteBuffer[0];
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);

            XmlNode childNode;
            XmlNode valueNode;
            XmlAttribute attribute;

            childNode = document.CreateElement("HeartRate");

            // Type
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = Constants.HeartRateReferenceTCXString[IsPercentageMaxHeartRate ? 1 : 0];
            childNode.Attributes.Append(attribute);

            // Value
            valueNode = document.CreateElement("Value");
            valueNode.AppendChild(document.CreateTextNode(MaxHeartRate.ToString()));
            childNode.AppendChild(valueNode);

            parentNode.AppendChild(childNode);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                if (parentNode.ChildNodes.Count == 1 && parentNode.FirstChild.Name == "HeartRate")
                {
                    XmlNode child = parentNode.FirstChild;

                    if (child.Attributes.Count == 1 && child.Attributes[0].Name == "xsi:type")
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
                            return false;
                        }

                        if (child.ChildNodes.Count == 1 && child.FirstChild.Name == "Value")
                        {
                            XmlNode valueNode = child.FirstChild;

                            if (valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                            {
                                Byte minValue;
                                Byte maxValue;

                                if (IsPercentageMaxHeartRate)
                                {
                                    minValue = Constants.MinHRInPercentMax;
                                    maxValue = Constants.MaxHRInPercentMax;
                                }
                                else
                                {
                                    minValue = Constants.MinHRInBPM;
                                    maxValue = Constants.MaxHRInBPM;
                                }

                                if (Utils.IsTextIntegerInRange(valueNode.FirstChild.Value, minValue, maxValue))
                                {
                                    MaxHeartRate = Byte.Parse(valueNode.FirstChild.Value);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public Byte MaxHeartRate
        {
            get { return m_MaxHeartRate; }
            set
            {
                if (MaxHeartRate != value)
                {
                    ValidateValue(value, IsPercentageMaxHeartRate);
                    m_MaxHeartRate = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("MaxHeartRate"));
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
                    m_IsPercentageMaxHR = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("IsPercentageMaxHeartRate"));
                }
            }
        }

        private void ValidateValue(Byte maxHeartRate, bool isPercentageMaxHeartRate)
        {
            if(isPercentageMaxHeartRate)
            {
                Trace.Assert(maxHeartRate >= Constants.MinHRInPercentMax && maxHeartRate <= Constants.MaxHRInPercentMax);
            }
            else
            {
                Trace.Assert(maxHeartRate >= Constants.MinHRInBPM && maxHeartRate <= Constants.MaxHRInBPM);
            }
        }

        private Byte m_MaxHeartRate;
        private bool m_IsPercentageMaxHR;
    }
}
