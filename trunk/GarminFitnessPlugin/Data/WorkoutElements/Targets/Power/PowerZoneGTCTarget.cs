using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class PowerZoneGTCTarget : BasePowerTarget.IConcretePowerTarget
    {
        public PowerZoneGTCTarget(BasePowerTarget baseTarget)
            : base(PowerTargetType.ZoneGTC, baseTarget)
        {
            Zone = 1;
        }

        public PowerZoneGTCTarget(Byte zone, BasePowerTarget baseTarget)
            : this(baseTarget)
        {
            Zone = zone;
        }

        public PowerZoneGTCTarget(Stream stream, DataVersion version, BasePowerTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.WriteByte(Zone);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BasePowerTarget.IConcretePowerTarget), stream, version);

            Zone = (Byte)stream.ReadByte();
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
            attribute.Value = "PredefinedPowerZone_t";
            parentNode.Attributes.Append(attribute);

            // Value
            childNode = document.CreateElement("Number");
            childNode.AppendChild(document.CreateTextNode(Zone.ToString()));
            parentNode.AppendChild(childNode);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            if (parentNode.ChildNodes.Count == 1 && parentNode.FirstChild.Name == "Number")
            {
                XmlNode valueNode = parentNode.FirstChild;

                if (valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText) &&
                    Utils.IsTextIntegerInRange(valueNode.FirstChild.Value, 1, 7))
                {
                    Zone = Byte.Parse(valueNode.FirstChild.Value);
                }
            }
        }

        public Byte Zone
        {
            get { return m_Zone; }
            set
            {
                if (m_Zone != value)
                {
                    Debug.Assert(value <= 7);
                    m_Zone = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("Zone"));
                }
            }
        }

        public override bool IsDirty
        {
            get { return false; }
            set { Debug.Assert(false); }
        }

        private Byte m_Zone;
    }
}
