using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class HeartRateZoneGTCTarget : BaseHeartRateTarget.IConcreteHeartRateTarget
    {
        public HeartRateZoneGTCTarget(BaseHeartRateTarget baseTarget)
            : base(HeartRateTargetType.ZoneGTC, baseTarget)
        {
            Zone = 1;
        }

        public HeartRateZoneGTCTarget(Byte zone, BaseHeartRateTarget baseTarget)
            : this(baseTarget)
        {
            Zone = zone;
        }

        public HeartRateZoneGTCTarget(Stream stream, DataVersion version, BaseHeartRateTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.WriteByte(Zone);
        }

        public override void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseHeartRateTarget.IConcreteHeartRateTarget), stream, version);

            Zone = (Byte)stream.ReadByte();
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);

            XmlAttribute attribute;
            XmlNode childNode;

            // Type
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = "PredefinedHeartRateZone_t";
            parentNode.Attributes.Append(attribute);

            // Value
            childNode = document.CreateElement("Number");
            childNode.AppendChild(document.CreateTextNode(Zone.ToString()));
            parentNode.AppendChild(childNode);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                if (parentNode.ChildNodes.Count == 1 && parentNode.FirstChild.Name == "Number")
                {
                    XmlNode valueNode = parentNode.FirstChild;

                    if (valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText) &&
                        Utils.IsTextIntegerInRange(valueNode.FirstChild.Value, 1, 5))
                    {
                        Zone = Byte.Parse(valueNode.FirstChild.Value);
                        return true;
                    }
                }
            }

            return false;
        }

        public Byte Zone
        {
            get { return m_Zone; }
            set
            {
                if (Zone != value)
                {
                    Trace.Assert(value <= 5);
                    m_Zone = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("Zone"));
                }
            }
        }

        public override bool IsDirty
        {
            get { return false; }
            set { Trace.Assert(false); }
        }

        private Byte m_Zone;
    }
}
