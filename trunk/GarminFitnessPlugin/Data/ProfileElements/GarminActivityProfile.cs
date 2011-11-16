using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.View;

namespace GarminFitnessPlugin.Data
{
    class GarminActivityProfile : IPluginSerializable, IXMLSerializable
    {
        public enum HRReferential
        {
            HRReferential_BPM = 0,
            HRReferential_PercentMax,
            HRReferential_PercentHRR,
            HRReferential_Count,
        }

        public GarminActivityProfile(GarminCategories category)
        {
            m_Category = category;

            // HR Zones (always stored in % Max)
            float currentLowHR = 0.5f;
            const float stepHR = 0.1f;
            for(int i = 0; i < Constants.GarminHRZoneCount; ++i)
            {
                m_HeartRateZones.Add(new GarminFitnessValueRange<GarminFitnessDoubleRange>(new GarminFitnessDoubleRange(currentLowHR, 0, 1),
                                                                                           new GarminFitnessDoubleRange(currentLowHR + stepHR, 0, 1)));

                currentLowHR += stepHR;
            }

            // Speed Zones
            float currentLowSpeed = 5;
            const float stepSpeed = 1;
            for(int i = 0; i < Constants.GarminSpeedZoneCount; ++i)
            {
                String zoneName = GarminFitnessView.GetLocalizedString("GTCSpeedZone" + (i + 1).ToString() + "Text");

                m_SpeedZones.Add(new GarminFitnessNamedSpeedZone(currentLowSpeed, currentLowSpeed + stepSpeed, zoneName));

                currentLowSpeed += stepSpeed;
            }
        }

        public override void Serialize(Stream stream)
        {
            m_MaxHeartRate.Serialize(stream);

            m_GearWeightInPounds.Serialize(stream);

            m_HRReferential.Serialize(stream);

            // HR zones
            for (int i = 0; i < m_HeartRateZones.Count; ++i)
            {
                m_HeartRateZones[i].Lower.Serialize(stream);

                m_HeartRateZones[i].Upper.Serialize(stream);
            }

            m_SpeedIsInPace.Serialize(stream);

            // Speed Zones
            for (int i = 0; i < m_SpeedZones.Count; ++i)
            {
                m_SpeedZones[i].InternalLow.Serialize(stream);

                m_SpeedZones[i].InternalHigh.Serialize(stream);

                m_SpeedZones[i].InternalName.Serialize(stream);
            }
        }
        
        public void SerializeToFITSport(Stream outputStream)
        {
            FITMessage sportMessage = new FITMessage(FITGlobalMessageIds.Sport);
            FITMessageField sport = new FITMessageField((Byte)FITSportFieldIds.Sport);
            FITMessageField subSport = new FITMessageField((Byte)FITSportFieldIds.SubSport);

            sport.SetEnum((Byte)Utils.GetFITSport(Category));
            subSport.SetEnum(0);

            sportMessage.AddField(sport);
            sportMessage.AddField(subSport);
            sportMessage.Serialize(outputStream);

            SerializeFITZonesTarget(outputStream);
            SerializeFITHRZones(outputStream);
            SerializeFITSpeedZones(outputStream);
            SerializeFITPowerZones(outputStream);
        }

        public virtual void SerializeFITZonesTarget(Stream outputStream)
        {
            FITMessage zonesTargetMessage = new FITMessage(FITGlobalMessageIds.ZonesTarget);
            FITMessageField maxHR = new FITMessageField((Byte)FITZonesTargetFieldIds.MaxHR);
            FITMessageField FTP = new FITMessageField((Byte)FITZonesTargetFieldIds.FTP);
            FITMessageField HRCalcType = new FITMessageField((Byte)FITZonesTargetFieldIds.HRCalcType);
            FITMessageField powerCalcType = new FITMessageField((Byte)FITZonesTargetFieldIds.PowerCalcType);

            maxHR.SetUInt8(MaximumHeartRate);
            FTP.SetUInt16(0);
            HRCalcType.SetEnum((Byte)FITHRCalcTypes.Custom);
            powerCalcType.SetEnum((Byte)FITPowerCalcTypes.Custom);

            zonesTargetMessage.AddField(maxHR);
            zonesTargetMessage.AddField(FTP);
            zonesTargetMessage.AddField(HRCalcType);
            zonesTargetMessage.AddField(powerCalcType);

            zonesTargetMessage.Serialize(outputStream);
        }

        public virtual void SerializeFITHRZones(Stream outputStream)
        {
            UInt16 i = 0;

            foreach (GarminFitnessValueRange<GarminFitnessDoubleRange> zone in m_HeartRateZones)
            {
                FITMessage zonesTargetMessage = new FITMessage(FITGlobalMessageIds.HRZones);
                FITMessageField index = new FITMessageField((Byte)FITHRZonesFieldIds.MessageIndex);
                FITMessageField zoneName = new FITMessageField((Byte)FITHRZonesFieldIds.Name);
                FITMessageField highBPM = new FITMessageField((Byte)FITHRZonesFieldIds.HighBPM);

                if (i == 0)
                {
                    index.SetUInt16(i);
                    zoneName.SetString(String.Format("HR Zone {0}", i), 16);
                    highBPM.SetUInt8((Byte)(zone.Lower * MaximumHeartRate));

                    zonesTargetMessage.AddField(index);
                    zonesTargetMessage.AddField(zoneName);
                    zonesTargetMessage.AddField(highBPM);

                    zonesTargetMessage.Serialize(outputStream);
                    zonesTargetMessage.Clear();
                }

                index.SetUInt16((UInt16)(i + 1));
                zoneName.SetString(String.Format("HR Zone {0}", i + 1), 16);
                highBPM.SetUInt8((Byte)(zone.Upper * MaximumHeartRate));

                zonesTargetMessage.AddField(index);
                zonesTargetMessage.AddField(zoneName);
                zonesTargetMessage.AddField(highBPM);

                zonesTargetMessage.Serialize(outputStream);

                ++i;
            }
        }

        public virtual void SerializeFITSpeedZones(Stream outputStream)
        {
            UInt16 i = 0;

            foreach (GarminFitnessNamedSpeedZone zone in m_SpeedZones)
            {
                FITMessage zonesTargetMessage = new FITMessage(FITGlobalMessageIds.SpeedZones);
                FITMessageField index = new FITMessageField((Byte)FITSpeedZonesFieldIds.MessageIndex);
                FITMessageField zoneName = new FITMessageField((Byte)FITSpeedZonesFieldIds.Name);
                FITMessageField highSpeed = new FITMessageField((Byte)FITSpeedZonesFieldIds.HighSpeed);

                if (i == 0)
                {
                    index.SetUInt16(i);
                    zoneName.SetString(zone.Name, 16);
                    highSpeed.SetUInt16((UInt16)(zone.Low * 1000));

                    zonesTargetMessage.AddField(index);
                    zonesTargetMessage.AddField(zoneName);
                    zonesTargetMessage.AddField(highSpeed);

                    zonesTargetMessage.Serialize(outputStream);
                    zonesTargetMessage.Clear();
                }

                index.SetUInt16((UInt16)(i + 1));
                zoneName.SetString(zone.Name, 16);
                highSpeed.SetUInt16((UInt16)(zone.High * 1000));

                zonesTargetMessage.AddField(index);
                zonesTargetMessage.AddField(zoneName);
                zonesTargetMessage.AddField(highSpeed);

                zonesTargetMessage.Serialize(outputStream);

                ++i;
            }
        }

        public virtual void SerializeFITPowerZones(Stream outputStream)
        {
        }

        public void Deserialize_V8(Stream stream, DataVersion version)
        {
            m_MaxHeartRate.Deserialize(stream, version);

            GarminFitnessDoubleRange gearWeightinKg = new GarminFitnessDoubleRange(Weight.Convert(m_GearWeightInPounds, Weight.Units.Pound, Weight.Units.Kilogram));

            gearWeightinKg.Deserialize(stream, version);
            SetGearWeightInUnits(gearWeightinKg, Weight.Units.Kilogram);

            GarminFitnessBool HRIsInPercentMax = new GarminFitnessBool(true);
            HRIsInPercentMax.Deserialize(stream, version);
            HRZonesReferential = HRIsInPercentMax ? HRReferential.HRReferential_PercentMax : HRReferential.HRReferential_BPM;

            for (int i = 0; i < m_HeartRateZones.Count; ++i)
            {
                m_HeartRateZones[i].Lower.Deserialize(stream, version);

                m_HeartRateZones[i].Upper.Deserialize(stream, version);
            }

            m_SpeedIsInPace.Deserialize(stream, version);

            GarminFitnessDoubleRange lowLimit = new GarminFitnessDoubleRange(Constants.MinSpeedMetric, Constants.MinSpeedMetric, Constants.MaxSpeedMetric);
            GarminFitnessDoubleRange highLimit = new GarminFitnessDoubleRange(Constants.MinSpeedMetric, Constants.MinSpeedMetric, Constants.MaxSpeedMetric);
            for (int i = 0; i < m_SpeedZones.Count; ++i)
            {
                lowLimit.Deserialize(stream, version);
                m_SpeedZones[i].Low = Length.Convert(lowLimit, Length.Units.Kilometer, Length.Units.Meter) / Constants.SecondsPerHour;

                highLimit.Deserialize(stream, version);
                m_SpeedZones[i].High = Length.Convert(highLimit, Length.Units.Kilometer, Length.Units.Meter) / Constants.SecondsPerHour;

                m_SpeedZones[i].InternalName.Deserialize(stream, version);
            }
        }

        public void Deserialize_V10(Stream stream, DataVersion version)
        {
            m_MaxHeartRate.Deserialize(stream, version);

            GarminFitnessDoubleRange gearWeightinKg = new GarminFitnessDoubleRange(Weight.Convert(m_GearWeightInPounds, Weight.Units.Pound, Weight.Units.Kilogram));

            gearWeightinKg.Deserialize(stream, version);
            SetGearWeightInUnits(gearWeightinKg, Weight.Units.Kilogram);

            GarminFitnessBool HRIsInPercentMax = new GarminFitnessBool(true);
            HRIsInPercentMax.Deserialize(stream, version);
            HRZonesReferential = HRIsInPercentMax ? HRReferential.HRReferential_PercentMax : HRReferential.HRReferential_BPM;

            for (int i = 0; i < m_HeartRateZones.Count; ++i)
            {
                m_HeartRateZones[i].Lower.Deserialize(stream, version);

                m_HeartRateZones[i].Upper.Deserialize(stream, version);
            }

            m_SpeedIsInPace.Deserialize(stream, version);

            for (int i = 0; i < m_SpeedZones.Count; ++i)
            {
                m_SpeedZones[i].InternalLow.Deserialize(stream, version);

                m_SpeedZones[i].InternalHigh.Deserialize(stream, version);

                m_SpeedZones[i].InternalName.Deserialize(stream, version);
            }
        }

        public void Deserialize_V22(Stream stream, DataVersion version)
        {
            m_MaxHeartRate.Deserialize(stream, version);

            m_GearWeightInPounds.Deserialize(stream, version);

            GarminFitnessBool HRIsInPercentMax = new GarminFitnessBool(true);
            HRIsInPercentMax.Deserialize(stream, version);
            HRZonesReferential = HRIsInPercentMax ? HRReferential.HRReferential_PercentMax : HRReferential.HRReferential_BPM;

            for (int i = 0; i < m_HeartRateZones.Count; ++i)
            {
                m_HeartRateZones[i].Lower.Deserialize(stream, version);

                m_HeartRateZones[i].Upper.Deserialize(stream, version);
            }

            m_SpeedIsInPace.Deserialize(stream, version);

            for (int i = 0; i < m_SpeedZones.Count; ++i)
            {
                m_SpeedZones[i].InternalLow.Deserialize(stream, version);

                m_SpeedZones[i].InternalHigh.Deserialize(stream, version);

                m_SpeedZones[i].InternalName.Deserialize(stream, version);
            }
        }

        public void Deserialize_V23(Stream stream, DataVersion version)
        {
            m_MaxHeartRate.Deserialize(stream, version);

            m_GearWeightInPounds.Deserialize(stream, version);

            GarminFitnessByteRange HRZoneReference = new GarminFitnessByteRange(0);
            HRZoneReference.Deserialize(stream, version);
            HRZonesReferential = (HRReferential)(Byte)HRZoneReference;

            for (int i = 0; i < m_HeartRateZones.Count; ++i)
            {
                m_HeartRateZones[i].Lower.Deserialize(stream, version);

                m_HeartRateZones[i].Upper.Deserialize(stream, version);
            }

            m_SpeedIsInPace.Deserialize(stream, version);

            for (int i = 0; i < m_SpeedZones.Count; ++i)
            {
                m_SpeedZones[i].InternalLow.Deserialize(stream, version);

                m_SpeedZones[i].InternalHigh.Deserialize(stream, version);

                m_SpeedZones[i].InternalName.Deserialize(stream, version);
            }
        }

        public virtual void DeserializeZonesTargetFromFIT(FITMessage zonesTargetMessage)
        {
            FITMessageField maxHR = zonesTargetMessage.GetField((Byte)FITZonesTargetFieldIds.MaxHR);

            if (maxHR != null)
            {
                MaximumHeartRate = maxHR.GetUInt8();
            }
        }

        public virtual void DeserializeHRZonesFromFIT(FITMessage HRZonesMessage)
        {
            FITMessageField zoneIndex = HRZonesMessage.GetField((Byte)FITHRZonesFieldIds.MessageIndex);
            FITMessageField zoneUpperValue = HRZonesMessage.GetField((Byte)FITHRZonesFieldIds.HighBPM);

            if (zoneIndex != null && zoneUpperValue != null)
            {
                HRReferential currentReferential = HRZonesReferential;
                UInt16 index = zoneIndex.GetUInt16();

                // Always deserialize in BPM
                HRZonesReferential = HRReferential.HRReferential_BPM;
            
                if (index > Constants.GarminHRZoneCount)
                {
                    throw new FITParserException("Invalid index for HR zone");
                }

                if (index == 0)
                {
                    SetHeartRateLowLimit(0, zoneUpperValue.GetUInt8());
                }
                else
                {
                    SetHeartRateHighLimit(index - 1, zoneUpperValue.GetUInt8());

                    if (index < Constants.GarminHRZoneCount)
                    {
                        SetHeartRateLowLimit(index, zoneUpperValue.GetUInt8());
                    }
                }

                HRZonesReferential = currentReferential;
            }
            else
            {
                throw new FITParserException("Missing fields for HR zone");
            }
        }

        public virtual void DeserializeSpeedZonesFromFIT(FITMessage speedZonesMessage)
        {
            FITMessageField zoneIndex = speedZonesMessage.GetField((Byte)FITSpeedZonesFieldIds.MessageIndex);
            FITMessageField zoneUpperValue = speedZonesMessage.GetField((Byte)FITSpeedZonesFieldIds.HighSpeed);
            FITMessageField zoneName = speedZonesMessage.GetField((Byte)FITSpeedZonesFieldIds.Name);

            if (zoneIndex != null &&
                zoneUpperValue != null &&
                zoneName != null)
            {
                UInt16 index = zoneIndex.GetUInt16();

                if (index > Constants.GarminSpeedZoneCount)
                {
                    throw new FITParserException("Invalid index for for speed zone");
                }

                if (index == 0)
                {
                    SetSpeedLowLimitInMetersPerSecond(index, zoneUpperValue.GetUInt16() / 1000.0);
                    SetSpeedName(index, zoneName.GetString());
                }
                else
                {
                    SetSpeedName(index - 1, zoneName.GetString());
                    SetSpeedHighLimitInMetersPerSecond(index - 1, zoneUpperValue.GetUInt16() / 1000.0);

                    if (index < Constants.GarminSpeedZoneCount)
                    {
                        SetSpeedLowLimitInMetersPerSecond(index, zoneUpperValue.GetUInt16() / 1000.0);
                    }
                }
            }
            else
            {
                throw new FITParserException("Missing fields for speed zone");
            }
        }

        public virtual void DeserializePowerZonesFromFIT(FITMessage powerZonesMessage)
        {
        }

        public virtual void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            XmlAttribute attributeNode;
            XmlNode activityNode, currentChild;

            activityNode = document.CreateElement(nodeName);
            parentNode.AppendChild(activityNode);

            attributeNode = document.CreateAttribute("Sport");
            attributeNode.Value = Constants.GarminCategoryTCXString[(int)Category];
            activityNode.Attributes.Append(attributeNode);

            attributeNode = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            attributeNode.Value = "ProfileActivity_t";
            activityNode.Attributes.Append(attributeNode);

            // Maximum heart rate
            currentChild = document.CreateElement(Constants.MaxHRBPMTCXString);
            activityNode.AppendChild(currentChild);
            m_MaxHeartRate.Serialize(currentChild, Constants.ValueTCXString, document);

            // Resting HR
            currentChild = document.CreateElement(Constants.RestHRBPMTCXString);
            activityNode.AppendChild(currentChild);
            GarminProfileManager.Instance.UserProfile.InternalRestingHeartRate.Serialize(currentChild, Constants.ValueTCXString, document);

            GarminFitnessDoubleRange gearWeightinKg = new GarminFitnessDoubleRange(Weight.Convert(m_GearWeightInPounds, Weight.Units.Pound, Weight.Units.Kilogram));

            gearWeightinKg.Serialize(activityNode, Constants.GearWeightTCXString, document);

            // HR zones
            for (int i = 0; i < Constants.GarminHRZoneCount; ++i)
            {
                currentChild = document.CreateElement(Constants.HeartRateZonesTCXString);
                activityNode.AppendChild(currentChild);

                // Number
                XmlNode numberNode = document.CreateElement("Number");
                numberNode.AppendChild(document.CreateTextNode((i + 1).ToString()));
                currentChild.AppendChild(numberNode);

                // View as BPM or % max
                GarminFitnessBool HRIsInPercentMax = new GarminFitnessBool(HRZonesReferential != HRReferential.HRReferential_BPM,
                                                                           Constants.PercentMaxTCXString,
                                                                           Constants.BPMTCXString);
                HRIsInPercentMax.Serialize(currentChild, Constants.ViewAsTCXString, document);

                // Low
                GarminFitnessByteRange lowLimit = new GarminFitnessByteRange((Byte)(m_HeartRateZones[i].Lower * MaximumHeartRate));
                XmlNode low = document.CreateElement(Constants.LowTCXString);
                currentChild.AppendChild(low);
                lowLimit.Serialize(low, Constants.ValueTCXString, document);

                // High
                GarminFitnessByteRange highLimit = new GarminFitnessByteRange((Byte)(m_HeartRateZones[i].Upper * MaximumHeartRate));
                XmlNode high = document.CreateElement(Constants.HighTCXString);
                currentChild.AppendChild(high);
                highLimit.Serialize(high, Constants.ValueTCXString, document);
            }

            // Speed zones
            for (int i = 0; i < Constants.GarminSpeedZoneCount; ++i)
            {
                currentChild = document.CreateElement(Constants.SpeedZonesTCXString);
                activityNode.AppendChild(currentChild);

                // Number
                XmlNode numberNode = document.CreateElement("Number");
                numberNode.AppendChild(document.CreateTextNode((i + 1).ToString()));
                currentChild.AppendChild(numberNode);

                // Name
                m_SpeedZones[i].InternalName.Serialize(currentChild, "Name", document);

                XmlNode valueChild = document.CreateElement(Constants.ValueTCXString);
                currentChild.AppendChild(valueChild);

                // View as pace or speed
                m_SpeedIsInPace.Serialize(valueChild, Constants.ViewAsTCXString, document);

                // Low
                m_SpeedZones[i].InternalLow.Serialize(valueChild, Constants.LowInMeterPerSecTCXString, document);

                // High
                m_SpeedZones[i].InternalHigh.Serialize(valueChild, Constants.HighInMeterPerSecTCXString, document);
            }
        }

        public virtual void Deserialize(XmlNode parentNode)
        {
            bool weightRead = false;
            int HRZonesRead = 0;
            int speedZonesRead = 0;

            ReadMaxHR(parentNode);

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == Constants.RestHRBPMTCXString &&
                    currentChild.ChildNodes.Count == 1 &&
                    currentChild.FirstChild.Name == Constants.ValueTCXString)
                {
                    GarminProfileManager.Instance.UserProfile.InternalRestingHeartRate.Deserialize(currentChild.FirstChild);
                }
                else if (currentChild.Name == Constants.GearWeightTCXString)
                {
                    GarminFitnessDoubleRange gearWeightinKg = new GarminFitnessDoubleRange(0);

                    gearWeightinKg.Deserialize(currentChild);
                    SetGearWeightInUnits(gearWeightinKg, Weight.Units.Kilogram);
                    weightRead = true;
                }
                else if (currentChild.Name == Constants.HeartRateZonesTCXString)
                {
                    int zoneIndex = PeekZoneNumber(currentChild);

                    if (zoneIndex != -1)
                    {
                        ReadHRZone(zoneIndex, currentChild);
                            HRZonesRead++;
                    }
                }
                else if (currentChild.Name == Constants.SpeedZonesTCXString)
                {
                    int zoneIndex = PeekZoneNumber(currentChild);

                    if (zoneIndex != -1)
                    {
                        ReadSpeedZone(zoneIndex, currentChild);
                        speedZonesRead++;
                    }
                }
            }

            // Check if all was read successfully
            if (!weightRead ||
                HRZonesRead != Constants.GarminHRZoneCount ||
                speedZonesRead != Constants.GarminSpeedZoneCount)
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in activity profile XML node", parentNode);
            }
        }

        public void Serialize(GarXFaceNet._FitnessUserProfile._Activity activityProfile)
        {
            HRReferential HRReference = HRZonesReferential;

            HRZonesReferential = HRReferential.HRReferential_BPM;
            activityProfile.SetGearWeight((float)Weight.Convert(GearWeightInPounds, Weight.Units.Pound, Weight.Units.Kilogram));
            activityProfile.SetMaxHeartRate(MaximumHeartRate);

            for (UInt32 i = 0; i < 5; ++i)
            {
                GarXFaceNet._FitnessUserProfile._Activity._HeartRateZone hrZone = activityProfile.GetHeartRateZone(i);

                hrZone.SetLowHeartRate((UInt32)GetHeartRateLowLimit((int)i));
                hrZone.SetHighHeartRate((UInt32)GetHeartRateHighLimit((int)i));
            }

            for (UInt32 i = 0; i < 10; ++i)
            {
                GarXFaceNet._FitnessUserProfile._Activity._SpeedZone speedZone = activityProfile.GetSpeedZone(i);

                speedZone.SetName(m_SpeedZones[(int)i].Name);
                speedZone.SetLowSpeed((float)m_SpeedZones[(int)i].Low);
                speedZone.SetHighSpeed((float)m_SpeedZones[(int)i].High);
            }

            HRZonesReferential = HRReference;
        }

        public void Deserialize(GarXFaceNet._FitnessUserProfile._Activity activityProfile)
        {
            SetGearWeightInUnits(activityProfile.GetGearWeight(), Weight.Units.Kilogram);
            MaximumHeartRate = (Byte)activityProfile.GetMaxHeartRate();

            for (UInt32 i = 0; i < 5; ++i)
            {
                GarXFaceNet._FitnessUserProfile._Activity._HeartRateZone hrZone = activityProfile.GetHeartRateZone(i);
            }
        }

        private void ReadHRZone(int index, XmlNode parentNode)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            bool viewAsRead = false;
            bool lowRead = false;
            bool highRead = false;
            GarminFitnessByteRange lowLimit = new GarminFitnessByteRange(0);
            GarminFitnessByteRange highLimit = new GarminFitnessByteRange(0);

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == Constants.ViewAsTCXString)
                {
                    GarminFitnessBool HRIsInPercentMax = new GarminFitnessBool(true,
                                                                               Constants.PercentMaxTCXString,
                                                                               Constants.BPMTCXString);
                    HRIsInPercentMax.Deserialize(currentChild);

                    // When we read % max, make sure we don't overwrite the BPM vs %Max/HRR since there
                    //  is no difference between both of these in the TCX file
                    if (!HRIsInPercentMax ||
                        HRZonesReferential == HRReferential.HRReferential_BPM)
                    {
                        HRZonesReferential = HRIsInPercentMax ? HRReferential.HRReferential_PercentMax : HRReferential.HRReferential_BPM;
                    }
                    viewAsRead = true;
                }
                else if (currentChild.Name == Constants.LowTCXString &&
                         currentChild.ChildNodes.Count == 1 &&
                         currentChild.FirstChild.Name == Constants.ValueTCXString)
                {
                    lowLimit.Deserialize(currentChild.FirstChild);
                    lowRead = true;
                }
                else if (currentChild.Name == Constants.HighTCXString &&
                         currentChild.ChildNodes.Count == 1 &&
                         currentChild.FirstChild.Name == Constants.ValueTCXString)
                {
                    highLimit.Deserialize(currentChild.FirstChild);
                    highRead = true;
                }
            }

            // Check if all was read successfully
            if (!viewAsRead || !lowRead || !highRead)
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in heart rate zone XML node", parentNode);
            }

            m_HeartRateZones[index].Lower = new GarminFitnessDoubleRange((double)Math.Min(lowLimit, highLimit) / MaximumHeartRate, 0, 1);
            m_HeartRateZones[index].Upper = new GarminFitnessDoubleRange((double)Math.Max(lowLimit, highLimit) / MaximumHeartRate, 0, 1);
        }

        private void ReadSpeedZone(int index, XmlNode parentNode)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminSpeedZoneCount);

            bool nameRead = false;
            bool viewAsRead = false;
            bool lowRead = false;
            bool highRead = false;
            GarminFitnessDoubleRange lowLimit = new GarminFitnessDoubleRange(0);
            GarminFitnessDoubleRange highLimit = new GarminFitnessDoubleRange(0);

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == "Name")
                {
                    m_SpeedZones[index].InternalName.Deserialize(currentChild);
                    nameRead = true;
                }
                else if (currentChild.Name == Constants.ValueTCXString)
                {
                    XmlNode valueNode = currentChild;

                    for (int j = 0; j < valueNode.ChildNodes.Count; ++j)
                    {
                        XmlNode valueChild = valueNode.ChildNodes[j];

                        if (valueChild.Name == Constants.ViewAsTCXString)
                        {
                            m_SpeedIsInPace.Deserialize(valueChild);
                            viewAsRead = true;
                        }
                        else if (valueChild.Name == Constants.LowInMeterPerSecTCXString)
                        {
                            lowLimit.Deserialize(valueChild);
                            lowRead = true;
                        }
                        else if (valueChild.Name == Constants.HighInMeterPerSecTCXString)
                        {
                            highLimit.Deserialize(valueChild);
                            highRead = true;
                        }
                    }
                }
            }

            // Check if all was read successfully
            if (!nameRead || !viewAsRead || !lowRead || !highRead)
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in activity profile XML node", parentNode);
            }

            m_SpeedZones[index].Low = Math.Min(lowLimit, highLimit);
            m_SpeedZones[index].High = Math.Max(lowLimit, highLimit);
        }

        protected int PeekZoneNumber(XmlNode zoneNode)
        {
            for (int i = 0; i < zoneNode.ChildNodes.Count; ++i)
            {
                XmlNode child = zoneNode.ChildNodes[i];

                if (child.Name == "Number" && child.ChildNodes.Count == 1 &&
                    child.FirstChild.GetType() == typeof(XmlText))
                {
                    Byte value;

                    if (Byte.TryParse(child.FirstChild.Value, out value))
                    {
                        return value - 1;
                    }
                }
            }

            return -1;
        }

        private void ReadMaxHR(XmlNode activityNode)
        {
            bool maxHRRead = false;

            for (int i = 0; i < activityNode.ChildNodes.Count; ++i)
            {
                XmlNode child = activityNode.ChildNodes[i];

                if (child.Name == Constants.MaxHRBPMTCXString &&
                    child.ChildNodes.Count == 1 &&
                    child.FirstChild.Name == Constants.ValueTCXString)
                {
                    m_MaxHeartRate.Deserialize(child.FirstChild);
                    maxHRRead = true;
                }
            }

            if (!maxHRRead)
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in activity profile XML node", activityNode);
            }
        }

        public virtual GarminActivityProfile Clone()
        {
            MemoryStream stream = new MemoryStream();
            GarminActivityProfile clone = new GarminActivityProfile(Category);

            Serialize(stream);
            stream.Position = 0;
            clone.Deserialize(stream, Constants.CurrentVersion);
            stream.Close();

            return clone;
        }

        protected void TriggerChangedEvent(PropertyChangedEventArgs args)
        {
            if(ActivityProfileChanged != null)
            {
                ActivityProfileChanged(this, args);
            }
        }

        public Byte GetHeartRateLowLimit(int index)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            double value = m_HeartRateZones[index].Lower;

            if (HRZonesReferential == HRReferential.HRReferential_BPM)
            {
                double lowLimit = Math.Max(Constants.MinHRInBPM, value * MaximumHeartRate);

                return (Byte)Math.Round(lowLimit, 0, MidpointRounding.AwayFromZero);
            }
            else if (HRZonesReferential == HRReferential.HRReferential_PercentHRR)
            {
                double reserveSize = MaximumHeartRate - GarminProfileManager.Instance.UserProfile.RestingHeartRate;
                double result = Math.Round((value * MaximumHeartRate - GarminProfileManager.Instance.UserProfile.RestingHeartRate) / reserveSize * 100.0, 0, MidpointRounding.AwayFromZero);

                return (Byte)Utils.Clamp(result, Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax);
            }
            else
            {
                return (Byte)Math.Round(value * 100, 0, MidpointRounding.AwayFromZero);
            }
        }

        public Byte GetHeartRateHighLimit(int index)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            double value = m_HeartRateZones[index].Upper;

            if (HRZonesReferential == HRReferential.HRReferential_BPM)
            {
                double highLimit = Math.Max(Constants.MinHRInBPM, value * MaximumHeartRate);

                return (Byte)Math.Round(highLimit, 0, MidpointRounding.AwayFromZero);
            }
            else if(HRZonesReferential == HRReferential.HRReferential_PercentHRR)
            {
                double reserveSize = MaximumHeartRate - GarminProfileManager.Instance.UserProfile.RestingHeartRate;
                double result = Math.Round((value * MaximumHeartRate - GarminProfileManager.Instance.UserProfile.RestingHeartRate) / reserveSize * 100.0, 0, MidpointRounding.AwayFromZero);

                return (Byte)Utils.Clamp(result, Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax);
            }
            else
            {
                return (Byte)Math.Round(value * 100, 0, MidpointRounding.AwayFromZero);
            }
        }

        public void SetHeartRateLowLimit(int index, Byte value)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            double percentValue;

            // Convert to % max if value is in BPM
            if (HRZonesReferential == HRReferential.HRReferential_BPM)
            {
                percentValue = value / (double)MaximumHeartRate;
            }
            // Convert to % max if value is in % HRR
            else if (HRZonesReferential == HRReferential.HRReferential_PercentHRR)
            {
                double reserveSize = MaximumHeartRate - GarminProfileManager.Instance.UserProfile.RestingHeartRate;

                percentValue = ((value / (double)Constants.MaxHRInPercentMax) * reserveSize +
                                GarminProfileManager.Instance.UserProfile.RestingHeartRate) / (double)MaximumHeartRate;
            }
            else
            {
                percentValue = value / (double)Constants.MaxHRInPercentMax;
            }

            if (m_HeartRateZones[index].Lower != percentValue)
            {
                m_HeartRateZones[index].Lower = new GarminFitnessDoubleRange(percentValue, 0, 1);

                TriggerChangedEvent(new PropertyChangedEventArgs("HeartRateZoneLimit"));
            }
        }

        public void SetHeartRateHighLimit(int index, Byte value)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            double percentValue;

            // Convert to % max if value is in BPM
            if (HRZonesReferential == HRReferential.HRReferential_BPM)
            {
                percentValue = value / (double)MaximumHeartRate;
            }
            // Convert to % max if value is in % HRR
            else if (HRZonesReferential == HRReferential.HRReferential_PercentHRR)
            {
                double reserveSize = MaximumHeartRate - GarminProfileManager.Instance.UserProfile.RestingHeartRate;

                percentValue = ((value / (double)Constants.MaxHRInPercentMax) * reserveSize +
                                GarminProfileManager.Instance.UserProfile.RestingHeartRate) / (double)MaximumHeartRate;
            }
            else
            {
                percentValue = value / (double)Constants.MaxHRInPercentMax;
            }

            if (m_HeartRateZones[index].Upper != percentValue)
            {
                m_HeartRateZones[index].Upper = new GarminFitnessDoubleRange(percentValue, 0, 1);

                TriggerChangedEvent(new PropertyChangedEventArgs("HeartRateZoneLimit"));
            }
        }

        public string GetSpeedZoneName(int index)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminSpeedZoneCount);

            return m_SpeedZones[index].Name;
        }

        public double GetSpeedLowLimit(int index)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminSpeedZoneCount);

            double speedValue = Length.Convert(m_SpeedZones[index].Low, Length.Units.Meter, BaseSpeedUnit) * Constants.SecondsPerHour;

            if (SpeedIsInPace)
            {
                return Utils.SpeedToPace(speedValue);
            }
            else
            {
                return speedValue;
            }
        }

        public double GetSpeedHighLimit(int index)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminSpeedZoneCount);

            double speedValue = Length.Convert(m_SpeedZones[index].High, Length.Units.Meter, BaseSpeedUnit) * Constants.SecondsPerHour;

            if (SpeedIsInPace)
            {
                return Utils.SpeedToPace(speedValue);
            }
            else
            {
                return speedValue;
            }
        }

        public void SetSpeedName(int index, string name)
        {
            if (m_SpeedZones[index].Name != name)
            {
                m_SpeedZones[index].Name = name;

                TriggerChangedEvent(new PropertyChangedEventArgs("SpeedZoneLimit"));
            }

        }

        public void SetSpeedLowLimitInMetersPerSecond(int index, double value)
        {
            if (m_SpeedZones[index].Low != value)
            {
                m_SpeedZones[index].Low = value;

                TriggerChangedEvent(new PropertyChangedEventArgs("SpeedZoneLimit"));

                if (value.CompareTo(m_SpeedZones[index].High) > 0)
                {
                    SetSpeedHighLimitInMetersPerSecond(index, value);
                }

                if (Options.Instance.ForceConsecutiveProfileSpeedZones &&
                    index > 0)
                {
                    SetSpeedHighLimitInMetersPerSecond(index - 1, value);
                }
            }
        }

        public void SetSpeedLowLimit(int index, double value)
        {
            double realValue = value;

            // Convert to speed if in pace
            if (SpeedIsInPace)
            {
                realValue = Utils.PaceToSpeed(value);
            }

            realValue = Length.Convert(realValue, BaseSpeedUnit, Length.Units.Meter) / Constants.SecondsPerHour;

            SetSpeedLowLimitInMetersPerSecond(index, realValue);
        }

        public void SetSpeedHighLimitInMetersPerSecond(int index, double value)
        {
            if (m_SpeedZones[index].High != value)
            {
                m_SpeedZones[index].High = value;

                TriggerChangedEvent(new PropertyChangedEventArgs("SpeedZoneLimit"));

                if (value.CompareTo(m_SpeedZones[index].Low) < 0)
                {
                    SetSpeedLowLimitInMetersPerSecond(index, value);
                }

                if (Options.Instance.ForceConsecutiveProfileSpeedZones &&
                    index < Constants.GarminSpeedZoneCount)
                {
                    SetSpeedLowLimitInMetersPerSecond(index + 1, value);
                }
            }
        }

        public void SetSpeedHighLimit(int index, double value)
        {
            double realValue = value;

            // Convert to speed if in pace
            if (SpeedIsInPace)
            {
                realValue = Utils.PaceToSpeed(realValue);
            }

            realValue = Length.Convert(realValue, BaseSpeedUnit, Length.Units.Meter) / Constants.SecondsPerHour;

            SetSpeedHighLimitInMetersPerSecond(index, realValue);
        }

        public void SetGearWeightInUnits(double weight, Weight.Units unit)
        {
            // Convert to pounds
            m_GearWeightInPounds.Value = Weight.Convert(weight, unit, Weight.Units.Pound);
        }

        public GarminCategories Category
        {
            get { return m_Category; }
        }

        public Byte MaximumHeartRate
        {
            get { return m_MaxHeartRate; }
            set
            {
                if (m_MaxHeartRate != value)
                {
                    if (HRZonesReferential == HRReferential.HRReferential_BPM)
                    {
                        // Update limit values as they will change since stored in %max
                        for (int i = 0; i < Constants.GarminHRZoneCount; ++i)
                        {
                            double currentLowValue = m_HeartRateZones[i].Lower;
                            double currentHighValue = m_HeartRateZones[i].Upper;

                            m_HeartRateZones[i].Lower = new GarminFitnessDoubleRange(Math.Min(1.0f, (currentLowValue * MaximumHeartRate) / value), 0, 1);
                            m_HeartRateZones[i].Upper = new GarminFitnessDoubleRange(Math.Min(1.0f, (currentHighValue * MaximumHeartRate) / value), 0, 1);
                        }
                    }

                    m_MaxHeartRate.Value = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("MaximumHeartRate"));
                }
            }
        }

        public double GearWeightInPounds
        {
            get { return m_GearWeightInPounds; }
            private set
            {
                if (m_GearWeightInPounds != value)
                {
                    m_GearWeightInPounds.Value = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("GearWeight"));
                }
            }
        }

        public HRReferential HRZonesReferential
        {
            get { return (HRReferential)(Byte)m_HRReferential; }
            set
            {
                if (m_HRReferential != (Byte)value)
                {
                    m_HRReferential.Value = (Byte)value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("HRZonesReferential"));
                }
            }
        }

        public bool SpeedIsInPace
        {
            get { return m_SpeedIsInPace; }
            set
            {
                if (m_SpeedIsInPace != value)
                {
                    m_SpeedIsInPace.Value = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("SpeedIsInPace"));
                }
            }
        }

        public Length.Units BaseSpeedUnit
        {
            get
            {
                if (Utils.IsStatute(PluginMain.GetApplication().SystemPreferences.DistanceUnits))
                {
                    return Length.Units.Mile;
                }
                else
                {
                    return Length.Units.Kilometer;
                }
            }
        }

        public delegate void ActivityProfileChangedEventHandler(GarminActivityProfile sender, PropertyChangedEventArgs changedProperty);
        public event ActivityProfileChangedEventHandler ActivityProfileChanged;

        private GarminCategories m_Category;
        private GarminFitnessByteRange m_MaxHeartRate = new GarminFitnessByteRange(185, Constants.MinHRInBPM, Constants.MaxHRInBPM);
        private GarminFitnessDoubleRange m_GearWeightInPounds = new GarminFitnessDoubleRange(0, Constants.MinWeight, Constants.MaxWeightInLbs);
        private GarminFitnessByteRange m_HRReferential = new GarminFitnessByteRange((Byte)HRReferential.HRReferential_BPM, 0, (Byte)HRReferential.HRReferential_Count);
        private GarminFitnessBool m_SpeedIsInPace = new GarminFitnessBool(false, Constants.SpeedOrPaceTCXString[0], Constants.SpeedOrPaceTCXString[1]);
        private List<GarminFitnessValueRange<GarminFitnessDoubleRange>> m_HeartRateZones = new List<GarminFitnessValueRange<GarminFitnessDoubleRange>>();
        private List<GarminFitnessNamedSpeedZone> m_SpeedZones = new List<GarminFitnessNamedSpeedZone>();
    }
}
