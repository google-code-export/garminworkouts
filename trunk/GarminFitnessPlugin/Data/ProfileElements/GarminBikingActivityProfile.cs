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
    public class GarminBikingActivityProfile : GarminActivityProfile
    {
        public GarminBikingActivityProfile(GarminCategories category) :
            base(category)
        {
            // Power Zones
            double currentPower = 0.5;
            double powerStep = 0.2;
            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                m_PowerZones.Add(new GarminFitnessValueRange<GarminFitnessDoubleRange>(new GarminFitnessDoubleRange(currentPower, Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTPInternal),
                                                                                       new GarminFitnessDoubleRange((UInt16)(currentPower + powerStep), Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTPInternal)));

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

            // FTP
            m_FTP.Serialize(stream);

            // Zones in percent FTP
            m_PowerZonesInPercentFTP.Serialize(stream);

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
        }

        public void SerializeBikeProfiles(Stream outputStream)
        {
            UInt16 i = 0;

            foreach (GarminBikeProfile bike in m_Bikes)
            {
                FITMessage bikeProfileMessage = new FITMessage(FITGlobalMessageIds.BikeProfile);
                FITMessageField index = new FITMessageField((Byte)FITBikeProfileFieldIds.MessageIndex);
                FITMessageField name = new FITMessageField((Byte)FITBikeProfileFieldIds.Name);
                FITMessageField odometer = new FITMessageField((Byte)FITBikeProfileFieldIds.Odometer);
                FITMessageField customWheelSize = new FITMessageField((Byte)FITBikeProfileFieldIds.CustomWheelSize);
                FITMessageField autoWheelSize = new FITMessageField((Byte)FITBikeProfileFieldIds.AutoWheelSize);
                FITMessageField weight = new FITMessageField((Byte)FITBikeProfileFieldIds.Weight);
                FITMessageField useAutoWheelSize = new FITMessageField((Byte)FITBikeProfileFieldIds.AutoWheelSetting);
                FITMessageField cadenceSensor = new FITMessageField((Byte)FITBikeProfileFieldIds.SpeedCadenceSensorEnabled);
                FITMessageField powerSensor = new FITMessageField((Byte)FITBikeProfileFieldIds.PowerSensorEnabled);

                index.SetUInt16(i);
                name.SetString(bike.Name, (Byte)(Constants.MaxNameLength + 1));
                odometer.SetUInt32((UInt32)(bike.OdometerInMeters * 100));
                customWheelSize.SetUInt16(bike.WheelSize);
                autoWheelSize.SetUInt16(bike.WheelSize);
                weight.SetUInt16((UInt16)Math.Round(Weight.Convert(bike.WeightInPounds, Weight.Units.Pound, Weight.Units.Kilogram) * 10, 0));
                useAutoWheelSize.SetEnum(bike.AutoWheelSize ? (Byte)FITBoolean.True : (Byte)FITBoolean.False);
                cadenceSensor.SetEnum(bike.HasCadenceSensor ? (Byte)FITBoolean.True : (Byte)FITBoolean.False);
                powerSensor.SetEnum(bike.HasPowerSensor ? (Byte)FITBoolean.True : (Byte)FITBoolean.False);

                bikeProfileMessage.AddField(index);
                bikeProfileMessage.AddField(name);
                bikeProfileMessage.AddField(odometer);
                bikeProfileMessage.AddField(customWheelSize);
                bikeProfileMessage.AddField(autoWheelSize);
                bikeProfileMessage.AddField(weight);
                bikeProfileMessage.AddField(useAutoWheelSize);
                bikeProfileMessage.AddField(cadenceSensor);
                bikeProfileMessage.AddField(powerSensor);

                bikeProfileMessage.Serialize(outputStream, i == 0);

                ++i;
            }
        }

        public override void SerializeFITZonesTarget(Stream outputStream)
        {
            FITMessage zonesTargetMessage = new FITMessage(FITGlobalMessageIds.ZonesTarget);
            FITMessageField maxHR = new FITMessageField((Byte)FITZonesTargetFieldIds.MaxHR);
            FITMessageField FTPvalue = new FITMessageField((Byte)FITZonesTargetFieldIds.FTP);
            FITMessageField HRCalcType = new FITMessageField((Byte)FITZonesTargetFieldIds.HRCalcType);
            FITMessageField powerCalcType = new FITMessageField((Byte)FITZonesTargetFieldIds.PowerCalcType);

            maxHR.SetUInt8(MaximumHeartRate);
            FTPvalue.SetUInt16(FTP);
            HRCalcType.SetEnum((Byte)FITHRCalcTypes.Custom);
            powerCalcType.SetEnum((Byte)FITPowerCalcTypes.Custom);

            zonesTargetMessage.AddField(maxHR);
            zonesTargetMessage.AddField(FTPvalue);
            zonesTargetMessage.AddField(HRCalcType);
            zonesTargetMessage.AddField(powerCalcType);

            zonesTargetMessage.Serialize(outputStream);
        }

        public override void SerializeFITPowerZones(Stream outputStream)
        {
            UInt16 i = 0;

            foreach (GarminFitnessValueRange<GarminFitnessDoubleRange> zone in m_PowerZones)
            {
                FITMessage zonesTargetMessage = new FITMessage(FITGlobalMessageIds.PowerZones);
                FITMessageField index = new FITMessageField((Byte)FITPowerZonesFieldIds.MessageIndex);
                FITMessageField zoneName = new FITMessageField((Byte)FITPowerZonesFieldIds.Name);
                FITMessageField highWatts = new FITMessageField((Byte)FITPowerZonesFieldIds.HighWatts);

                if (i == 0)
                {
                    index.SetUInt16(i);
                    zoneName.SetString(String.Format("Power Zone {0}", i), (Byte)(Constants.MaxNameLength + 1));
                    highWatts.SetUInt16((UInt16)(zone.Lower * FTP));

                    zonesTargetMessage.AddField(index);
                    zonesTargetMessage.AddField(zoneName);
                    zonesTargetMessage.AddField(highWatts);

                    zonesTargetMessage.Serialize(outputStream);
                    zonesTargetMessage.Clear();
                }

                index.SetUInt16((UInt16)(i + 1));
                zoneName.SetString(String.Format("Power Zone {0}", i + 1), (Byte)(Constants.MaxNameLength + 1));
                highWatts.SetUInt16((UInt16)(zone.Upper * FTP));

                zonesTargetMessage.AddField(index);
                zonesTargetMessage.AddField(zoneName);
                zonesTargetMessage.AddField(highWatts);

                zonesTargetMessage.Serialize(outputStream, false);

                ++i;
            }
        }

        public new void Deserialize_V8(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(GarminActivityProfile), stream, version);

            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                GarminFitnessUInt16Range lower = new GarminFitnessUInt16Range(0, Constants.MinPowerInWatts, Constants.MaxPowerProfile);
                lower.Deserialize(stream, version);
                SetPowerLowLimit(i, lower);

                GarminFitnessUInt16Range  upper = new GarminFitnessUInt16Range(0, Constants.MinPowerInWatts, Constants.MaxPowerProfile);
                upper.Deserialize(stream, version);
                SetPowerHighLimit(i, upper);
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

            List<GarminFitnessValueRange<GarminFitnessUInt16Range>> powerZones = new List<GarminFitnessValueRange<GarminFitnessUInt16Range>>();

            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                powerZones.Add(new GarminFitnessValueRange<GarminFitnessUInt16Range>(new GarminFitnessUInt16Range(300, Constants.MinPowerInWatts, Constants.MaxPowerProfile),
                                                                                     new GarminFitnessUInt16Range(300, Constants.MinPowerInWatts, Constants.MaxPowerProfile)));

                powerZones[i].Lower.Deserialize(stream, version);
                powerZones[i].Upper.Deserialize(stream, version);
            }

            // Bike profiles
            for (int i = 0; i < Constants.GarminBikeProfileCount; ++i)
            {
                m_Bikes[i].Deserialize(stream, version);
            }

            // FTP (was forgotten in V8)
            m_FTP.Deserialize(stream, version);

            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                SetPowerLowLimit(i, powerZones[i].Lower);
                SetPowerHighLimit(i, powerZones[i].Upper);
            }
        }

        public void Deserialize_V25(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(GarminActivityProfile), stream, version);

            // FTP
            m_FTP.Deserialize(stream, version);

            m_PowerZonesInPercentFTP.Deserialize(stream, version);

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
                low.AppendChild(document.CreateTextNode((m_PowerZones[i].Lower * FTP).ToString("0")));
                currentZoneNode.AppendChild(low);

                // High
                XmlNode high = document.CreateElement(Constants.HighTCXString);
                high.AppendChild(document.CreateTextNode((m_PowerZones[i].Upper * FTP).ToString("0")));
                currentZoneNode.AppendChild(high);

                powerZonesChild.AppendChild(currentZoneNode);
            }

            // Power extension
            XmlNode powerExtensionChild = document.CreateElement(Constants.PowerExtensionTCXString);
            attributeNode = document.CreateAttribute("xmlns");
            attributeNode.Value = "http://www.garmin.com/xmlschemas/ProfileExtension/v2";
            powerExtensionChild.Attributes.Append(attributeNode);

            GarminFitnessString wattsType = new GarminFitnessString("Watts");
            wattsType.Serialize(powerExtensionChild, "Type", document);

            currentChild.AppendChild(powerZonesChild);
            currentChild.AppendChild(powerExtensionChild);
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

                if (!FTPRead ||
                    powerZonesRead != Constants.GarminPowerZoneCount ||
                    bikeProfilesRead != 3)
                {
                    throw new GarminFitnessXmlDeserializationException("Missing information in biking profile XML node", parentNode);
                }
            }
        }

        public void DeserializeBikeProfile(FITMessage bikeProfileMessage)
        {
            FITMessageField bikeIndex = bikeProfileMessage.GetField((Byte)FITBikeProfileFieldIds.MessageIndex);
            FITMessageField bikeName = bikeProfileMessage.GetField((Byte)FITBikeProfileFieldIds.Name);
            FITMessageField odometer = bikeProfileMessage.GetField((Byte)FITBikeProfileFieldIds.Odometer);
            FITMessageField customWheelSize = bikeProfileMessage.GetField((Byte)FITBikeProfileFieldIds.CustomWheelSize);
            FITMessageField autoWheelSize = bikeProfileMessage.GetField((Byte)FITBikeProfileFieldIds.AutoWheelSize);
            FITMessageField weight = bikeProfileMessage.GetField((Byte)FITBikeProfileFieldIds.Weight);
            FITMessageField autoWheelSetting = bikeProfileMessage.GetField((Byte)FITBikeProfileFieldIds.AutoWheelSetting);
            FITMessageField speedCadenceSensorEnabled = bikeProfileMessage.GetField((Byte)FITBikeProfileFieldIds.SpeedCadenceSensorEnabled);
            FITMessageField powerSensorEnabled = bikeProfileMessage.GetField((Byte)FITBikeProfileFieldIds.PowerSensorEnabled);

            if (bikeIndex != null)
            {
                GarminBikeProfile profile = m_Bikes[bikeIndex.GetUInt16()];

                if (bikeName != null)
                {
                    profile.Name = bikeName.GetString();
                }

                if (odometer != null)
                {
                    profile.OdometerInMeters = Math.Min(odometer.GetUInt32() / 100.0, Constants.MaxOdometerMeters);
                }

                if (weight != null)
                {
                    profile.WeightInPounds = Weight.Convert(weight.GetUInt16() / 10.0, Weight.Units.Kilogram, Weight.Units.Pound);
                }

                if (autoWheelSetting != null)
                {
                    bool useAutoWheelSize = (autoWheelSetting.GetEnum() == (Byte)FITBoolean.True);

                    if (useAutoWheelSize && autoWheelSize != null)
                    {
                        profile.AutoWheelSize = true;
                        profile.WheelSize = autoWheelSize.GetUInt16();
                    }
                    else if (!useAutoWheelSize && customWheelSize != null)
                    {
                        profile.AutoWheelSize = false;
                        profile.WheelSize = customWheelSize.GetUInt16();
                    }
                }

                if (speedCadenceSensorEnabled != null)
                {
                    profile.HasCadenceSensor = (speedCadenceSensorEnabled.GetEnum() == (Byte)FITBoolean.True);
                }

                if (powerSensorEnabled != null)
                {
                    profile.HasPowerSensor = (powerSensorEnabled.GetEnum() == (Byte)FITBoolean.True);
                }
            }
            else
            {
                throw new FITParserException("Invalid bike profile index");
            }
        }

        public override void DeserializeZonesTargetFromFIT(FITMessage zonesTargetMessage)
        {
            base.DeserializeZonesTargetFromFIT(zonesTargetMessage);

            FITMessageField FTPField = zonesTargetMessage.GetField((Byte)FITZonesTargetFieldIds.FTP);

            if (FTPField != null)
            {
                FTP = FTPField.GetUInt16();
            }
        }

        public override void DeserializePowerZonesFromFIT(FITMessage powerZonesMessage)
        {
            base.DeserializePowerZonesFromFIT(powerZonesMessage);

            FITMessageField zoneIndex = powerZonesMessage.GetField((Byte)FITPowerZonesFieldIds.MessageIndex);
            FITMessageField zoneUpperValue = powerZonesMessage.GetField((Byte)FITPowerZonesFieldIds.HighWatts);

            if (zoneIndex != null && zoneUpperValue != null)
            {
                bool currentPercentFTP = PowerZonesInPercentFTP;
                UInt16 index = zoneIndex.GetUInt16();

                PowerZonesInPercentFTP = false;

                if (index > Constants.GarminPowerZoneCount)
                {
                    throw new FITParserException("Invalid index for power zone");
                }

                if (index == 0)
                {
                    SetPowerLowLimit(index, zoneUpperValue.GetUInt16());
                }
                else
                {
                    SetPowerHighLimit(index - 1, zoneUpperValue.GetUInt16());

                    if (index < Constants.GarminPowerZoneCount)
                    {
                        SetPowerLowLimit(index, zoneUpperValue.GetUInt16());
                    }
                }

                PowerZonesInPercentFTP = currentPercentFTP;
            }
            else
            {
                throw new FITParserException("Missing fields for power zone");
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
            GarminFitnessValueRange<GarminFitnessUInt16Range> zone = new GarminFitnessValueRange<GarminFitnessUInt16Range>(new GarminFitnessUInt16Range(0),
                                                                                                                           new GarminFitnessUInt16Range(0));

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == Constants.LowTCXString)
                {
                    zone.Lower.Deserialize(currentChild);
                    lowRead = true;
                }
                else if (currentChild.Name == Constants.HighTCXString &&
                         currentChild.ChildNodes.Count == 1 &&
                         currentChild.FirstChild.GetType() == typeof(XmlText))
                {
                    zone.Upper.Deserialize(currentChild);
                    highRead = true;
                }
            }

            // Check if all was read successfully
            if (!lowRead || !highRead)
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in profile power zone XML node", parentNode);
            }
            else
            {
                m_PowerZones[index].Lower.Value = zone.Lower / (double)FTP;
                m_PowerZones[index].Upper.Value = zone.Upper / (double)FTP;
            }

            // Reorder both elements, GTC doesn't enforce
            if(m_PowerZones[index].Lower > m_PowerZones[index].Upper)
            {
                GarminFitnessDoubleRange temp = m_PowerZones[index].Lower;

                m_PowerZones[index].Lower = m_PowerZones[index].Upper;
                m_PowerZones[index].Upper = temp;
            }
        }

        public UInt16 GetPowerLowLimit(int index)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);
            UInt16 lowValue = (UInt16)(m_PowerZones[index].Lower * 100);

            if (PowerZonesInPercentFTP)
            {
                lowValue = (UInt16)Utils.Clamp(lowValue, Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTP);
            }
            else
            {
                lowValue = (UInt16)Utils.Clamp(Math.Round(m_PowerZones[index].Lower * FTP, 0),
                                               Constants.MinPowerInWatts,
                                               Constants.MaxPowerProfile);
            }

            return lowValue;
        }

        public UInt16 GetPowerHighLimit(int index)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);
            UInt16 highValue = (UInt16)(m_PowerZones[index].Upper * 100);

            if (PowerZonesInPercentFTP)
            {
                highValue = (UInt16)Utils.Clamp(highValue, Constants.MinPowerInPercentFTP, Constants.MaxPowerInPercentFTP);
            }
            else
            {
                highValue = (UInt16)Utils.Clamp(Math.Round(m_PowerZones[index].Upper * FTP, 0),
                                               Constants.MinPowerInWatts,
                                               Constants.MaxPowerProfile);
            }

            return highValue;
        }

        public void SetPowerLowLimit(int index, UInt16 value)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);
            double FTPBasedValue = value / 100.0;

            if (!PowerZonesInPercentFTP)
            {
                FTPBasedValue = value / (double)FTP;
            }

            if (m_PowerZones[index].Lower != FTPBasedValue)
            {
                m_PowerZones[index].Lower.Value = FTPBasedValue;

                // Reorder both elements
                if (m_PowerZones[index].Lower > m_PowerZones[index].Upper)
                {
                    m_PowerZones[index].Upper.Value = m_PowerZones[index].Lower;
                }

                TriggerChangedEvent(new PropertyChangedEventArgs("PowerZoneLimit"));
            }
        }

        public void SetPowerHighLimit(int index, UInt16 value)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminPowerZoneCount);
            double FTPBasedValue = value / 100.0;

            if (!PowerZonesInPercentFTP)
            {
                FTPBasedValue = value / (double)FTP;
            }

            if (m_PowerZones[index].Upper != FTPBasedValue)
            {
                m_PowerZones[index].Upper.Value = FTPBasedValue;

                // Reorder both elements
                if (m_PowerZones[index].Lower > m_PowerZones[index].Upper)
                {
                    m_PowerZones[index].Lower.Value = m_PowerZones[index].Upper;
                }

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
                    if (!PowerZonesInPercentFTP)
                    {
                        // Update limit values as they will change since stored in %FTP
                        for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
                        {
                            double currentLowValue = m_PowerZones[i].Lower;
                            double currentHighValue = m_PowerZones[i].Upper;

                            m_PowerZones[i].Lower.Value = Utils.Clamp((currentLowValue * FTP) / value,
                                                                      Constants.MinPowerInPercentFTP,
                                                                      Constants.MaxPowerInPercentFTPInternal);
                            m_PowerZones[i].Upper.Value = Utils.Clamp((currentHighValue * FTP) / value,
                                                                      Constants.MinPowerInPercentFTP,
                                                                      Constants.MaxPowerInPercentFTPInternal);
                        }
                    }

                    m_FTP.Value = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("FTP"));
                }
            }
        }

        public bool PowerZonesInPercentFTP
        {
            get { return m_PowerZonesInPercentFTP; }
            set
            {
                if (PowerZonesInPercentFTP != value)
                {
                    m_PowerZonesInPercentFTP.Value = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("PowerZonesInPercentFTP"));
                }
            }
        }

        private GarminFitnessBool m_PowerZonesInPercentFTP = new GarminFitnessBool(false);
        private GarminFitnessUInt16Range m_FTP = new GarminFitnessUInt16Range(300, Constants.MinPowerInWatts, Constants.MaxPowerFTP);
        private List<GarminFitnessValueRange<GarminFitnessDoubleRange>> m_PowerZones = new List<GarminFitnessValueRange<GarminFitnessDoubleRange>>();
        private GarminBikeProfile[] m_Bikes = new GarminBikeProfile[3];
    }
}
