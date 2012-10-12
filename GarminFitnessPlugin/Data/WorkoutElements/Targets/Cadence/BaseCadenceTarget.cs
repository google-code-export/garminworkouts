using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class BaseCadenceTarget : ITarget
    {
        public abstract class IConcreteCadenceTarget : IPluginSerializable, IXMLSerializable, IDirty
        {
            public IConcreteCadenceTarget(CadenceTargetType type, BaseCadenceTarget baseTarget)
            {
                Debug.Assert(type != CadenceTargetType.CadenceTargetTypeCount);

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
            }

            public virtual void Deserialize(XmlNode parentNode)
            {
            }

            protected void TriggerTargetChangedEvent(IConcreteCadenceTarget target, PropertyChangedEventArgs args)
            {
                if (target == BaseTarget.ConcreteTarget)
                {
                    BaseTarget.TriggerTargetChangedEvent(args);
                }
            } 

            public CadenceTargetType Type
            {
                get { return m_Type; }
            }

            public BaseCadenceTarget BaseTarget
            {
                get { return m_BaseTarget; }
            }

            public abstract bool IsDirty
            {
                get;
                set;
            }

            public enum CadenceTargetType
            {
                [StepDescriptionStringProviderAttribute("CadenceZoneTargetDescriptionText")]
                ZoneST = 0,
                [StepDescriptionStringProviderAttribute("CadenceRangeTargetDescriptionText")]
                Range,
                CadenceTargetTypeCount
            };

            CadenceTargetType m_Type;
            BaseCadenceTarget m_BaseTarget;
        }

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

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField targetType = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.TargetType);

            targetType.SetEnum((Byte)FITWorkoutStepTargetTypes.Cadence);

            ConcreteTarget.FillFITStepMessage(message);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // In V0, we only have GTC zone type
            m_ConcreteTarget = new CadenceRangeTarget(stream, version, this);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            Byte[] intBuffer = new Byte[sizeof(UInt32)];
            BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType type;

            stream.Read(intBuffer, 0, sizeof(UInt32));
            type = (BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType)BitConverter.ToUInt32(intBuffer, 0);

            switch (type)
            {
                case BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.ZoneST:
                    {
                        m_ConcreteTarget = new CadenceZoneSTTarget(stream, version, this);
                        break;
                    }
                case BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.Range:
                    {
                        m_ConcreteTarget = new CadenceRangeTarget(stream, version, this);
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

            ConcreteTarget.Serialize(parentNode, nodeName, document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            // We have either a range or a ST Cadence zone but we can't tell before
            //  the extension section so create a range and if it ends up being a ST
            //  zone, replace it
            ConcreteTarget = new CadenceRangeTarget(this);

            ConcreteTarget.Deserialize(parentNode);
        }

        public override void HandleTargetOverride(XmlNode extensionNode)
        {
            // We got here so our target must be a range
            Debug.Assert(ConcreteTarget.Type == BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.Range);

            IZoneCategory referenceZones = Options.Instance.CadenceZoneCategory;
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
                    Utils.IsTextIntegerInRange(childNode.FirstChild.Value, (UInt16)1, (UInt16)referenceZones.Zones.Count))
                {
                    zoneIndex = int.Parse(childNode.FirstChild.Value);
                }
            }

            if (zoneReferenceId == referenceZones.ReferenceId && zoneIndex != -1)
            {
                ConcreteTarget = new CadenceZoneSTTarget(referenceZones.Zones[zoneIndex], this);
            }
        }

        public BaseCadenceTarget.IConcreteCadenceTarget ConcreteTarget
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

        private BaseCadenceTarget.IConcreteCadenceTarget m_ConcreteTarget = null;
    }
}
