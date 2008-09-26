using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.Data
{
    abstract class IConcretePowerTarget : IPluginSerializable, IXMLSerializable, IDirty
    {
        public IConcretePowerTarget(PowerTargetType type, BasePowerTarget baseTarget)
        {
            Trace.Assert(type != PowerTargetType.HeartRateTargetTypeCount);

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

        public PowerTargetType Type
        {
            get { return m_Type; }
        }

        public BasePowerTarget BaseTarget
        {
            get { return m_BaseTarget; }
        }

        public abstract bool IsDirty
        {
            get;
            set;
        }

        public enum PowerTargetType
        {
            [StepDescriptionStringProviderAttribute("PowerZoneTargetDescriptionText")]
            ZoneGTC = 0,
            [StepDescriptionStringProviderAttribute("PowerZoneTargetDescriptionText")]
            ZoneST,
            [StepDescriptionStringProviderAttribute("PowerRangeTargetDescriptionText")]
            Range,
            HeartRateTargetTypeCount
        };

        PowerTargetType m_Type;
        BasePowerTarget m_BaseTarget;
    }
}
