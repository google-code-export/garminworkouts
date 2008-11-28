using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class BaseHeartRateTarget : ITarget
    {
        public abstract class IConcreteHeartRateTarget : IPluginSerializable, IXMLSerializable, IDirty
        {
            public IConcreteHeartRateTarget(HeartRateTargetType type, BaseHeartRateTarget baseTarget)
            {
                Trace.Assert(type != HeartRateTargetType.HeartRateTargetTypeCount);

                m_Type = type;
                m_BaseTarget = baseTarget;
            }

            public override void Serialize(Stream stream)
            {
                stream.Write(BitConverter.GetBytes((Int32)Type), 0, sizeof(Int32));
            }

            public void Deserialize_V0(Stream stream, DataVersion version)
            {
                // This is the code that was in ITarget in data V0.  Since we changed our
                //  inheritance structure between V0 and V1, we must also change where the
                //  loading is done. It happens ITarget didn't deserialize anything in V0,
                //  so this is empty
            }

            public virtual void Serialize(XmlNode parentNode, XmlDocument document)
            {
            }

            public virtual bool Deserialize(XmlNode parentNode)
            {
                return true;
            }


            protected void TriggerTargetChangedEvent(IConcreteHeartRateTarget target, PropertyChangedEventArgs args)
            {
                if (target == BaseTarget.ConcreteTarget)
                {
                    BaseTarget.TriggerTargetChangedEvent(args);
                }
            } 

            public HeartRateTargetType Type
            {
                get { return m_Type; }
            }

            public BaseHeartRateTarget BaseTarget
            {
                get { return m_BaseTarget; }
            }

            public abstract bool IsDirty
            {
                get;
                set;
            }

            public enum HeartRateTargetType
            {
                [StepDescriptionStringProviderAttribute("HeartRateZoneTargetDescriptionText")]
                ZoneGTC = 0,
                [StepDescriptionStringProviderAttribute("HeartRateZoneTargetDescriptionText")]
                ZoneST,
                [StepDescriptionStringProviderAttribute("HeartRateRangeTargetDescriptionText")]
                Range,
                HeartRateTargetTypeCount
            };

            HeartRateTargetType m_Type;
            BaseHeartRateTarget m_BaseTarget;
        }

        public BaseHeartRateTarget(IStep parent)
            : base(ITarget.TargetType.HeartRate, parent)
        {
            if (Options.Instance.UseSportTracksHeartRateZones)
            {
                ConcreteTarget = new HeartRateZoneSTTarget(this);
            }
            else
            {
                ConcreteTarget = new HeartRateZoneGTCTarget(this);
            }
        }

        public BaseHeartRateTarget(Stream stream, DataVersion version, IStep parent)
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
            m_ConcreteTarget = new HeartRateZoneGTCTarget(stream, version, this);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            Byte[] intBuffer = new Byte[sizeof(UInt32)];
            IConcreteHeartRateTarget.HeartRateTargetType type;

            stream.Read(intBuffer, 0, sizeof(UInt32));
            type = (IConcreteHeartRateTarget.HeartRateTargetType)BitConverter.ToUInt32(intBuffer, 0);

            switch(type)
            {
                case IConcreteHeartRateTarget.HeartRateTargetType.ZoneGTC:
                    {
                        m_ConcreteTarget = new HeartRateZoneGTCTarget(stream, version, this);
                        break;
                    }
                case IConcreteHeartRateTarget.HeartRateTargetType.ZoneST:
                    {
                        m_ConcreteTarget = new HeartRateZoneSTTarget(stream, version, this);
                        break;
                    }
                case IConcreteHeartRateTarget.HeartRateTargetType.Range:
                    {
                        m_ConcreteTarget = new HeartRateRangeTarget(stream, version, this);
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

            childNode = document.CreateElement("HeartRateZone");
            parentNode.AppendChild(childNode);

            ConcreteTarget.Serialize(childNode, document);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if(base.Deserialize(parentNode))
            {
                if (parentNode.ChildNodes.Count == 1 && parentNode.FirstChild.Name == "HeartRateZone")
                {
                    XmlNode child = parentNode.FirstChild;

                    if (child.Attributes.Count == 1 && child.Attributes[0].Name == "xsi:type" &&
                        child.Attributes[0].Value == "PredefinedHeartRateZone_t")
                    {
                        // We have a GTC HR zone
                        ConcreteTarget = new HeartRateZoneGTCTarget(this);
                        return ConcreteTarget.Deserialize(child);
                    }
                    else if(child.Attributes.Count == 1 && child.Attributes[0].Name == "xsi:type" &&
                        child.Attributes[0].Value == "CustomHeartRateZone_t")
                    {
                        // We have either a range or a ST HR zone but we can't tell before the
                        //  extension section so create a range and if it ends up being a ST
                        //  zone, replace it
                        ConcreteTarget = new HeartRateRangeTarget(this);

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
            Trace.Assert(ConcreteTarget.Type == IConcreteHeartRateTarget.HeartRateTargetType.Range);

            IZoneCategory referenceZones = ParentStep.ParentWorkout.Category.HeartRateZone;
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
                ConcreteTarget = new HeartRateZoneSTTarget(referenceZones.Zones[zoneIndex], this);
            }
        }

        public IConcreteHeartRateTarget ConcreteTarget
        {
            get { return m_ConcreteTarget; }
            set
            {
                if (ConcreteTarget != value)
                {
                    m_ConcreteTarget = value;

                    TriggerTargetChangedEvent(new PropertyChangedEventArgs("ConcreteTarget"));
                }
            }
        }

        public override bool IsDirty
        {
            get { return ConcreteTarget.IsDirty; }
            set { Trace.Assert(false); }
        }

        private IConcreteHeartRateTarget m_ConcreteTarget = null;
    }
}
