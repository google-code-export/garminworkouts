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
            SetValues(75, 165);
        }

        public PowerRangeTarget(Byte minPower, Byte maxPower, BasePowerTarget baseTarget)
            : this(baseTarget)
        {
            SetValues(minPower, maxPower);
        }

        public PowerRangeTarget(Stream stream, DataVersion version, BasePowerTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.Write(BitConverter.GetBytes(MinPower), 0, sizeof(UInt16));
            stream.Write(BitConverter.GetBytes(MaxPower), 0, sizeof(UInt16));
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BasePowerTarget.IConcretePowerTarget), stream, version);

            byte[] intBuffer = new byte[sizeof(UInt16)];
            UInt16 min;
            UInt16 max;

            stream.Read(intBuffer, 0, sizeof(UInt16));
            min = BitConverter.ToUInt16(intBuffer, 0);

            stream.Read(intBuffer, 0, sizeof(UInt16));
            max = BitConverter.ToUInt16(intBuffer, 0);

            SetValues(min, max);
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);

            XmlAttribute attribute;
            XmlNode childNode;
            XmlNode valueNode;

            // Type
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = "CustomPowerZone_t";
            parentNode.Attributes.Append(attribute);

            // Low
            childNode = document.CreateElement("Low");
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = "PowerInWatts_t";
            childNode.Attributes.Append(attribute);
            valueNode = document.CreateElement(Constants.ValueTCXString);
            valueNode.AppendChild(document.CreateTextNode(MinPower.ToString()));
            childNode.AppendChild(valueNode);
            parentNode.AppendChild(childNode);

            // High
            childNode = document.CreateElement("High");
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = "PowerInWatts_t";
            childNode.Attributes.Append(attribute);
            valueNode = document.CreateElement(Constants.ValueTCXString);
            valueNode.AppendChild(document.CreateTextNode(MaxPower.ToString()));
            childNode.AppendChild(valueNode);
            parentNode.AppendChild(childNode);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                int min = -1;
                int max = -1;

                for(int i = 0; i < parentNode.ChildNodes.Count; ++i)
                {
                    XmlNode child = parentNode.ChildNodes[i];

                    if (child.Name == "Low" && child.ChildNodes.Count == 1 &&
                        child.FirstChild.Name == Constants.ValueTCXString)
                    {
                        XmlNode valueNode = child.FirstChild;

                        if (valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                        {
                            if (Utils.IsTextIntegerInRange(valueNode.FirstChild.Value, Constants.MinPower, Constants.MaxPowerWorkout))
                            {
                                min = UInt16.Parse(valueNode.FirstChild.Value);
                            }
                        }
                    }
                    else if (child.Name == "High" && child.ChildNodes.Count == 1 &&
                        child.FirstChild.Name == Constants.ValueTCXString)
                    {
                        XmlNode valueNode = child.FirstChild;

                        if (valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                        {
                            if (Utils.IsTextIntegerInRange(valueNode.FirstChild.Value, Constants.MinPower, Constants.MaxPowerWorkout))
                            {
                                max = UInt16.Parse(valueNode.FirstChild.Value);
                            }
                        }
                    }
                }

                if(min != -1 && max != -1)
                {
                    // Make sure min and max are in the right order, GTC doesn't enforce
                    if (min > max)
                    {
                        SetValues((UInt16)max, (UInt16)min);
                    }
                    else
                    {
                        SetValues((UInt16)min, (UInt16)max);
                    }

                    return true;
                }
            }

            return false;
        }

        public void SetMinPower(UInt16 minPower)
        {
            ValidateValue(minPower, MaxPower);

            MinPower = minPower;
        }

        public void SetMaxPower(UInt16 maxPower)
        {
            ValidateValue(MinPower, maxPower);

            MaxPower = maxPower;
        }

        public void SetValues(UInt16 minPower, UInt16 maxPower)
        {
            ValidateValue(minPower, maxPower);

            MinPower = minPower;
            MaxPower = maxPower;
        }

        private void ValidateValue(UInt16 minPower, UInt16 maxPower)
        {
            Debug.Assert(minPower >= Constants.MinPower && minPower <= Constants.MaxPowerWorkout);
            Debug.Assert(maxPower >= Constants.MinPower && maxPower <= Constants.MaxPowerWorkout);
            Debug.Assert(minPower <= maxPower);
        }

        public UInt16 MinPower
        {
            get { return m_MinPower; }
            private set
            {
                if (MinPower != value)
                {
                    m_MinPower = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MinPower"));
                }
            }
        }

        public UInt16 MaxPower
        {
            get { return m_MaxPower; }
            private set
            {
                if (MaxPower != value)
                {
                    m_MaxPower = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MaxPower"));
                }
            }
        }

        public override bool IsDirty
        {
            get { return false; }
            set { Debug.Assert(false); }
        }

        private UInt16 m_MinPower;
        private UInt16 m_MaxPower;
    }
}
