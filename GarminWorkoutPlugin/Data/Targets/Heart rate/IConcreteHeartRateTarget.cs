using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace GarminWorkoutPlugin.Data
{
    abstract class IConcreteHeartRateTarget : IPluginSerializable, IXMLSerializable, IDirty
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
}
