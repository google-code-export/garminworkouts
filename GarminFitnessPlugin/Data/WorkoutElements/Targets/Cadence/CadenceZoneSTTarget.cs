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

            // Zone category refId
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
            Deserialize(typeof(BaseCadenceTarget.IConcreteCadenceTarget), stream, version);

            IZoneCategory zones = Options.Instance.CadenceZoneCategory;
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
                Debug.Assert(zones.Zones.Count > 0);
                Zone = zones.Zones[0];
            }
        }

        public void Deserialize_V3(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseCadenceTarget.IConcreteCadenceTarget), stream, version);

            IZoneCategory zones = Options.Instance.CadenceZoneCategory;
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
            XmlNode childNode;
            XmlNode valueNode;

            // Low
            childNode = document.CreateElement("Low");
            childNode.AppendChild(document.CreateTextNode(String.Format(culture.NumberFormat, "{0:0.00000}", Zone.Low)));
            parentNode.AppendChild(childNode);

            // High
            Byte zoneHigh = (Byte)Math.Min(Constants.MaxCadence, Zone.High);
            childNode = document.CreateElement("High");
            childNode.AppendChild(document.CreateTextNode(String.Format(culture.NumberFormat, "{0:0.00000}", zoneHigh)));
            parentNode.AppendChild(childNode);

            // Extension
            for (int i = 0; i < Options.Instance.CadenceZoneCategory.Zones.Count; ++i)
            {
                INamedLowHighZone currentZone = Options.Instance.CadenceZoneCategory.Zones[i];
                
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
                    valueNode.AppendChild(document.CreateTextNode(Options.Instance.CadenceZoneCategory.ReferenceId));
                    categoryNode.AppendChild(valueNode);
                    valueNode = document.CreateElement("Index");
                    valueNode.AppendChild(document.CreateTextNode(i.ToString()));
                    categoryNode.AppendChild(valueNode);
                    extensionNode.AppendChild(categoryNode);

                    BaseTarget.ParentStep.ParentWorkout.AddSportTracksExtension(extensionNode);
                }
            }
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
