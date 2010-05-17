using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class HeartRateZoneSTTarget : BaseHeartRateTarget.IConcreteHeartRateTarget
    {
        public HeartRateZoneSTTarget(BaseHeartRateTarget baseTarget)
            : base(HeartRateTargetType.ZoneST, baseTarget)
        {
            Debug.Assert(baseTarget.ParentStep.ParentConcreteWorkout.Category.HeartRateZone.Zones.Count > 0);

            Zone = baseTarget.ParentStep.ParentConcreteWorkout.Category.HeartRateZone.Zones[0];
        }

        public HeartRateZoneSTTarget(INamedLowHighZone zone, BaseHeartRateTarget baseTarget)
            : this(baseTarget)
        {
            Zone = zone;
        }

        public HeartRateZoneSTTarget(Stream stream, DataVersion version, BaseHeartRateTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            IZoneCategory zones = BaseTarget.ParentStep.ParentConcreteWorkout.Category.HeartRateZone;
            String zoneRefID = zones.ReferenceId;

            GarminFitnessString categoryRefID = new GarminFitnessString(zoneRefID);
            categoryRefID.Serialize(stream);

            GarminFitnessInt32Range zoneIndex = new GarminFitnessInt32Range(Utils.FindIndexForZone(zones.Zones, Zone));
            zoneIndex.Serialize(stream);

            GarminFitnessBool dirty = new GarminFitnessBool(IsDirty);
            dirty.Serialize(stream);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseHeartRateTarget.IConcreteHeartRateTarget), stream, version);

            IZoneCategory zones = BaseTarget.ParentStep.ParentConcreteWorkout.Category.HeartRateZone;

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
            Deserialize(typeof(BaseHeartRateTarget.IConcreteHeartRateTarget), stream, version);

            IZoneCategory zones = BaseTarget.ParentStep.ParentConcreteWorkout.Category.HeartRateZone;

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

            float lastMaxHR = PluginMain.GetApplication().Logbook.Athlete.InfoEntries.LastEntryAsOfDate(DateTime.Now).MaximumHeartRatePerMinute;
            XmlAttribute attribute;
            XmlNode childNode;

            // Type
            attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            attribute.Value = "CustomHeartRateZone_t";
            parentNode.Attributes.Append(attribute);

            // Low
            GarminFitnessByteRange lowValue = new GarminFitnessByteRange(0);
            childNode = document.CreateElement("Low");
            parentNode.AppendChild(childNode);

            attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            childNode.Attributes.Append(attribute);
            if (!float.IsNaN(lastMaxHR))
            {
                float baseMultiplier = Constants.MaxHRInPercentMax / lastMaxHR;

                lowValue.Value = (Byte)Math.Round(Zone.Low * baseMultiplier, 0, MidpointRounding.AwayFromZero);
                attribute.Value = Constants.HeartRateReferenceTCXString[1];
            }
            else
            {
                lowValue.Value = (Byte)Utils.Clamp(Zone.Low, Constants.MinHRInBPM, Constants.MaxHRInBPM);
                attribute.Value = Constants.HeartRateReferenceTCXString[0];
            }
            lowValue.Serialize(childNode, Constants.ValueTCXString, document);

            // High
            GarminFitnessByteRange highValue = new GarminFitnessByteRange(0);
            childNode = document.CreateElement("High");
            parentNode.AppendChild(childNode);

            attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            childNode.Attributes.Append(attribute);
            if (!float.IsNaN(lastMaxHR))
            {
                float baseMultiplier = Constants.MaxHRInPercentMax / lastMaxHR;

                highValue.Value = (Byte)Math.Min(Constants.MaxHRInPercentMax, Math.Round(Zone.High * baseMultiplier, 0, MidpointRounding.AwayFromZero));
                attribute.Value = Constants.HeartRateReferenceTCXString[1];
            }
            else
            {
                highValue.Value = (Byte)Utils.Clamp(Zone.High, Constants.MinHRInBPM, Constants.MaxHRInBPM);
                attribute.Value = Constants.HeartRateReferenceTCXString[0];
            }
            highValue.Serialize(childNode, Constants.ValueTCXString, document);

            // Extension
            Utils.SerializeSTZoneInfoXML(BaseTarget.ParentStep,
                                         BaseTarget.ParentStep.ParentConcreteWorkout.Category.HeartRateZone,
                                         Zone, document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            // We should not end up here, the Xml deserialization should pass by extensions
            Debug.Assert(false);
        }

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            step.SetTargetType(1);
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
