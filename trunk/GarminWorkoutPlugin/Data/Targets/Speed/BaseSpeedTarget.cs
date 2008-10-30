using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class BaseSpeedTarget : ITarget
    {
        public BaseSpeedTarget(IStep parent)
            : base(TargetType.Speed, parent)
        {
            if (Options.UseSportTracksSpeedZones)
            {
                ConcreteTarget = new SpeedZoneSTTarget(this);
            }
            else
            {
                ConcreteTarget = new SpeedZoneGTCTarget(this);
            }
        }

        public BaseSpeedTarget(Stream stream, DataVersion version, IStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_ConcreteTarget.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // In V0, we only have speed range target
            m_ConcreteTarget = new SpeedRangeTarget(stream, version, this);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            Byte[] intBuffer = new Byte[sizeof(UInt32)];
            IConcreteSpeedTarget.SpeedTargetType type;

            stream.Read(intBuffer, 0, sizeof(UInt32));
            type = (IConcreteSpeedTarget.SpeedTargetType)BitConverter.ToUInt32(intBuffer, 0);

            switch (type)
            {
                case IConcreteSpeedTarget.SpeedTargetType.ZoneGTC:
                    {
                        m_ConcreteTarget = new SpeedZoneGTCTarget(stream, version, this);
                        break;
                    }
                case IConcreteSpeedTarget.SpeedTargetType.ZoneST:
                    {
                        m_ConcreteTarget = new SpeedZoneSTTarget(stream, version, this);
                        break;
                    }
                case IConcreteSpeedTarget.SpeedTargetType.Range:
                    {
                        m_ConcreteTarget = new SpeedRangeTarget(stream, version, this);
                        break;
                    }
                default:
                    {
                        Trace.Assert(false);
                        break;
                    }
            }
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);

            XmlNode childNode;

            childNode = document.CreateElement("SpeedZone");
            parentNode.AppendChild(childNode);

            ConcreteTarget.Serialize(childNode, document);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                if (parentNode.ChildNodes.Count == 1 && parentNode.FirstChild.Name == "SpeedZone")
                {
                    XmlNode child = parentNode.FirstChild;

                    if (child.Attributes.Count == 1 && child.Attributes[0].Name == "xsi:type" &&
                        child.Attributes[0].Value == "PredefinedSpeedZone_t")
                    {
                        // We have a GTC HR zone
                        ConcreteTarget = new SpeedZoneGTCTarget(this);
                        return ConcreteTarget.Deserialize(child);
                    }
                    else if (child.Attributes.Count == 1 && child.Attributes[0].Name == "xsi:type" &&
                        child.Attributes[0].Value == "CustomSpeedZone_t")
                    {
                        // We have either a range or a ST HR zone but we can't tell before the
                        //  extension section so create a range and if it ends up being a ST
                        //  zone, replace it
                        ConcreteTarget = new SpeedRangeTarget(this);

                        return ConcreteTarget.Deserialize(child); ;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public override void HandleTargetOverride(XmlNode extensionNode)
        {
            // We got here so our target must be a range
            Trace.Assert(ConcreteTarget.Type == IConcreteSpeedTarget.SpeedTargetType.Range);

            IZoneCategory referenceZones = ParentStep.ParentWorkout.Category.SpeedZone;
            string zoneReferenceId = null;
            int zoneIndex = -1;

            for (int j = 0; j < extensionNode.ChildNodes.Count; ++j)
            {
                XmlNode childNode = extensionNode.ChildNodes[j];

                if (childNode.Name == "Id" && childNode.ChildNodes.Count == 1 &&
                    childNode.FirstChild.GetType() == typeof(XmlText))
                {
                    zoneReferenceId = childNode.FirstChild.Value;
                }
                else if (childNode.Name == "Index" && childNode.ChildNodes.Count == 1 &&
                    childNode.FirstChild.GetType() == typeof(XmlText) &&
                    Utils.IsTextIntegerInRange(childNode.FirstChild.Value, 0, (UInt16)(referenceZones.Zones.Count - 1)))
                {
                    zoneIndex = int.Parse(childNode.FirstChild.Value);
                }
            }

            if (zoneReferenceId == referenceZones.ReferenceId && zoneIndex != -1)
            {
                ConcreteTarget = new SpeedZoneSTTarget(referenceZones.Zones[zoneIndex], this);
            }
        }

        public IConcreteSpeedTarget ConcreteTarget
        {
            get { return m_ConcreteTarget; }
            set { m_ConcreteTarget = value; }
        }

        public override bool IsDirty
        {
            get { return ConcreteTarget.IsDirty; }
            set { Trace.Assert(false); }
        }

        private IConcreteSpeedTarget m_ConcreteTarget = null;
    }
}
