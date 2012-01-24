using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Controller;
using System.ComponentModel;

namespace GarminFitnessPlugin.Data
{
    public class BaseSpeedTarget : ITarget
    {
        public abstract class IConcreteSpeedTarget : IPluginSerializable, IXMLSerializable, IDirty
        {
            public IConcreteSpeedTarget(SpeedTargetType type, BaseSpeedTarget baseTarget)
            {
                Debug.Assert(type != SpeedTargetType.SpeedTargetTypeCount);

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

                childNode = document.CreateElement("SpeedZone");
                parentNode.AppendChild(childNode);
            }

            public virtual void Deserialize(XmlNode parentNode)
            {
            }

            protected void TriggerTargetChangedEvent(IConcreteSpeedTarget target, PropertyChangedEventArgs args)
            {
                if (target == BaseTarget.ConcreteTarget)
                {
                    BaseTarget.TriggerTargetChangedEvent(args);
                }
            } 

            public SpeedTargetType Type
            {
                get { return m_Type; }
            }

            public BaseSpeedTarget BaseTarget
            {
                get { return m_BaseTarget; }
            }

            public bool ViewAsPace
            {
                get { return BaseTarget.ParentStep.ParentConcreteWorkout.Category.SpeedUnits == Speed.Units.Pace; }
            }

            public abstract bool IsDirty
            {
                get;
                set;
            }

            public enum SpeedTargetType
            {
                [StepDescriptionStringProviderAttribute("SpeedZoneTargetDescriptionText")]
                ZoneGTC = 0,
                [StepDescriptionStringProviderAttribute("SpeedZoneTargetDescriptionText")]
                ZoneST,
                [StepDescriptionStringProviderAttribute("SpeedRangeTargetDescriptionText")]
                Range,
                SpeedTargetTypeCount
            };

            SpeedTargetType m_Type;
            BaseSpeedTarget m_BaseTarget;
        }

        public BaseSpeedTarget(IStep parent)
            : base(TargetType.Speed, parent)
        {
            if (Options.Instance.UseSportTracksSpeedZones)
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

            ConcreteTarget.Serialize(stream);
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField targetType = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetType);

            targetType.SetEnum((Byte)FITWorkoutStepTargetTypes.Speed);
            message.AddField(targetType);

            ConcreteTarget.FillFITStepMessage(message);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // In V0, we only have speed range target
            ConcreteTarget = new SpeedRangeTarget(stream, version, this);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            Byte[] intBuffer = new Byte[sizeof(UInt32)];
            BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType type;

            stream.Read(intBuffer, 0, sizeof(UInt32));
            type = (IConcreteSpeedTarget.SpeedTargetType)BitConverter.ToUInt32(intBuffer, 0);

            switch (type)
            {
                case BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.ZoneGTC:
                    {
                        ConcreteTarget = new SpeedZoneGTCTarget(stream, version, this);
                        break;
                    }
                case BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.ZoneST:
                    {
                        ConcreteTarget = new SpeedZoneSTTarget(stream, version, this);
                        break;
                    }
                case BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.Range:
                    {
                        ConcreteTarget = new SpeedRangeTarget(stream, version, this);
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

            ConcreteTarget.Serialize(parentNode, "SpeedZone", document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            if (parentNode.ChildNodes.Count == 1 && parentNode.FirstChild.Name == "SpeedZone")
            {
                XmlNode child = parentNode.FirstChild;

                if (child.Attributes.Count == 1 && child.Attributes[0].Name == Constants.XsiTypeTCXString &&
                    child.Attributes[0].Value.Equals(Constants.SpeedRangeZoneTCXString[0]))
                {
                    // We have a GTC HR zone
                    ConcreteTarget = new SpeedZoneGTCTarget(this);
                    ConcreteTarget.Deserialize(child);
                }
                else if (child.Attributes.Count == 1 && child.Attributes[0].Name == Constants.XsiTypeTCXString &&
                    child.Attributes[0].Value.Equals(Constants.SpeedRangeZoneTCXString[1]))
                {
                    // We have either a range or a ST HR zone but we can't tell before the
                    //  extension section so create a range and if it ends up being a ST
                    //  zone, replace it
                    ConcreteTarget = new SpeedRangeTarget(this);
                    ConcreteTarget.Deserialize(child); ;
                }
                else
                {
                }
            }
        }

        public override void HandleTargetOverride(XmlNode extensionNode)
        {
            // We got here so our target must be a range
            Debug.Assert(ConcreteTarget.Type == BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.Range);

            IZoneCategory referenceZones = ParentStep.ParentConcreteWorkout.Category.SpeedZone;
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
                ConcreteTarget = new SpeedZoneSTTarget(referenceZones.Zones[zoneIndex], this);
            }
        }

        public BaseSpeedTarget.IConcreteSpeedTarget ConcreteTarget
        {
            get { return m_ConcreteTarget; }
            set
            {
                if (m_ConcreteTarget != value)
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

        private BaseSpeedTarget.IConcreteSpeedTarget m_ConcreteTarget = null;
    }
}
