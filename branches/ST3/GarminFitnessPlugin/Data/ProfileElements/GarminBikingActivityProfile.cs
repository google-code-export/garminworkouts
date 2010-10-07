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
            // Power Zones
            UInt16 currentPower = 100;
            UInt16 powerStep = 50;
            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                m_PowerZones.Add(new GarminFitnessValueRange<GarminFitnessUInt16Range>(new GarminFitnessUInt16Range(currentPower, Constants.MinPowerInWatts, Constants.MaxPowerProfile),
                                                                                       new GarminFitnessUInt16Range((UInt16)(currentPower + powerStep), Constants.MinPowerInWatts, Constants.MaxPowerProfile)));

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
                m_PowerZones[i].Lower.Serialize(stream);

                m_PowerZones[i].Upper.Serialize(stream);
            }

            // Bike profiles
            for (int i = 0; i < Constants.GarminBikeProfileCount; ++i)
            {
                m_Bikes[i].Serialize(stream);
            }

            // FTP
            m_FTP.Serialize(stream);
        }

        public new void Deserialize_V8(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(GarminActivityProfile), stream, version);

            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                m_PowerZones[i].Lower.Deserialize(stream, version);

                m_PowerZones[i].Upper.Deserialize(stream, version);
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

            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                m_PowerZones[i].Lower.Deserialize(stream, version);

                m_PowerZones[i].Upper.Deserialize(stream, version);
            }

            // Bike profiles
            for (int i = 0; i < Constants.GarminBikeProfileCount; ++i)
            {
                m_Bikes[i].Deserialize(stream, version);
            }

            // FTP (was forgotten in V8)
            m_FTP.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            // Get the right parent, which was created in the base class
            parentNode = parentNode.LastChild;

            // Change the xsi:type to "BikeProfileActivity_t"
            if (parentNode.Attributes.GetNamedItem(Constants.XsiTypeTCXString) != null)
            {
                parentNode.Attributes.GetNamedItem(Constants.XsiTypeTCXString).Value = "BikeProfileActivity_t";
            }

            XmlAttribute attributeNode;
            XmlNode currentChild;

            currentChild = document.CreateElement(Constants.ExtensionsTCXString);

            // Power zones
            XmlNode powerZonesChild = document.CreateElement(Constants.PowerZonesTCXString);
            attributeNode = document.CreateAttribute("xmlns");
            attributeNode.Value = "http://www.garmin.com/xmlschemas/ProfileExtension/v1";
            powerZonesChild.Attributes.Append(attributeNode);

            m_FTP.Serialize(powerZonesChild, Constants.FTPTCXString, document);

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
                m_Bikes[i].Serialize(parentNode, Constants.BikeTCXString, document);
            }
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            if (parentNode.Attributes.GetNamedItem(Constants.XsiTypeTCXString).Value == "BikeProfileActivity_t")
            {
                bool FTPRead = false;
                int powerZonesRead = 0;
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

                            if (powerChild.Name == Constants.FTPTCXString)
                            {
                                m_FTP.Deserialize(powerChild);
                                FTPRead = true;
                            }
                            else if (powerChild.Name == Constants.PowerZoneTCXString)
                            {
                                int zoneIndex = PeekZoneNumber(powerChild);

                                if (zoneIndex != -1)
                                {
                                    ReadPowerZone(zoneIndex, powerChild);
                                    powerZonesRead++;
                                }
                            }
                        }
                    }
                    else if (currentChild.Name == Constants.BikeTCXString)
                    {
                        m_Bikes[bikeProfilesRead].Deserialize(currentChild);
                        bikeProfilesRead++;
                    }
                }

                if (!FTPRead || powerZonesRead != Constants.GarminPowerZoneCount ||
                    bikeProfilesRead != 3)
                {
                    throw new GarminFitnessXmlDeserializationException("Missing information in biking profile XML node", parentNode);
                }
            }
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

        private void ReadPowerZone(int index, XmlNode parentNode)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);

            bool lowRead = false;
            bool highRead = false;

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == Constants.LowTCXString)
                {
                    m_PowerZones[index].Lower.Deserialize(currentChild);
                    lowRead = true;
                }
                else if (currentChild.Name == Constants.HighTCXString &&
                         currentChild.ChildNodes.Count == 1 &&
                         currentChild.FirstChild.GetType() == typeof(XmlText))
                {
                    m_PowerZones[index].Upper.Deserialize(currentChild);
                    highRead = true;
                }
            }

            // Check if all was read successfully
            if (!lowRead || !highRead)
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in profile power zone XML node", parentNode);
            }

            // Reorder both elements, GTC doesn't enforce
            if(m_PowerZones[index].Lower > m_PowerZones[index].Upper)
            {
                GarminFitnessUInt16Range temp = m_PowerZones[index].Lower;

                m_PowerZones[index].Lower = m_PowerZones[index].Upper;
                m_PowerZones[index].Upper = temp;
            }
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
                m_PowerZones[index].Lower = new GarminFitnessUInt16Range(value, Constants.MinPowerInWatts, Constants.MaxPowerProfile);

                TriggerChangedEvent(new PropertyChangedEventArgs("PowerZoneLimit"));
            }
        }

        public void SetPowerHighLimit(int index, UInt16 value)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);

            if (m_PowerZones[index].Upper != value)
            {
                m_PowerZones[index].Upper = new GarminFitnessUInt16Range(value, Constants.MinPowerInWatts, Constants.MaxPowerProfile);

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
                    m_FTP.Value = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("FTP"));
                }
            }
        }

        private GarminFitnessUInt16Range m_FTP = new GarminFitnessUInt16Range(300, Constants.MinPowerInWatts, Constants.MaxPowerProfile);
        private List<GarminFitnessValueRange<GarminFitnessUInt16Range>> m_PowerZones = new List<GarminFitnessValueRange<GarminFitnessUInt16Range>>();
        private GarminBikeProfile[] m_Bikes = new GarminBikeProfile[3];
    }
}
