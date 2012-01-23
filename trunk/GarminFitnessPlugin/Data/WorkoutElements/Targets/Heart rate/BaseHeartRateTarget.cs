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
                Debug.Assert(type != HeartRateTargetType.HeartRateTargetTypeCount);

                m_Type = type;
                m_BaseTarget = baseTarget;
            }

            public override void Serialize(Stream stream)
            {
                stream.Write(BitConverter.GetBytes((Int32)Type), 0, sizeof(Int32));
            }

            public abstract void FillFITStepMessage(FITMessage message);

            public void Deserialize_V0(Stream stream, DataVersion version)
            {
                // This is the code that was in ITarget in data V0.  Since we changed our
                //  inheritance structure between V0 and V1, we must also change where the
                //  loading is done. It happens ITarget didn't deserialize anything in V0,
                //  so this is empty
            }

            public virtual void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
            {
                XmlNode childNode;

                childNode = document.CreateElement("HeartRateZone");
                parentNode.AppendChild(childNode);
            }

            public virtual void Deserialize(XmlNode parentNode)
            {
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

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField targetType = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetType);

            targetType.SetEnum((Byte)FITWorkoutStepTargetTypes.HeartRate);
            message.AddField(targetType);

            ConcreteTarget.FillFITStepMessage(message);
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
                        Debug.Assert(false);
                        break;
                    }
            }
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            // This node was added by our parent...
            parentNode = parentNode.LastChild;

            ConcreteTarget.Serialize(parentNode, "HeartRateZone", document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            if (parentNode.ChildNodes.Count == 1 && parentNode.FirstChild.Name == "HeartRateZone")
            {
                XmlNode child = parentNode.FirstChild;

                if (child.Attributes.Count == 1 && child.Attributes[0].Name == Constants.XsiTypeTCXString &&
                    child.Attributes[0].Value.Equals(Constants.HeartRateRangeZoneTCXString[0]))
                {
                    // We have a GTC HR zone
                    ConcreteTarget = new HeartRateZoneGTCTarget(this);
                    ConcreteTarget.Deserialize(child);
                }
                else if(child.Attributes.Count == 1 && child.Attributes[0].Name == Constants.XsiTypeTCXString &&
                    child.Attributes[0].Value.Equals(Constants.HeartRateRangeZoneTCXString[1]))
                {
                    // We have either a range or a ST HR zone but we can't tell before the
                    //  extension section so create a range and if it ends up being a ST
                    //  zone, replace it
                    ConcreteTarget = new HeartRateRangeTarget(this);

                    ConcreteTarget.Deserialize(child); ;
                }
            }
        }

        public override void HandleTargetOverride(XmlNode extensionNode)
        {
            // We got here so our target must be a range
            Debug.Assert(ConcreteTarget.Type == IConcreteHeartRateTarget.HeartRateTargetType.Range);

            IZoneCategory referenceZones = ParentStep.ParentConcreteWorkout.Category.HeartRateZone;
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
                    Utils.IsTextIntegerInRange(childNode.FirstChild.Value, (UInt16)0, (UInt16)(referenceZones.Zones.Count - 1)))
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
            set { Debug.Assert(false); }
        }

        private IConcreteHeartRateTarget m_ConcreteTarget = null;
    }
}
