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

            m_HRIsInPercentMax.Serialize(stream);

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

        public void Deserialize_V8(Stream stream, DataVersion version)
        {
            m_MaxHeartRate.Deserialize(stream, version);

            GarminFitnessDoubleRange gearWeightinKg = new GarminFitnessDoubleRange(Weight.Convert(m_GearWeightInPounds, Weight.Units.Pound, Weight.Units.Kilogram));

            gearWeightinKg.Deserialize(stream, version);
            SetGearWeightInUnits(gearWeightinKg, Weight.Units.Kilogram);

            m_HRIsInPercentMax.Deserialize(stream, version);

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

            m_HRIsInPercentMax.Deserialize(stream, version);

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

            m_HRIsInPercentMax.Deserialize(stream, version);

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
                m_HRIsInPercentMax.Serialize(currentChild, Constants.ViewAsTCXString, document);

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
            bool inPercentMax = HRIsInPercentMax;

            HRIsInPercentMax = false;
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

            HRIsInPercentMax = inPercentMax;
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
                    m_HRIsInPercentMax.Deserialize(currentChild);
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

            if (HRIsInPercentMax)
            {
                return (Byte)Math.Round(value * 100, 0, MidpointRounding.AwayFromZero);
            }
            else
            {
                double lowLimit = Math.Max(Constants.MinHRInBPM, value * MaximumHeartRate);

                return (Byte)Math.Round(lowLimit, 0, MidpointRounding.AwayFromZero);
            }
        }

        public Byte GetHeartRateHighLimit(int index)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            double value = m_HeartRateZones[index].Upper;

            if (HRIsInPercentMax)
            {
                return (Byte)Math.Round(value * 100, 0, MidpointRounding.AwayFromZero);
            }
            else
            {
                double highLimit = Math.Max(Constants.MinHRInBPM, value * MaximumHeartRate);

                return (Byte)Math.Round(highLimit, 0, MidpointRounding.AwayFromZero);
            }
        }

        public void SetHeartRateLowLimit(int index, Byte value)
        {
            Debug.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            double percentValue;

            // Convert to BPM if value is in % HRMax
            if (HRIsInPercentMax)
            {
                percentValue = value / (double)Constants.MaxHRInPercentMax;
            }
            else
            {
                percentValue = value / (double)MaximumHeartRate;
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

            // Convert to BPM if value is in % HRMax
            if (HRIsInPercentMax)
            {
                percentValue = value / (double)Constants.MaxHRInPercentMax;
            }
            else
            {
                percentValue = value / (double)MaximumHeartRate;
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

        public void SetSpeedLowLimit(int index, double value)
        {
            double realValue = value;

            // Convert to speed if in pace
            if (SpeedIsInPace)
            {
                realValue = Utils.PaceToSpeed(value);
            }

            realValue = Length.Convert(realValue, BaseSpeedUnit, Length.Units.Meter) / Constants.SecondsPerHour;

            if (m_SpeedZones[index].Low != realValue)
            {
                m_SpeedZones[index].Low = realValue;

                TriggerChangedEvent(new PropertyChangedEventArgs("SpeedZoneLimit"));
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

            if (m_SpeedZones[index].High != realValue)
            {
                m_SpeedZones[index].High = realValue;

                TriggerChangedEvent(new PropertyChangedEventArgs("SpeedZoneLimit"));
            }
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
                    if(!HRIsInPercentMax)
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

        public bool HRIsInPercentMax
        {
            get { return m_HRIsInPercentMax; }
            set
            {
                if (m_HRIsInPercentMax != value)
                {
                    m_HRIsInPercentMax.Value = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("HRIsInPercentMax"));
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
        private GarminFitnessDoubleRange m_GearWeightInPounds = new GarminFitnessDoubleRange(0, Constants.MinWeight, Constants.MaxWeight);
        private GarminFitnessBool m_HRIsInPercentMax = new GarminFitnessBool(false, Constants.PercentMaxTCXString, Constants.BPMTCXString);
        private GarminFitnessBool m_SpeedIsInPace = new GarminFitnessBool(false, Constants.SpeedOrPaceTCXString[0], Constants.SpeedOrPaceTCXString[1]);
        private List<GarminFitnessValueRange<GarminFitnessDoubleRange>> m_HeartRateZones = new List<GarminFitnessValueRange<GarminFitnessDoubleRange>>();
        private List<GarminFitnessNamedSpeedZone> m_SpeedZones = new List<GarminFitnessNamedSpeedZone>();
    }
}
