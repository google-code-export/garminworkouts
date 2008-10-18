using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.Data
{
    class BasePowerTarget : ITarget
    {
        public BasePowerTarget(IStep parent)
            : base(ITarget.TargetType.Power, parent)
        {
            if (Options.UseSportTracksPowerZones)
            {
                ConcreteTarget = new PowerZoneSTTarget(this);
            }
            else
            {
                ConcreteTarget = new PowerZoneGTCTarget(this);
            }
        }

        public BasePowerTarget(Stream stream, DataVersion version, IStep parent)
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
            // In V0, we only have GTC zone type
            m_ConcreteTarget = new PowerZoneGTCTarget(stream, version, this);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            Byte[] intBuffer = new Byte[sizeof(UInt32)];
            IConcretePowerTarget.PowerTargetType type;

            stream.Read(intBuffer, 0, sizeof(UInt32));
            type = (IConcretePowerTarget.PowerTargetType)BitConverter.ToUInt32(intBuffer, 0);

            switch(type)
            {
                case IConcretePowerTarget.PowerTargetType.ZoneGTC:
                    {
                        m_ConcreteTarget = new PowerZoneGTCTarget(stream, version, this);
                        break;
                    }
                case IConcretePowerTarget.PowerTargetType.ZoneST:
                    {
                        m_ConcreteTarget = new PowerZoneSTTarget(stream, version, this);
                        break;
                    }
                case IConcretePowerTarget.PowerTargetType.Range:
                    {
                        m_ConcreteTarget = new PowerRangeTarget(stream, version, this);
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

            childNode = document.CreateElement("PowerZone");
            parentNode.AppendChild(childNode);

            ConcreteTarget.Serialize(childNode, document);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if(base.Deserialize(parentNode))
            {
                if (parentNode.ChildNodes.Count == 1 && parentNode.FirstChild.Name == "PowerZone")
                {
                    XmlNode child = parentNode.FirstChild;

                    if (child.Attributes.Count == 1 && child.Attributes[0].Name == "xsi:type" &&
                        child.Attributes[0].Value == "PredefinedPowerZone_t")
                    {
                        // We have a GTC HR zone
                        ConcreteTarget = new PowerZoneGTCTarget(this);
                        return ConcreteTarget.Deserialize(child);
                    }
                    else if(child.Attributes.Count == 1 && child.Attributes[0].Name == "xsi:type" &&
                        child.Attributes[0].Value == "CustomPowerZone_t")
                    {
                        // We have either a range or a ST power zone but we can't tell before the
                        //  extension section so create a range and if it ends up being a ST
                        //  zone, replace it
                        ConcreteTarget = new PowerRangeTarget(this);

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
            Trace.Assert(ConcreteTarget.Type == IConcretePowerTarget.PowerTargetType.Range);

            IZoneCategory referenceZones = Options.PowerZoneCategory;
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
                ConcreteTarget = new PowerZoneSTTarget(referenceZones.Zones[zoneIndex], this);
            }
        }

        public IConcretePowerTarget ConcreteTarget
        {
            get { return m_ConcreteTarget; }
            set { m_ConcreteTarget = value; }
        }

        public override bool IsDirty
        {
            get { return ConcreteTarget.IsDirty; }
            set { Trace.Assert(false); }
        }

        private IConcretePowerTarget m_ConcreteTarget = null;
    }
}
