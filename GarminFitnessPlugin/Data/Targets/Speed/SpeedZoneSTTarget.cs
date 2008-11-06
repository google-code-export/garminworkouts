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
            Trace.Assert(baseTarget.ParentStep.ParentWorkout.Category.SpeedZone.Zones.Count > 0);

            Zone = baseTarget.ParentStep.ParentWorkout.Category.SpeedZone.Zones[0];
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

            IZoneCategory zones = BaseTarget.ParentStep.ParentWorkout.Category.SpeedZone;
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
            Deserialize(typeof(BaseSpeedTarget.IConcreteSpeedTarget), stream, version);

            IZoneCategory zones = BaseTarget.ParentStep.ParentWorkout.Category.SpeedZone;
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] stringBuffer;
            Int32 stringLength;
            int zoneIndex;

            // RefId
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            
            // Zone Id
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
            Deserialize(typeof(BaseSpeedTarget.IConcreteSpeedTarget), stream, version);

            IZoneCategory zones = BaseTarget.ParentStep.ParentWorkout.Category.SpeedZone;
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

            // Zone Id
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
            base.Serialize(parentNode, document);

            CultureInfo culture = new CultureInfo("en-us");
            XmlAttribute attribute;
            XmlNode valueNode;

            // Type
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = "CustomSpeedZone_t";
            parentNode.Attributes.Append(attribute);

            // View as
            valueNode = document.CreateElement("ViewAs");
            valueNode.AppendChild(document.CreateTextNode(Constants.SpeedOrPaceTCXString[ViewAsPace ? 0 : 1]));
            parentNode.AppendChild(valueNode);

            // Low
            double lowInMetersPerSecond = Math.Max(0.44722, Math.Min(26.8222, Zone.Low));
            valueNode = document.CreateElement("LowInMetersPerSecond");
            valueNode.AppendChild(document.CreateTextNode(String.Format(culture.NumberFormat, "{0:0.00000}", lowInMetersPerSecond)));
            parentNode.AppendChild(valueNode);

            // High
            double highInMetersPerSecond = Math.Max(0.44722, Math.Min(26.8222, Zone.High));
            valueNode = document.CreateElement("HighInMetersPerSecond");
            valueNode.AppendChild(document.CreateTextNode(String.Format(culture.NumberFormat, "{0:0.00000}", highInMetersPerSecond)));
            parentNode.AppendChild(valueNode);

            // Extension
            for (int i = 0; i < BaseTarget.ParentStep.ParentWorkout.Category.SpeedZone.Zones.Count; ++i)
            {
                INamedLowHighZone currentZone = BaseTarget.ParentStep.ParentWorkout.Category.SpeedZone.Zones[i];
                
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
                    valueNode.AppendChild(document.CreateTextNode(BaseTarget.ParentStep.ParentWorkout.Category.SpeedZone.ReferenceId));
                    categoryNode.AppendChild(valueNode);
                    valueNode = document.CreateElement("Index");
                    valueNode.AppendChild(document.CreateTextNode(i.ToString()));
                    categoryNode.AppendChild(valueNode);
                    extensionNode.AppendChild(categoryNode);

                    BaseTarget.ParentStep.ParentWorkout.AddSportTracksExtension(extensionNode);
                }
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
