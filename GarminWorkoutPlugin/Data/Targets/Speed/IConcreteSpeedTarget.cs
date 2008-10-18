using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.Data
{
    abstract class IConcreteSpeedTarget : IPluginSerializable, IXMLSerializable, IDirty
    {
        public IConcreteSpeedTarget(SpeedTargetType type, BaseSpeedTarget baseTarget)
        {
            Trace.Assert(type != SpeedTargetType.SpeedTargetTypeCount);

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
            get { return BaseTarget.ParentStep.ParentWorkout.Category.SpeedUnits == Speed.Units.Pace; }
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
}
