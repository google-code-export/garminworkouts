using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;

namespace GarminFitnessPlugin.Data
{
    class GarminBikingActivityProfile : GarminActivityProfile
    {
        public GarminBikingActivityProfile(GarminCategories category) :
            base(category)
        {
            // Power Zones
            UInt16 currentPower = 100;
            UInt16 powerStep = 50;
            for (int i = 0; i < Constants.GarminSpeedZoneCount; ++i)
            {
                m_PowerZones.Add(new GarminFitnessValueRange<UInt16>(currentPower, (UInt16)(currentPower + powerStep)));

                currentPower += powerStep;
            }
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            // Power zones
            for (int i = 0; i < m_PowerZones.Count; ++i)
            {
                // Low bound
                stream.Write(BitConverter.GetBytes(m_PowerZones[i].Lower), 0, sizeof(UInt16));

                // High bound
                stream.Write(BitConverter.GetBytes(m_PowerZones[i].Upper), 0, sizeof(UInt16));
            }
        }

        public new void Deserialize_V8(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(GarminActivityProfile), stream, version);

            byte[] intBuffer = new byte[sizeof(UInt16)];

            for (int i = 0; i < m_PowerZones.Count; ++i)
            {
                // Lower limit
                stream.Read(intBuffer, 0, sizeof(UInt16));
                m_PowerZones[i].Lower = BitConverter.ToUInt16(intBuffer, 0);

                // Upper limit
                stream.Read(intBuffer, 0, sizeof(UInt16));
                m_PowerZones[i].Upper = BitConverter.ToUInt16(intBuffer, 0);
            }
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            return base.Deserialize(parentNode);
        }

        public UInt16 GetPowerLowLimit(int index)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);

            return m_PowerZones[index].Lower;
        }

        public UInt16 GetPowerHighLimit(int index)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);

            return m_PowerZones[index].Upper;
        }

        public void SetPowerLowLimit(int index, UInt16 value)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);

            if (m_PowerZones[index].Lower != value)
            {
                m_PowerZones[index].Lower = value;

                TriggerChangedEvent(new PropertyChangedEventArgs("PowerZoneLimit"));
            }
        }

        public void SetPowerHighLimit(int index, UInt16 value)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);

            if (m_PowerZones[index].Upper != value)
            {
                m_PowerZones[index].Upper = value;

                TriggerChangedEvent(new PropertyChangedEventArgs("PowerZoneLimit"));
            }
        }

        private List<GarminFitnessValueRange<UInt16>> m_PowerZones = new List<GarminFitnessValueRange<UInt16>>();
    }
}
