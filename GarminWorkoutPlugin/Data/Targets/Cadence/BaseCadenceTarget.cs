using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.Data
{
    class BaseCadenceTarget : ITarget
    {
        public BaseCadenceTarget(IStep parent)
            : base(ITarget.TargetType.Cadence, parent)
        {
            ConcreteTarget = new CadenceZoneSTTarget(this);
        }

        public BaseCadenceTarget(Stream stream, DataVersion version, IStep parent)
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
            m_ConcreteTarget = new CadenceRangeTarget(stream, version, this);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            Byte[] intBuffer = new Byte[sizeof(UInt32)];
            IConcreteCadenceTarget.CadenceTargetType type;

            stream.Read(intBuffer, 0, sizeof(UInt32));
            type = (IConcreteCadenceTarget.CadenceTargetType)BitConverter.ToUInt32(intBuffer, 0);

            switch (type)
            {
                case IConcreteCadenceTarget.CadenceTargetType.ZoneST:
                    {
                        m_ConcreteTarget = new CadenceZoneSTTarget(stream, version, this);
                        break;
                    }
                case IConcreteCadenceTarget.CadenceTargetType.Range:
                    {
                        m_ConcreteTarget = new CadenceRangeTarget(stream, version, this);
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

            ConcreteTarget.Serialize(parentNode, document);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                // We have either a range or a ST Cadence zone but we can't tell before
                //  the extension section so create a range and if it ends up being a ST
                //  zone, replace it
                ConcreteTarget = new CadenceRangeTarget(this);

                return ConcreteTarget.Deserialize(parentNode);
            }

            return false;
        }

        public override void HandleTargetOverride(XmlNode extensionNode)
        {
            // We got here so our target must be a range
            Trace.Assert(ConcreteTarget.Type == IConcreteCadenceTarget.CadenceTargetType.Range);

            IZoneCategory referenceZones = Options.CadenceZoneCategory;
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
                    Utils.IsTextIntegerInRange(childNode.FirstChild.Value, 1, (UInt16)referenceZones.Zones.Count))
                {
                    zoneIndex = int.Parse(childNode.FirstChild.Value);
                }
            }

            if (zoneReferenceId == referenceZones.ReferenceId && zoneIndex != -1)
            {
                ConcreteTarget = new CadenceZoneSTTarget(referenceZones.Zones[zoneIndex], this);
            }
        }

        public IConcreteCadenceTarget ConcreteTarget
        {
            get { return m_ConcreteTarget; }
            set { m_ConcreteTarget = value; }
        }

        public override bool IsDirty
        {
            get { return ConcreteTarget.IsDirty; }
            set { Trace.Assert(false); }
        }

        private IConcreteCadenceTarget m_ConcreteTarget = null;
    }
}
