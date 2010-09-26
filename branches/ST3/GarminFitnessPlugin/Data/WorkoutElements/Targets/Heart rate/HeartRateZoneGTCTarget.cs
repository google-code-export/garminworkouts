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

            m_Zone.Serialize(stream);
        }

        public override void SerializetoFIT(FITMessage message)
        {
            FITMessageField HRZone = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);

            HRZone.SetUInt32(Zone);
            message.AddField(HRZone);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseHeartRateTarget.IConcreteHeartRateTarget), stream, version);

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
            attribute.Value = "PredefinedHeartRateZone_t";
            parentNode.Attributes.Append(attribute);

            m_Zone.Serialize(parentNode, "Number", document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            if (parentNode.ChildNodes.Count != 1 || parentNode.FirstChild.Name != "Number")
            {
                throw new GarminFitnessXmlDeserializationException("Invalid GTC heart rate target in XML node", parentNode);
            }

            m_Zone.Deserialize(parentNode.FirstChild);
        }

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            step.SetTargetType(1);
            step.SetTargetValue(Zone);
        }

        public override void Deserialize(GarXFaceNet._Workout._Step step)
        {
            Zone = (Byte)step.GetDurationValue();
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

        private GarminFitnessByteRange m_Zone = new GarminFitnessByteRange(1, 1, Constants.GarminHRZoneCount);
    }
}
