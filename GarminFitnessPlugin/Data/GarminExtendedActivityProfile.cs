using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;

namespace GarminFitnessPlugin.Data
{
    class GarminExtendedActivityProfile : GarminActivityProfile
    {
        public GarminExtendedActivityProfile(GarminCategories category) :
            base(category)
        {
            // Power Zones
            UInt16 startPower = 100;
            for (int i = 0; i < Constants.GarminSpeedZoneCount; ++i)
            {
                m_PowerZonesMaximum.Add(startPower);

                startPower += 50;
            }
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);
        }

        public new void Deserialize_V8(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(GarminActivityProfile), stream, version);


        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            return false;
        }

        void OnPowerZonePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TriggerChangedEvent(new PropertyChangedEventArgs("HeartRateZones"));
        }

        private List<UInt16> m_PowerZonesMaximum = new List<UInt16>();
    }
}
