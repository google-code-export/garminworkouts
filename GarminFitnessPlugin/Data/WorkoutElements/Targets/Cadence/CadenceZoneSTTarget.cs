using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class CadenceZoneSTTarget : BaseCadenceTarget.IConcreteCadenceTarget
    {
        public CadenceZoneSTTarget(BaseCadenceTarget baseTarget)
            : base(CadenceTargetType.ZoneST, baseTarget)
        {
            Debug.Assert(Options.Instance.CadenceZoneCategory.Zones.Count > 0);

            Zone = Options.Instance.CadenceZoneCategory.Zones[0];
        }

        public CadenceZoneSTTarget(INamedLowHighZone zone, BaseCadenceTarget baseTarget)
            : this(baseTarget)
        {
            Zone = zone;
        }

        public CadenceZoneSTTarget(Stream stream, DataVersion version, BaseCadenceTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            IZoneCategory zones = Options.Instance.CadenceZoneCategory;
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
            FITMessageField cadenceZone = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);
            FITMessageField minCadence = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueLow);
            FITMessageField maxCadence = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueHigh);

            cadenceZone.SetUInt32((Byte)0);
            message.AddField(cadenceZone);
            minCadence.SetUInt32((UInt32)Math.Max(Constants.MinCadence, Zone.Low));
            message.AddField(minCadence);
            maxCadence.SetUInt32((UInt32)Math.Min(Constants.MaxCadence, Zone.High));
            message.AddField(maxCadence);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseCadenceTarget.IConcreteCadenceTarget), stream, version);

            IZoneCategory zones = Options.Instance.CadenceZoneCategory;

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
            Deserialize(typeof(BaseCadenceTarget.IConcreteCadenceTarget), stream, version);

            IZoneCategory zones = Options.Instance.CadenceZoneCategory;

            // RefId
            GarminFitnessString categoryRefID = new GarminFitnessString();
            categoryRefID.Deserialize(stream, version);

            // Zone index
            GarminFitnessInt32Range zoneIndex = new GarminFitnessInt32Range(0);
            zoneIndex.Deserialize(stream, version);

            // Dirty flag
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

            CultureInfo culture = new CultureInfo("en-us");

            // Low
            GarminFitnessDoubleRange low = new GarminFitnessDoubleRange(Zone.Low);
            low.Serialize(parentNode, "Low", document);

            // High
            GarminFitnessDoubleRange high = new GarminFitnessDoubleRange((Byte)Math.Min(Constants.MaxCadence, Zone.High));
            high.Serialize(parentNode, "High", document);

            // Extension
            Utils.SerializeSTZoneInfoXML(BaseTarget.ParentStep,
                                         Options.Instance.CadenceZoneCategory,
                                         Zone, document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            // We should not end up here, the Xml deserialization should pass by extensions
            Debug.Assert(false);
        }

        public INamedLowHighZone Zone
        {
            get { return m_Zone; }
            set
            {
                if (Zone != value)
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
