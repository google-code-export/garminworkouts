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

            m_Zone.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BasePowerTarget.IConcretePowerTarget), stream, version);

            m_Zone.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            // This node was added by our parent...
            parentNode = parentNode.LastChild;

            XmlAttribute attribute;

            // Type
            attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            attribute.Value = "PredefinedPowerZone_t";
            parentNode.Attributes.Append(attribute);

            m_Zone.Serialize(parentNode, "Number", document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            if (parentNode.ChildNodes.Count != 1 || parentNode.FirstChild.Name != "Number")
            {
                throw new GarminFitnessXmlDeserializationException("Invalid GTC power target in XML node", parentNode);
            }

            m_Zone.Deserialize(parentNode.FirstChild);
        }

        public Byte Zone
        {
            get { return m_Zone; }
            set
            {
                if (Zone != value)
                {
                    m_Zone.Value = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("Zone"));
                }
            }
        }

        public override bool IsDirty
        {
            get { return false; }
            set { Debug.Assert(false); }
        }

        private GarminFitnessByteRange m_Zone = new GarminFitnessByteRange(1, 1, Constants.GarminPowerZoneCount);
    }
}
