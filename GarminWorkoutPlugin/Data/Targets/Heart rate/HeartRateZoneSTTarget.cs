using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;

namespace GarminWorkoutPlugin.Data
{
    class HeartRateZoneSTTarget : IConcreteHeartRateTarget
    {
        public HeartRateZoneSTTarget(BaseHeartRateTarget baseTarget)
            : base(HeartRateTargetType.ZoneST, baseTarget)
        {
            Trace.Assert(baseTarget.ParentStep.ParentWorkout.Category.HeartRateZone.Zones.Count > 0);

            Zone = baseTarget.ParentStep.ParentWorkout.Category.HeartRateZone.Zones[0];
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

            IZoneCategory zones = BaseTarget.ParentStep.ParentWorkout.Category.HeartRateZone;
            String zoneRefID = zones.ReferenceId;

            // Zone categroy refId
            stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(zoneRefID)), 0, sizeof(Int32));
            stream.Write(Encoding.UTF8.GetBytes(zoneRefID), 0, Encoding.UTF8.GetByteCount(zoneRefID));

            // Zone index
            stream.Write(BitConverter.GetBytes(Utils.FindIndexForZone(zones.Zones, Zone)), 0, sizeof(Int32));

            // Dirty flag
            stream.Write(BitConverter.GetBytes(IsDirty), 0, sizeof(bool));
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IConcreteHeartRateTarget), stream, version);

            IZoneCategory zones = BaseTarget.ParentStep.ParentWorkout.Category.HeartRateZone;
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] stringBuffer;
            Int32 stringLength;
            int zoneIndex;

            // RefId
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);

            // Zone index
            stream.Read(intBuffer, 0, sizeof(Int32));
            zoneIndex = BitConverter.ToInt32(intBuffer, 0);

            if (Encoding.UTF8.GetString(stringBuffer) == zones.ReferenceId && zoneIndex < zones.Zones.Count)
            {
                Zone = zones.Zones[zoneIndex];
            }
            else
            {
                Trace.Assert(zones.Zones.Count > 0);
                Zone = zones.Zones[0];
            }
        }

        public void Deserialize_V3(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IConcreteHeartRateTarget), stream, version);

            IZoneCategory zones = BaseTarget.ParentStep.ParentWorkout.Category.HeartRateZone;
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] boolBuffer = new byte[sizeof(bool)];
            byte[] stringBuffer;
            Int32 stringLength;
            int zoneIndex;

            // RefId
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);

            // Zone index
            stream.Read(intBuffer, 0, sizeof(Int32));
            zoneIndex = BitConverter.ToInt32(intBuffer, 0);

            // Dirty flag
            stream.Read(boolBuffer, 0, sizeof(bool));

            if (Encoding.UTF8.GetString(stringBuffer) == zones.ReferenceId && zoneIndex < zones.Zones.Count)
            {
                Zone = zones.Zones[zoneIndex];

                // Was the step dirty on last save?
                IsDirty = BitConverter.ToBoolean(boolBuffer, 0);
            }
            else
            {
                Trace.Assert(zones.Zones.Count > 0);
                Zone = zones.Zones[0];

                // We can't find saved zone, force dirty
                IsDirty = true;
            }
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            float lastMaxHR = PluginMain.GetApplication().Logbook.Athlete.InfoEntries.LastEntryAsOfDate(DateTime.Now).MaximumHeartRatePerMinute;
            float baseMultiplier = 100.0f / lastMaxHR;
            base.Serialize(parentNode, document);

            XmlAttribute attribute;
            XmlNode childNode;
            XmlNode valueNode;

            // Type
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = "CustomHeartRateZone_t";
            parentNode.Attributes.Append(attribute);

            // Low
            Byte lowPercent = (Byte)Math.Round(Zone.Low * baseMultiplier, 0, MidpointRounding.AwayFromZero);
            childNode = document.CreateElement("Low");
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = Constants.HeartRateReferenceTCXString[1];
            childNode.Attributes.Append(attribute);
            valueNode = document.CreateElement("Value");
            valueNode.AppendChild(document.CreateTextNode(lowPercent.ToString()));
            childNode.AppendChild(valueNode);
            parentNode.AppendChild(childNode);

            // High
            Byte highPercent = (Byte)Math.Min(100, Math.Round(Zone.High * baseMultiplier, 0, MidpointRounding.AwayFromZero));
            childNode = document.CreateElement("High");
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = Constants.HeartRateReferenceTCXString[1];
            childNode.Attributes.Append(attribute);
            valueNode = document.CreateElement("Value");
            valueNode.AppendChild(document.CreateTextNode(highPercent.ToString()));
            childNode.AppendChild(valueNode);
            parentNode.AppendChild(childNode);

            // Extension
            XmlNode indexNode = document.CreateElement("Index");
            for (int i = 0; i < BaseTarget.ParentStep.ParentWorkout.Category.HeartRateZone.Zones.Count; ++i)
            {
                INamedLowHighZone currentZone = BaseTarget.ParentStep.ParentWorkout.Category.HeartRateZone.Zones[i];
                
                if(currentZone == Zone)
                {
                    XmlNode extensionNode;
                    XmlNode categoryNode;

                    extensionNode = document.CreateElement("TargetOverride");
                    valueNode = document.CreateElement("StepId");
                    valueNode.AppendChild(document.CreateTextNode(Utils.GetStepExportId(BaseTarget.ParentStep).ToString()));
                    extensionNode.AppendChild(valueNode);
                    categoryNode = document.CreateElement("Category");
                    valueNode = document.CreateElement("Id");
                    valueNode.AppendChild(document.CreateTextNode(BaseTarget.ParentStep.ParentWorkout.Category.HeartRateZone.ReferenceId));
                    categoryNode.AppendChild(valueNode);
                    valueNode = document.CreateElement("Index");
                    valueNode.AppendChild(document.CreateTextNode(i.ToString()));
                    categoryNode.AppendChild(valueNode);
                    extensionNode.AppendChild(categoryNode);

                    BaseTarget.ParentStep.ParentWorkout.AddSportTracksExtension(extensionNode);
                }
            }

            if (indexNode != null)
            {
            }
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            // We should not end up here, the Xml deserialization should pass by extensions
            Trace.Assert(false);

            return false;
        }

        public INamedLowHighZone Zone
        {
            get { return m_Zone; }
            set
            {
                m_Zone = value;
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
