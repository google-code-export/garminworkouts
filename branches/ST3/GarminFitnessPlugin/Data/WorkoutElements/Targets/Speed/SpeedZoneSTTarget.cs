using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class SpeedZoneSTTarget : BaseSpeedTarget.IConcreteSpeedTarget
    {
        public SpeedZoneSTTarget(BaseSpeedTarget baseTarget)
            : base(SpeedTargetType.ZoneST, baseTarget)
        {
            Debug.Assert(baseTarget.ParentStep.ParentConcreteWorkout.Category.SpeedZone.Zones.Count > 0);

            Zone = baseTarget.ParentStep.ParentConcreteWorkout.Category.SpeedZone.Zones[0];
        }

        public SpeedZoneSTTarget(INamedLowHighZone zone, BaseSpeedTarget baseTarget)
            : this(baseTarget)
        {
            Zone = zone;
        }

        public SpeedZoneSTTarget(Stream stream, DataVersion version, BaseSpeedTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            IZoneCategory zones = BaseTarget.ParentStep.ParentConcreteWorkout.Category.SpeedZone;
            String zoneRefID = zones.ReferenceId;

            GarminFitnessString categoryRefID = new GarminFitnessString(zoneRefID);
            categoryRefID.Serialize(stream);

            GarminFitnessInt32Range zoneIndex = new GarminFitnessInt32Range(Utils.FindIndexForZone(zones.Zones, Zone));
            zoneIndex.Serialize(stream);

            GarminFitnessBool dirty = new GarminFitnessBool(IsDirty);
            dirty.Serialize(stream);
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField speedZone = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);
            FITMessageField minSpeed = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueLow);
            FITMessageField maxSpeed = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueHigh);

            speedZone.SetUInt32((Byte)0);
            message.AddField(speedZone);
            minSpeed.SetUInt32((UInt32)(Utils.Clamp(Zone.Low, Constants.MinSpeedMetersPerSecond, Constants.MaxSpeedMetersPerSecond) * 1000));
            message.AddField(minSpeed);
            maxSpeed.SetUInt32((UInt32)(Utils.Clamp(Zone.High, Constants.MinSpeedMetersPerSecond, Constants.MaxSpeedMetersPerSecond) * 1000));
            message.AddField(maxSpeed);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseSpeedTarget.IConcreteSpeedTarget), stream, version);

            IZoneCategory zones = BaseTarget.ParentStep.ParentConcreteWorkout.Category.SpeedZone;

            // RefId
            GarminFitnessString categoryRefID = new GarminFitnessString();
            categoryRefID.Deserialize(stream, version);

            // Zone index
            GarminFitnessInt32Range zoneIndex = new GarminFitnessInt32Range(0);
            zoneIndex.Deserialize(stream, version);

            if (categoryRefID == zones.ReferenceId && zoneIndex < zones.Zones.Count)
            {
                Zone = zones.Zones[zoneIndex];
            }
            else
            {
                Debug.Assert(zones.Zones.Count > 0);
                Zone = zones.Zones[0];

                // We can't find saved zone, force dirty
                IsDirty = true;
            }
        }

        public void Deserialize_V3(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseSpeedTarget.IConcreteSpeedTarget), stream, version);

            IZoneCategory zones = BaseTarget.ParentStep.ParentConcreteWorkout.Category.SpeedZone;

            GarminFitnessString categoryRefID = new GarminFitnessString();
            categoryRefID.Deserialize(stream, version);

            GarminFitnessInt32Range zoneIndex = new GarminFitnessInt32Range(0);
            zoneIndex.Deserialize(stream, version);

            GarminFitnessBool dirty = new GarminFitnessBool(IsDirty);
            dirty.Deserialize(stream, version);

            if (categoryRefID == zones.ReferenceId && zoneIndex < zones.Zones.Count)
            {
                Zone = zones.Zones[zoneIndex];

                // Was the step dirty on last save?
                IsDirty = dirty;
            }
            else
            {
                Debug.Assert(zones.Zones.Count > 0);
                Zone = zones.Zones[0];

                // We can't find saved zone, force dirty
                IsDirty = true;
            }
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            // This node was added by our parent...
            parentNode = parentNode.LastChild;

            XmlAttribute attribute;

            // Type
            attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            attribute.Value = "CustomSpeedZone_t";
            parentNode.Attributes.Append(attribute);

            // View as
            GarminFitnessBool viewAs = new GarminFitnessBool(ViewAsPace, Constants.SpeedOrPaceTCXString[0], Constants.SpeedOrPaceTCXString[1]);
            viewAs.Serialize(parentNode, Constants.ViewAsTCXString, document);

            // Low
            GarminFitnessDoubleRange lowInMetersPerSecond = new GarminFitnessDoubleRange(Utils.Clamp(Zone.Low, Constants.MinSpeedMetersPerSecond, Constants.MaxSpeedMetersPerSecond));
            lowInMetersPerSecond.Serialize(parentNode, Constants.LowInMeterPerSecTCXString, document);

            // High
            GarminFitnessDoubleRange highInMetersPerSecond = new GarminFitnessDoubleRange(Utils.Clamp(Zone.High, Constants.MinSpeedMetersPerSecond, Constants.MaxSpeedMetersPerSecond));
            highInMetersPerSecond.Serialize(parentNode, Constants.HighInMeterPerSecTCXString, document);

            // Extension
            Utils.SerializeSTZoneInfoXML(BaseTarget.ParentStep,
                                         BaseTarget.ParentStep.ParentConcreteWorkout.Category.SpeedZone,
                                         Zone, document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            // We should not end up here, the Xml deserialization should pass by extensions
            Debug.Assert(false);
        }

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            step.SetTargetType(0);
            step.SetTargetValue(0);
            step.SetTargetCustomZoneLow(Zone.Low);
            step.SetTargetCustomZoneHigh(Zone.High);
        }

        public override void Deserialize(GarXFaceNet._Workout._Step step)
        {
            // We should not end up here, the Xml deserialization should pass by extensions
            Debug.Assert(false);
        }

        public INamedLowHighZone Zone
        {
            get { return m_Zone; }
            set
            {
                if (m_Zone != value)
                {
                    m_Zone = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("Zone"));
                }

                IsDirty = false;
            }
        }

        public override bool IsDirty
        {
            get { return m_IsDirty; }
            set { m_IsDirty = value; }
        }

        private INamedLowHighZone m_Zone;
        private bool m_IsDirty = false;
    }
}
