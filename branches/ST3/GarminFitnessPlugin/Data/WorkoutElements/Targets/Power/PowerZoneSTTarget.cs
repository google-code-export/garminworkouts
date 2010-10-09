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
    class PowerZoneSTTarget : BasePowerTarget.IConcretePowerTarget
    {
        public PowerZoneSTTarget(BasePowerTarget baseTarget)
            : base(PowerTargetType.ZoneST, baseTarget)
        {
            Debug.Assert(Options.Instance.PowerZoneCategory.Zones.Count > 0);

            Zone = Options.Instance.PowerZoneCategory.Zones[0];
        }

        public PowerZoneSTTarget(INamedLowHighZone zone, BasePowerTarget baseTarget)
            : this(baseTarget)
        {
            Zone = zone;
        }

        public PowerZoneSTTarget(Stream stream, DataVersion version, BasePowerTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            IZoneCategory zones = Options.Instance.PowerZoneCategory;
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
            FITMessageField powerZone = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);
            FITMessageField minPower = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueLow);
            FITMessageField maxPower = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueHigh);
            bool exportAsPercentFTP = Options.Instance.ExportSportTracksPowerAsPercentFTP;
            GarminCategories category = Options.Instance.GetGarminCategory(BaseTarget.ParentStep.ParentWorkout.Category);
            float lastFTP = 0;

            if (category == GarminCategories.Biking)
            {
                GarminBikingActivityProfile profile = (GarminBikingActivityProfile)GarminProfileManager.Instance.UserProfile.GetProfileForActivity(category);

                lastFTP = profile.FTP;
            }
            else
            {
                exportAsPercentFTP = false;
            }

            powerZone.SetUInt32((Byte)0);
            message.AddField(powerZone);

            if (exportAsPercentFTP)
            {
                float baseMultiplier = Constants.MaxPowerInPercentFTP / lastFTP;

                minPower.SetUInt32((UInt32)Utils.Clamp(Math.Round(Zone.Low * baseMultiplier, 0, MidpointRounding.AwayFromZero), Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTP));
                maxPower.SetUInt32((UInt32)Utils.Clamp(Math.Round(Zone.High * baseMultiplier, 0, MidpointRounding.AwayFromZero), Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTP));
            }
            else
            {
                minPower.SetUInt32((UInt32)Utils.Clamp(Zone.Low, Constants.MinPowerInWatts, Constants.MaxPowerWorkoutInWatts) + 1000);
                maxPower.SetUInt32((UInt32)Utils.Clamp(Zone.High, Constants.MinPowerInWatts, Constants.MaxPowerWorkoutInWatts) + 1000);
            }

            message.AddField(minPower);
            message.AddField(maxPower);
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BasePowerTarget.IConcretePowerTarget), stream, version);

            IZoneCategory zones = Options.Instance.PowerZoneCategory;

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
            Deserialize(typeof(BasePowerTarget.IConcretePowerTarget), stream, version);

            IZoneCategory zones = Options.Instance.PowerZoneCategory;

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
            bool exportAsPercentFTP = Options.Instance.ExportSportTracksPowerAsPercentFTP;
            GarminCategories category = Options.Instance.GetGarminCategory(BaseTarget.ParentStep.ParentWorkout.Category);
            float lastFTP = 0;

            if (category == GarminCategories.Biking)
            {
                GarminBikingActivityProfile profile = (GarminBikingActivityProfile)GarminProfileManager.Instance.UserProfile.GetProfileForActivity(category);

                lastFTP = profile.FTP;
            }
            else
            {
                exportAsPercentFTP = false;
            }

            // This node was added by our parent...
            parentNode = parentNode.LastChild;

            XmlAttribute attribute;
            XmlNode childNode;
            GarminFitnessBool exportAsPercentMax = new GarminFitnessBool(exportAsPercentFTP, Constants.PowerReferenceTCXString[1], Constants.PowerReferenceTCXString[0]);
            GarminFitnessUInt32Range lowValue = new GarminFitnessUInt32Range(0);
            GarminFitnessUInt32Range highValue = new GarminFitnessUInt32Range(0);

            // Type
            attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            attribute.Value = "CustomPowerZone_t";
            parentNode.Attributes.Append(attribute);

            if (exportAsPercentMax)
            {
                float baseMultiplier = Constants.MaxHRInPercentMax / lastFTP;

                lowValue.Value = (UInt32)Utils.Clamp(Math.Round(Zone.Low * baseMultiplier, 0, MidpointRounding.AwayFromZero), Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTP);
                highValue.Value = (UInt32)Utils.Clamp(Math.Round(Zone.High * baseMultiplier, 0, MidpointRounding.AwayFromZero), Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTP);
            }
            else
            {
                lowValue.Value = (UInt32)Utils.Clamp(Zone.Low, Constants.MinPowerInWatts, Constants.MaxPowerWorkoutInWatts);
                highValue.Value = (UInt32)Utils.Clamp(Zone.High, Constants.MinPowerInWatts, Constants.MaxPowerWorkoutInWatts);
            }

            // Low
            childNode = document.CreateElement("Low");
            parentNode.AppendChild(childNode);
            exportAsPercentMax.SerializeAttribute(childNode, Constants.XsiTypeTCXString, Constants.xsins, document);
            lowValue.Serialize(childNode, Constants.ValueTCXString, document);

            // High
            childNode = document.CreateElement("High");
            parentNode.AppendChild(childNode);
            exportAsPercentMax.SerializeAttribute(childNode, Constants.XsiTypeTCXString, Constants.xsins, document);
            highValue.Serialize(childNode, Constants.ValueTCXString, document);

            // Extension
            Utils.SerializeSTZoneInfoXML(BaseTarget.ParentStep,
                                         Options.Instance.PowerZoneCategory,
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
