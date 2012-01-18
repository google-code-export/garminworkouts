using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class SpeedZoneGTCTarget : BaseSpeedTarget.IConcreteSpeedTarget
    {
        public SpeedZoneGTCTarget(BaseSpeedTarget baseTarget)
            : base(SpeedTargetType.ZoneGTC, baseTarget)
        {
        }

        public SpeedZoneGTCTarget(Byte zone, BaseSpeedTarget baseTarget)
            : this(baseTarget)
        {
            Zone = zone;
        }

        public SpeedZoneGTCTarget(Stream stream, DataVersion version, BaseSpeedTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_Zone.Serialize(stream);
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField speedZone = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);

            speedZone.SetUInt32(Zone);
            message.AddField(speedZone);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseSpeedTarget.IConcreteSpeedTarget), stream, version);

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
            attribute.Value = Constants.SpeedRangeZoneTCXString[0];
            parentNode.Attributes.Append(attribute);

            m_Zone.Serialize(parentNode, "Number", document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            if (parentNode.ChildNodes.Count != 1 || parentNode.FirstChild.Name != "Number")
            {
                throw new GarminFitnessXmlDeserializationException("Invalid GTC speed target in XML node", parentNode);
            }

            m_Zone.Deserialize(parentNode.FirstChild);
        }

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            step.SetTargetType(0);
            step.SetTargetValue(Zone);
        }

        public override void Deserialize(GarXFaceNet._Workout._Step step)
        {
            Zone = (Byte)step.GetTargetValue();
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

        private GarminFitnessByteRange m_Zone = new GarminFitnessByteRange(1, 1, Constants.GarminSpeedZoneCount);
    }
}
