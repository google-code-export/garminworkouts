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
            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
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
            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                // Low bound
                stream.Write(BitConverter.GetBytes(m_PowerZones[i].Lower), 0, sizeof(UInt16));

                // High bound
                stream.Write(BitConverter.GetBytes(m_PowerZones[i].Upper), 0, sizeof(UInt16));
            }

            // Bike profiles
            for (int i = 0; i < Constants.GarminBikeProfileCount; ++i)
            {
                m_Bikes[i].Serialize(stream);
            }

            // FTP
            stream.Write(BitConverter.GetBytes(FTP), 0, sizeof(UInt16));
        }

        public new void Deserialize_V8(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(GarminActivityProfile), stream, version);

            byte[] intBuffer = new byte[sizeof(UInt16)];

            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                // Lower limit
                stream.Read(intBuffer, 0, sizeof(UInt16));
                m_PowerZones[i].Lower = BitConverter.ToUInt16(intBuffer, 0);

                // Upper limit
                stream.Read(intBuffer, 0, sizeof(UInt16));
                m_PowerZones[i].Upper = BitConverter.ToUInt16(intBuffer, 0);
            }

            // Wow we serialize too much stuff in V8, 10 power zones instead of 7
            //  skip the remaining 3
            stream.Position += 12;

            // Bike profiles
            for (int i = 0; i < Constants.GarminBikeProfileCount; ++i)
            {
                m_Bikes[i].Deserialize(stream, version);
            }
        }

        public void Deserialize_V9(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(GarminActivityProfile), stream, version);

            byte[] intBuffer = new byte[sizeof(UInt16)];

            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                // Lower limit
                stream.Read(intBuffer, 0, sizeof(UInt16));
                m_PowerZones[i].Lower = BitConverter.ToUInt16(intBuffer, 0);

                // Upper limit
                stream.Read(intBuffer, 0, sizeof(UInt16));
                m_PowerZones[i].Upper = BitConverter.ToUInt16(intBuffer, 0);
            }

            // Bike profiles
            for (int i = 0; i < Constants.GarminBikeProfileCount; ++i)
            {
                m_Bikes[i].Deserialize(stream, version);
            }

            // FTP (was forgotten in V8)
            stream.Read(intBuffer, 0, sizeof(UInt16));
            FTP = BitConverter.ToUInt16(intBuffer, 0);
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);

            // Get the right parent, which was created in the base class
            parentNode = parentNode.LastChild;

            // Change the xsi:type to "BikeProfileActivity_t"
            if (parentNode.Attributes.GetNamedItem("xsi:type") != null)
            {
                parentNode.Attributes.GetNamedItem("xsi:type").Value = "BikeProfileActivity_t";
            }

            XmlAttribute attributeNode;
            XmlNode currentChild;

            currentChild = document.CreateElement(Constants.ExtensionsTCXString);

            // Power zones
            XmlNode powerZonesChild = document.CreateElement(Constants.PowerZonesTCXString);
            attributeNode = document.CreateAttribute("xmlns");
            attributeNode.Value = "http://www.garmin.com/xmlschemas/ProfileExtension/v1";
            powerZonesChild.Attributes.Append(attributeNode);

            // FTP
            XmlNode FTPNode = document.CreateElement(Constants.FTPTCXString);
            FTPNode.AppendChild(document.CreateTextNode(FTP.ToString()));
            powerZonesChild.AppendChild(FTPNode);

            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                XmlNode currentZoneNode = document.CreateElement(Constants.PowerZoneTCXString);

                // Number
                XmlNode numberNode = document.CreateElement("Number");
                numberNode.AppendChild(document.CreateTextNode((i + 1).ToString()));
                currentZoneNode.AppendChild(numberNode);

                // Low
                XmlNode low = document.CreateElement(Constants.LowTCXString);
                low.AppendChild(document.CreateTextNode(m_PowerZones[i].Lower.ToString()));
                currentZoneNode.AppendChild(low);

                // High
                XmlNode high = document.CreateElement(Constants.HighTCXString);
                high.AppendChild(document.CreateTextNode(m_PowerZones[i].Upper.ToString()));
                currentZoneNode.AppendChild(high);

                powerZonesChild.AppendChild(currentZoneNode);
            }

            currentChild.AppendChild(powerZonesChild);
            parentNode.AppendChild(currentChild);

            // Bike profiles
            for (int i = 0; i < Constants.GarminBikeProfileCount; ++i)
            {
                m_Bikes[i].Serialize(parentNode, document);
            }
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                UInt16 FTPValue = 0;
                int bikeProfilesRead = 0;

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
                                if (!Utils.IsTextIntegerInRange(powerChild.FirstChild.Value, Constants.MinPower, Constants.MaxPowerProfile))
                                {
                                    return false;
                                }

                                FTPValue = UInt16.Parse(powerChild.FirstChild.Value);
                            }
                            else if (powerChild.Name == Constants.PowerZoneTCXString)
                            {
                                int zoneIndex = PeekZoneNumber(powerChild);

                                if (zoneIndex != -1)
                                {
                                    if (!ReadPowerZone(zoneIndex, powerChild))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    else if (currentChild.Name == Constants.BikeTCXString)
                    {
                        if (!m_Bikes[bikeProfilesRead].Deserialize(currentChild))
                        {
                            return false;
                        }

                        bikeProfilesRead++;
                    }
                }

                // Officialize
                FTP = FTPValue;

                return true;
            }

            return false;
        }

        private bool ReadPowerZone(int index, XmlNode parentNode)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);

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
                                                    Constants.MinPower, Constants.MaxPowerProfile))
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
                                                    Constants.MinPower, Constants.MaxPowerProfile))
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

        public override GarminActivityProfile Clone()
        {
            MemoryStream stream = new MemoryStream();
            GarminActivityProfile clone = new GarminBikingActivityProfile(Category);

            Serialize(stream);
            stream.Position = 0;
            clone.Deserialize(stream, Constants.CurrentVersion);

            return clone;
        }

        public UInt16 GetPowerLowLimit(int index)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);

            return m_PowerZones[index].Lower;
        }

        public UInt16 GetPowerHighLimit(int index)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);

            return m_PowerZones[index].Upper;
        }

        public void SetPowerLowLimit(int index, UInt16 value)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);

            if (m_PowerZones[index].Lower != value)
            {
                m_PowerZones[index].Lower = value;

                TriggerChangedEvent(new PropertyChangedEventArgs("PowerZoneLimit"));
            }
        }

        public void SetPowerHighLimit(int index, UInt16 value)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);

            if (m_PowerZones[index].Upper != value)
            {
                m_PowerZones[index].Upper = value;

                TriggerChangedEvent(new PropertyChangedEventArgs("PowerZoneLimit"));
            }
        }

        public string GetBikeName(int index)
        {
            Debug.Assert(index >= 0 && index <= Constants.GarminBikeProfileCount);

            return m_Bikes[index].Name;
        }

        public GarminBikeProfile GetBikeProfile(int index)
        {
            Debug.Assert(index >= 0 && index <= Constants.GarminBikeProfileCount);

            return m_Bikes[index];
        }

        public UInt16 FTP
        {
            get { return m_FTP; }
            set
            {
                if (m_FTP != value)
                {
                    Debug.Assert(value >= Constants.MinPower && value <= Constants.MaxPowerProfile);

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
