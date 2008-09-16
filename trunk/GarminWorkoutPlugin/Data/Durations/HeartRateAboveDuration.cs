using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace GarminWorkoutPlugin.Data
{
    class HeartRateAboveDuration : IDuration
    {
        public HeartRateAboveDuration(IStep parent)
            : base(DurationType.HeartRateAbove, parent)
        {
            m_MaxHeartRate = 50;
            m_IsPercentageMaxHR = true;
        }

        public HeartRateAboveDuration(Byte maxHeartRate, bool isPercentageMaxHeartRate, IStep parent)
            : this(parent)
        {
            ValidateValue(maxHeartRate, isPercentageMaxHeartRate);

            m_MaxHeartRate = maxHeartRate;
            m_IsPercentageMaxHR = isPercentageMaxHeartRate;
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
                                    minValue = 1;
                                    maxValue = 100;
                                }
                                else
                                {
                                    minValue = 30;
                                    maxValue = 240;
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
                ValidateValue(value, m_IsPercentageMaxHR);
                m_MaxHeartRate = value;
            }
        }

        public bool IsPercentageMaxHeartRate
        {
            get { return m_IsPercentageMaxHR; }
            set
            {
                ValidateValue(m_MaxHeartRate, value);
                m_IsPercentageMaxHR = value;
            }
        }

        private void ValidateValue(Byte maxHeartRate, bool isPercentageMaxHeartRate)
        {
            if(isPercentageMaxHeartRate)
            {
                Trace.Assert(maxHeartRate >= 1 && maxHeartRate <= 100);
            }
            else
            {
                Trace.Assert(maxHeartRate>= 30 && maxHeartRate <= 240);
            }
        }

        private Byte m_MaxHeartRate;
        private bool m_IsPercentageMaxHR;
    }
}
