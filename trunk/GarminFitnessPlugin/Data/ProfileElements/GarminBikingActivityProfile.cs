using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class GarminBikingActivityProfile : GarminActivityProfile
    {
        public GarminBikingActivityProfile(GarminCategories category) :
            base(category)
        {
            m_FTP = 300;

            // Power Zones
            UInt16 currentPower = 100;
            UInt16 powerStep = 50;
            for (int i = 0; i < Constants.GarminSpeedZoneCount; ++i)
            {
                m_PowerZones.Add(new GarminFitnessValueRange<UInt16>(currentPower, (UInt16)(currentPower + powerStep)));

                currentPower += powerStep;
            }

            // Bike profiles
            m_Bikes[0] = new GarminBikeProfile("Road");
            m_Bikes[1] = new GarminBikeProfile("MTB");
            m_Bikes[2] = new GarminBikeProfile("Spinner");

            for (int i = 0; i < Constants.GarminBikeProfileCount; ++i)
            {
                m_Bikes[i].BikeProfileChanged += new GarminBikeProfile.BikeProfileChangedEventHandler(OnBikeProfileChanged);
            }
        }

        void OnBikeProfileChanged(GarminBikeProfile sender, PropertyChangedEventArgs changedProperty)
        {
            TriggerChangedEvent(new PropertyChangedEventArgs("BikeProfile"));
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
            base.Deserialize(parentNode);

            bool FTPRead = false;
            int powerZonesRead = 0;
            int bikeProfilesRead = 0;
            UInt16 FTPValue = 0;

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == Constants.ExtensionsTCXString &&
                    currentChild.ChildNodes.Count == 1 &&
                    currentChild.FirstChild.Name == Constants.PowerZonesTCXString)
                {
                    XmlNode powerZonesNode = currentChild.FirstChild;

                    for (int j = 0; j < powerZonesNode.ChildNodes.Count; ++j)
                    {
                        XmlNode powerChild = powerZonesNode.ChildNodes[j];

                        if (powerChild.Name == Constants.FTPTCXString &&
                            powerChild.ChildNodes.Count == 1 &&
                            powerChild.FirstChild.GetType() == typeof(XmlText))
                        {
                            if (!Utils.IsTextIntegerInRange(powerChild.FirstChild.Value, Constants.MinPower, Constants.MaxPower))
                            {
                                return false;
                            }

                            FTPValue = UInt16.Parse(powerChild.FirstChild.Value);
                            FTPRead = true;
                        }
                        else if (powerChild.Name == Constants.PowerZoneTCXString)
                        {
                            int zoneIndex = PeekZoneNumber(powerChild);

                            if (zoneIndex != -1)
                            {
                                if (ReadPowerZone(zoneIndex, powerChild))
                                {
                                    powerZonesRead++;
                                }
                            }
                        }
                    }
                }
                else if (currentChild.Name == Constants.BikeTCXString)
                {
                    if (m_Bikes[bikeProfilesRead].Deserialize(currentChild))
                    {
                        bikeProfilesRead++;
                    }
                }
            }

            // Check if all was read successfully
            if (!FTPRead ||
                powerZonesRead != Constants.GarminPowerZoneCount ||
                bikeProfilesRead != Constants.GarminBikeProfileCount)
            {
                return false;
            }

            return true;
        }

        private bool ReadPowerZone(int index, XmlNode parentNode)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);

            bool lowRead = false;
            bool highRead = false;
            UInt16 lowLimit = 0;
            UInt16 highLimit = 0;

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == Constants.LowTCXString &&
                    currentChild.ChildNodes.Count == 1 &&
                    currentChild.FirstChild.GetType() == typeof(XmlText))
                {
                    if (!Utils.IsTextIntegerInRange(currentChild.FirstChild.Value,
                                                    Constants.MinPower, Constants.MaxPower))
                    {
                        return false;
                    }

                    lowLimit = UInt16.Parse(currentChild.FirstChild.Value);
                    lowRead = true;
                }
                else if (currentChild.Name == Constants.HighTCXString &&
                         currentChild.ChildNodes.Count == 1 &&
                         currentChild.FirstChild.GetType() == typeof(XmlText))
                {
                    if (!Utils.IsTextIntegerInRange(currentChild.FirstChild.Value,
                                                    Constants.MinPower, Constants.MaxPower))
                    {
                        return false;
                    }

                    highLimit = UInt16.Parse(currentChild.FirstChild.Value);
                    highRead = true;
                }
            }

            // Check if all was read successfully
            if (!lowRead || !highRead)
            {
                return false;
            }

            m_PowerZones[index].Lower = (UInt16)Math.Min(lowLimit, highLimit);
            m_PowerZones[index].Upper = (UInt16)Math.Max(lowLimit, highLimit);

            return true;
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

        public string GetBikeName(int index)
        {
            Trace.Assert(index >= 0 && index <= Constants.GarminBikeProfileCount);

            return m_Bikes[index].Name;
        }

        public GarminBikeProfile GetBikeProfile(int index)
        {
            Trace.Assert(index >= 0 && index <= Constants.GarminBikeProfileCount);

            return m_Bikes[index];
        }

        public UInt16 FTP
        {
            get { return m_FTP; }
            set
            {
                if (m_FTP != value)
                {
                    Trace.Assert(value >= Constants.MinPower && value <= Constants.MaxPower);

                    m_FTP = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("FTP"));
                }
            }
        }

        private UInt16 m_FTP;
        private List<GarminFitnessValueRange<UInt16>> m_PowerZones = new List<GarminFitnessValueRange<UInt16>>();
        private GarminBikeProfile[] m_Bikes = new GarminBikeProfile[3];
    }
}
