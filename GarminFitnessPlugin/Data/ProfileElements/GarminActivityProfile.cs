using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

            m_MaxHeartRate = 185;
            m_RestingHeartRate = 60;
            m_GearWeight = 0;
            m_HRIsInPercentMax = true;
            m_SpeedIsInPace = false;

            // HR Zones (always stored in % Max)
            float currentLowHR = 0.5f;
            const float stepHR = 0.1f;
            for(int i = 0; i < Constants.GarminHRZoneCount; ++i)
            {
                m_HeartRateZones.Add(new GarminFitnessValueRange<float>(currentLowHR, currentLowHR + stepHR));

                currentLowHR += stepHR;
            }

            // Speed Zones
            float currentLowSpeed = 10;
            const float stepSpeed = 5;
            for(int i = 0; i < Constants.GarminSpeedZoneCount; ++i)
            {
                String zoneName = GarminFitnessView.ResourceManager.GetString("GTCSpeedZone" + (i + 1).ToString() + "Text", GarminFitnessView.UICulture);

                m_SpeedZones.Add(new GarminFitnessNamedLowHighZone(currentLowSpeed, currentLowSpeed + stepSpeed, zoneName));

                currentLowSpeed += stepSpeed;
            }
        }

        public override void Serialize(Stream stream)
        {
            // Max HR
            stream.WriteByte(MaximumHeartRate);
            
            // Gear weight in pounds
            stream.Write(BitConverter.GetBytes(GearWeight), 0, sizeof(double));

            // HR as max?
            stream.Write(BitConverter.GetBytes(HRIsInPercentMax), 0, sizeof(bool));
            // HR zones
            for (int i = 0; i < m_HeartRateZones.Count; ++i)
            {
                // Low bound
                stream.Write(BitConverter.GetBytes((double)m_HeartRateZones[i].Lower), 0, sizeof(double));

                // High bound
                stream.Write(BitConverter.GetBytes((double)m_HeartRateZones[i].Upper), 0, sizeof(double));
            }

            // Speed as pace?
            stream.Write(BitConverter.GetBytes(SpeedIsInPace), 0, sizeof(bool));
            // Speed Zones
            for (int i = 0; i < m_SpeedZones.Count; ++i)
            {
                // Low bound
                stream.Write(BitConverter.GetBytes((double)m_SpeedZones[i].Low), 0, sizeof(double));

                // High bound
                stream.Write(BitConverter.GetBytes((double)m_SpeedZones[i].High), 0, sizeof(double));

                // Name
                stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(m_SpeedZones[i].Name)), 0, sizeof(Int32));
                stream.Write(Encoding.UTF8.GetBytes(m_SpeedZones[i].Name), 0, Encoding.UTF8.GetByteCount(m_SpeedZones[i].Name));
            }
        }

        public override void Deserialize_V0(Stream stream, DataVersion version)
        {
        }

        public void Deserialize_V8(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] boolBuffer = new byte[sizeof(bool)];
            byte[] doubleBuffer = new byte[sizeof(double)];
            byte[] stringBuffer;
            Int32 stringLength;

            // Max HR
            MaximumHeartRate = (Byte)stream.ReadByte();

            // Gear weight
            stream.Read(doubleBuffer, 0, sizeof(double));
            GearWeight = BitConverter.ToDouble(doubleBuffer, 0);

            // HR as % max
            stream.Read(boolBuffer, 0, sizeof(bool));
            HRIsInPercentMax = BitConverter.ToBoolean(boolBuffer, 0);

            for (int i = 0; i < m_HeartRateZones.Count; ++i)
            {
                // Lower limit
                stream.Read(doubleBuffer, 0, sizeof(double));
                m_HeartRateZones[i].Lower = (float)BitConverter.ToDouble(doubleBuffer, 0);

                // Upper limit
                stream.Read(doubleBuffer, 0, sizeof(double));
                m_HeartRateZones[i].Upper = (float)BitConverter.ToDouble(doubleBuffer, 0);
            }

            // Speed as pace
            stream.Read(boolBuffer, 0, sizeof(bool));
            SpeedIsInPace = BitConverter.ToBoolean(boolBuffer, 0);

            for (int i = 0; i < m_SpeedZones.Count; ++i)
            {
                // Lower limit
                stream.Read(doubleBuffer, 0, sizeof(double));
                m_SpeedZones[i].Low = (float)BitConverter.ToDouble(doubleBuffer, 0);

                // Upper limit
                stream.Read(doubleBuffer, 0, sizeof(double));
                m_SpeedZones[i].High = (float)BitConverter.ToDouble(doubleBuffer, 0);

                // Speed zone name
                stream.Read(intBuffer, 0, sizeof(Int32));
                stringLength = BitConverter.ToInt32(intBuffer, 0);
                stringBuffer = new byte[stringLength];
                stream.Read(stringBuffer, 0, stringLength);
                m_SpeedZones[i].Name = Encoding.UTF8.GetString(stringBuffer);
            }
        }

        public virtual void Serialize(XmlNode parentNode, XmlDocument document)
        {
        }

        public virtual bool Deserialize(XmlNode parentNode)
        {
            bool weightRead = false;
            int HRZonesRead = 0;
            int speedZonesRead = 0;
            double weightInPounds = 0;

            // Put default value, use current to avoid overwrite if rest HR not present
            RestingHeartRate = GarminProfileManager.Instance.RestingHeartRate;

            Byte maxHR;
            if (!PeekMaxHR(parentNode, out maxHR))
            {
                return false;
            }
            MaximumHeartRate = maxHR;

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == Constants.RestHRBPMTCXString &&
                    currentChild.ChildNodes.Count == 1 &&
                    currentChild.FirstChild.Name == Constants.ValueTCXString)
                {
                    XmlNode valueNode = currentChild.FirstChild;

                    if (valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                    {
                        if (!Utils.IsTextIntegerInRange(valueNode.FirstChild.Value, Constants.MinHRInBPM, Constants.MaxHRInBPM))
                        {
                            return false;
                        }

                        RestingHeartRate = Byte.Parse(valueNode.FirstChild.Value);
                    }
                }
                else if (currentChild.Name == Constants.GearWeightTCXString &&
                         currentChild.ChildNodes.Count == 1 &&
                         currentChild.FirstChild.GetType() == typeof(XmlText))
                {
                    double weight;

                    if (!Utils.IsTextFloatInRange(currentChild.FirstChild.Value, Constants.MinWeight, Constants.MaxWeight))
                    {
                        return false;
                    }

                    weight = double.Parse(currentChild.FirstChild.Value);
                    weightInPounds = Weight.Convert(weight, Weight.Units.Kilogram, Weight.Units.Pound);
                    weightRead = true;
                }
                else if (currentChild.Name == Constants.HeartRateZonesTCXString)
                {
                    int zoneIndex = PeekZoneNumber(currentChild);

                    if (zoneIndex != -1)
                    {
                        if (ReadHRZone(zoneIndex, currentChild))
                        {
                            HRZonesRead++;
                        }
                    }
                }
                else if (currentChild.Name == Constants.SpeedZonesTCXString)
                {
                    int zoneIndex = PeekZoneNumber(currentChild);

                    if (zoneIndex != -1)
                    {
                        if (ReadSpeedZone(zoneIndex, currentChild))
                        {
                            speedZonesRead++;
                        }
                    }
                }
            }

            // Check if all was read successfully
            if (!weightRead ||
                HRZonesRead != Constants.GarminHRZoneCount ||
                speedZonesRead != Constants.GarminSpeedZoneCount)
            {
                return false;
            }

            // Officialize
            GearWeight = weightInPounds;

            // Convert speed zones to the right unit (m/sec to km/h)
            for (int i = 0; i < Constants.GarminSpeedZoneCount; ++i)
            {
                float tempHigh = m_SpeedZones[i].High;

                m_SpeedZones[i].Low = (m_SpeedZones[i].Low * Constants.SecondsPerHour) / 1000.0f;
                m_SpeedZones[i].High = (tempHigh * Constants.SecondsPerHour) / 1000.0f;
            }

            return true;
        }

        private bool ReadHRZone(int index, XmlNode parentNode)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            bool viewAsRead = false;
            bool lowRead = false;
            bool highRead = false;
            bool viewAsPercentMax = false;
            Byte lowLimit = 0;
            Byte highLimit = 0;

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == Constants.ViewAsTCXString &&
                    currentChild.ChildNodes.Count == 1 &&
                    currentChild.FirstChild.GetType() == typeof(XmlText))
                {
                    viewAsPercentMax = currentChild.FirstChild.Value == Constants.PercentMaxTCXString;
                    viewAsRead = true;
                }
                else if (currentChild.Name == Constants.LowTCXString &&
                         currentChild.ChildNodes.Count == 1 &&
                         currentChild.FirstChild.Name == Constants.ValueTCXString)
                {
                    XmlNode valueNode = currentChild.FirstChild;

                    if (valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                    {
                        lowLimit = Byte.Parse(valueNode.FirstChild.Value);
                        lowRead = true;
                    }
                }
                else if (currentChild.Name == Constants.HighTCXString &&
                         currentChild.ChildNodes.Count == 1 &&
                         currentChild.FirstChild.Name == Constants.ValueTCXString)
                {
                    XmlNode valueNode = currentChild.FirstChild;

                    if (valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                    {
                        highLimit = Byte.Parse(valueNode.FirstChild.Value);
                        highRead = true;
                    }
                }
            }

            // Check if all was read successfully
            if (!viewAsRead || !lowRead || !highRead)
            {
                return false;
            }

            HRIsInPercentMax = viewAsPercentMax;
            m_HeartRateZones[index].Lower = (float)Math.Min(lowLimit, highLimit) / MaximumHeartRate;
            m_HeartRateZones[index].Upper = (float)Math.Max(lowLimit, highLimit) / MaximumHeartRate;

            return true;
        }

        private bool ReadSpeedZone(int index, XmlNode parentNode)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminSpeedZoneCount);

            bool nameRead = false;
            bool viewAsRead = false;
            bool lowRead = false;
            bool highRead = false;
            bool viewAsPace = false;
            double lowLimit = 0;
            double highLimit = 0;
            string name = String.Empty;

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == "Name" &&
                    currentChild.ChildNodes.Count == 1 &&
                    currentChild.FirstChild.GetType() == typeof(XmlText))
                {
                    name = currentChild.FirstChild.Value;
                    nameRead = true;
                }
                else if(currentChild.Name == Constants.ValueTCXString)
                {
                    XmlNode valueNode = currentChild;

                    for (int j = 0; j < valueNode.ChildNodes.Count; ++j)
                    {
                        XmlNode valueChild = valueNode.ChildNodes[j];

                        if (valueChild.Name == Constants.ViewAsTCXString &&
                            valueChild.ChildNodes.Count == 1 &&
                            valueChild.FirstChild.GetType() == typeof(XmlText))
                        {
                            viewAsPace = valueChild.FirstChild.Value == Constants.SpeedOrPaceTCXString[0];
                            viewAsRead = true;
                        }
                        else if (valueChild.Name == Constants.LowInMeterPerSecTCXString &&
                                 valueChild.ChildNodes.Count == 1 &&
                                 valueChild.FirstChild.GetType() == typeof(XmlText))
                        {
                            lowLimit = double.Parse(valueChild.FirstChild.Value);
                            lowRead = true;
                        }
                        else if (valueChild.Name == Constants.HighInMeterPerSecTCXString &&
                                 valueChild.ChildNodes.Count == 1 &&
                                 valueChild.FirstChild.GetType() == typeof(XmlText))
                        {
                            highLimit = double.Parse(valueChild.FirstChild.Value);
                            highRead = true;
                        }
                    }
                }
            }

            // Check if all was read successfully
            if (!nameRead ||!viewAsRead || !lowRead || !highRead)
            {
                return false;
            }

            SpeedIsInPace = viewAsPace;
            m_SpeedZones[index].Name = name;
            m_SpeedZones[index].Low = (float)Math.Min(lowLimit, highLimit);
            m_SpeedZones[index].High = (float)Math.Max(lowLimit, highLimit);

            return true;
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

        private bool PeekMaxHR(XmlNode zoneNode, out Byte maxHR)
        {
            maxHR = 0;

            for (int i = 0; i < zoneNode.ChildNodes.Count; ++i)
            {
                XmlNode child = zoneNode.ChildNodes[i];

                if (child.Name == Constants.MaxHRBPMTCXString &&
                    child.ChildNodes.Count == 1 &&
                    child.FirstChild.Name == Constants.ValueTCXString)
                {
                    XmlNode valueNode = child.FirstChild;

                    if (valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                    {
                        if (!Utils.IsTextIntegerInRange(valueNode.FirstChild.Value, Constants.MinHRInBPM, Constants.MaxHRInBPM))
                        {
                            return false;
                        }

                        maxHR = Byte.Parse(valueNode.FirstChild.Value);

                        return true;
                    }
                }
            }

            return false;
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
            Trace.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            float value = m_HeartRateZones[index].Lower;

            if (HRIsInPercentMax)
            {
                return (Byte)Math.Round(value * 100, 0, MidpointRounding.AwayFromZero);
            }
            else
            {
                return (Byte)Math.Round(value * MaximumHeartRate, 0, MidpointRounding.AwayFromZero);
            }
        }

        public Byte GetHeartRateHighLimit(int index)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            float value = m_HeartRateZones[index].Upper;

            if (HRIsInPercentMax)
            {
                return (Byte)Math.Round(value * 100, 0, MidpointRounding.AwayFromZero);
            }
            else
            {
                return (Byte)Math.Round(value * MaximumHeartRate, 0, MidpointRounding.AwayFromZero);
            }
        }

        public void SetHeartRateLowLimit(int index, Byte value)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            float percentValue;

            // Convert to BPM if value is in % HRMax
            if (HRIsInPercentMax)
            {
                percentValue = value / (float)Constants.MaxHRInPercentMax;
            }
            else
            {
                percentValue = value / (float)MaximumHeartRate;
            }

            if (m_HeartRateZones[index].Lower != percentValue)
            {
                m_HeartRateZones[index].Lower = percentValue;

                TriggerChangedEvent(new PropertyChangedEventArgs("HeartRateZoneLimit"));
            }
        }

        public void SetHeartRateHighLimit(int index, Byte value)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            float percentValue;

            // Convert to BPM if value is in % HRMax
            if (HRIsInPercentMax)
            {
                percentValue = value / (float)Constants.MaxHRInPercentMax;
            }
            else
            {
                percentValue = value / (float)MaximumHeartRate;
            }

            if (m_HeartRateZones[index].Upper != percentValue)
            {
                m_HeartRateZones[index].Upper = percentValue;

                TriggerChangedEvent(new PropertyChangedEventArgs("HeartRateZoneLimit"));
            }
        }

        public string GetSpeedZoneName(int index)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminSpeedZoneCount);

            return m_SpeedZones[index].Name;
        }

        public double GetSpeedLowLimit(int index)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminSpeedZoneCount);

            double speedValue = Length.Convert(m_SpeedZones[index].Low, Length.Units.Kilometer, BaseSpeedUnit);

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
            Trace.Assert(index >= 0 && index < Constants.GarminSpeedZoneCount);

            double speedValue = Length.Convert(m_SpeedZones[index].High, Length.Units.Kilometer, BaseSpeedUnit);

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
            double realValue = Length.Convert(value, BaseSpeedUnit, Length.Units.Kilometer);

            // Convert to BPM if value is in % HRMax
            if (SpeedIsInPace)
            {
                realValue = Utils.PaceToSpeed(value);
            }

            if (m_SpeedZones[index].Low != realValue)
            {
                m_SpeedZones[index].Low = (float)realValue;

                TriggerChangedEvent(new PropertyChangedEventArgs("SpeedZoneLimit"));
            }
        }

        public void SetSpeedHighLimit(int index, double value)
        {
            double realValue = Length.Convert(value, BaseSpeedUnit, Length.Units.Kilometer);

            // Convert to BPM if value is in % HRMax
            if (SpeedIsInPace)
            {
                realValue = Utils.PaceToSpeed(realValue);
            }

            if (m_SpeedZones[index].High != realValue)
            {
                m_SpeedZones[index].High = (float)realValue;

                TriggerChangedEvent(new PropertyChangedEventArgs("SpeedZoneLimit"));
            }
        }

        public void SetGearWeightInUnits(double weight, Weight.Units unit)
        {
            // Convert to pounds
            GearWeight = Weight.Convert(weight, unit, Weight.Units.Pound);
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
                            float currentLowValue = m_HeartRateZones[i].Lower;
                            float currentHighValue = m_HeartRateZones[i].Upper;

                            m_HeartRateZones[i].Lower = Math.Min(1.0f, (currentLowValue * MaximumHeartRate) / value);
                            m_HeartRateZones[i].Upper = Math.Min(1.0f, (currentHighValue * MaximumHeartRate) / value);
                        }
                    }

                    m_MaxHeartRate = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("MaximumHeartRate"));
                }
            }
        }

        public Byte RestingHeartRate
        {
            get { return m_RestingHeartRate; }
            set
            {
                if (m_RestingHeartRate != value)
                {
                    m_RestingHeartRate = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("RestingHeartRate"));
                }
            }
        }

        public double GearWeight
        {
            get { return m_GearWeight; }
            private set
            {
                if (m_GearWeight != value)
                {
                    m_GearWeight = value;

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
                    m_HRIsInPercentMax = value;

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
                    m_SpeedIsInPace = value;

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
        private Byte m_MaxHeartRate;
        private Byte m_RestingHeartRate;
        private double m_GearWeight;
        private bool m_HRIsInPercentMax;
        private bool m_SpeedIsInPace;
        private List<GarminFitnessValueRange<float>> m_HeartRateZones = new List<GarminFitnessValueRange<float>>();
        private List<GarminFitnessNamedLowHighZone> m_SpeedZones = new List<GarminFitnessNamedLowHighZone>();
    }
}
