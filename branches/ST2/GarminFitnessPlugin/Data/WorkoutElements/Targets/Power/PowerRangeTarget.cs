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

            m_MinPower.Serialize(stream);
            m_MaxPower.Serialize(stream);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BasePowerTarget.IConcretePowerTarget), stream, version);

            m_MinPower.Deserialize(stream, version);
            m_MaxPower.Deserialize(stream, version);
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
            attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            attribute.Value = "PowerInWatts_t";
            childNode.Attributes.Append(attribute);
            parentNode.AppendChild(childNode);
            m_MinPower.Serialize(childNode, Constants.ValueTCXString, document);

            // High
            childNode = document.CreateElement("High");
            attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            attribute.Value = "PowerInWatts_t";
            childNode.Attributes.Append(attribute);
            parentNode.AppendChild(childNode);
            m_MaxPower.Serialize(childNode, Constants.ValueTCXString, document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            bool minRead = false;
            bool maxRead = false;

            for(int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode child = parentNode.ChildNodes[i];

                if (child.Name == "Low" && child.ChildNodes.Count == 1 &&
                    child.FirstChild.Name == Constants.ValueTCXString)
                {
                    m_MinPower.Deserialize(child.FirstChild);
                    minRead = true;
                }
                else if (child.Name == "High" && child.ChildNodes.Count == 1 &&
                    child.FirstChild.Name == Constants.ValueTCXString)
                {
                    m_MaxPower.Deserialize(child.FirstChild);
                    maxRead = true;
                }
            }

            if(!minRead || !maxRead)
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in power range target XML node", parentNode);
            }

            SetValues(Math.Min(MinPower, MaxPower), Math.Max(MinPower, MaxPower));
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
            Debug.Assert(minPower <= maxPower);
            Debug.Assert(m_MinPower.IsInRange(minPower));
            Debug.Assert(m_MaxPower.IsInRange(maxPower));
        }

        public UInt16 MinPower
        {
            get { return m_MinPower; }
            private set
            {
                if (MinPower != value)
                {
                    m_MinPower.Value = value;

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
                    m_MaxPower.Value = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MaxPower"));
                }
            }
        }

        public override bool IsDirty
        {
            get { return false; }
            set { Debug.Assert(false); }
        }

        private GarminFitnessUInt16Range m_MinPower = new GarminFitnessUInt16Range(75, Constants.MinPower, Constants.MaxPowerWorkout);
        private GarminFitnessUInt16Range m_MaxPower = new GarminFitnessUInt16Range(165, Constants.MinPower, Constants.MaxPowerWorkout);
    }
}
