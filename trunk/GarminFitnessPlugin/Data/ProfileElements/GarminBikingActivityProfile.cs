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
        class GarminBikeProfile
        {
            public GarminBikeProfile()
            {
                SetData(true, false, "Bike", 0, 0, true, 2100);
            }

            public GarminBikeProfile(string name)
            {
                SetData(true, false, name, 0, 0, true, 2100);
            }

            private void TriggerBikeProfileChangedEvent(string propertyName)
            {
                if (BikeProfileChanged != null)
                {
                    BikeProfileChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            public void SetData(bool hasCadenceSensor, bool hasPowerSensor, string name,
                                double odometer, double weightinPounds, bool autoWheelSize,
                                UInt16 wheelSize)
            {
                HasCadenceSensor = hasCadenceSensor;
                HasPowerSensor = hasPowerSensor;
                Name = name;
                OdometerInMeters = odometer;
                WeightInPounds = weightinPounds;
                AutoWheelSize = autoWheelSize;
                WheelSize = wheelSize;
            }

            public bool HasCadenceSensor
            {
                get { return m_HasCadenceSensor; }
                set
                {
                    if (m_HasCadenceSensor != value)
                    {
                        m_HasCadenceSensor = value;

                        TriggerBikeProfileChangedEvent("HasCadenceSensor");
                    }
                }
            }

            public bool HasPowerSensor
            {
                get { return m_HasPowerSensor; }
                set
                {
                    if (m_HasPowerSensor != value)
                    {
                        m_HasPowerSensor = value;

                        TriggerBikeProfileChangedEvent("HasPowerSensor");
                    }
                }
            }

            public bool AutoWheelSize
            {
                get { return m_AutoWheelSize; }
                set
                {
                    if (m_AutoWheelSize != value)
                    {
                        m_AutoWheelSize = value;

                        TriggerBikeProfileChangedEvent("AutoWheelSize");
                    }
                }
            }

            public string Name
            {
                get { return m_Name; }
                set
                {
                    if (m_Name != value)
                    {
                        m_Name = value;

                        TriggerBikeProfileChangedEvent("Name");
                    }
                }
            }

            public double OdometerInMeters
            {
                get { return m_OdometerInMeters; }
                set
                {
                    if (m_OdometerInMeters != value)
                    {
                        m_OdometerInMeters = value;

                        TriggerBikeProfileChangedEvent("OdometerInMeters");
                    }
                }
            }

            public double WeightInPounds
            {
                get { return m_WeightInPounds; }
                set
                {
                    if (m_WeightInPounds != value)
                    {
                        m_WeightInPounds = value;

                        TriggerBikeProfileChangedEvent("WeightInPounds");
                    }
                }
            }

            public UInt16 WheelSize
            {
                get { return m_WheelSize; }
                set
                {
                    if (m_WheelSize != value)
                    {
                        m_WheelSize = value;

                        TriggerBikeProfileChangedEvent("WheelSize");
                    }
                }
            }

            public delegate void BikeProfileChangedEventHandler(GarminBikeProfile sender, PropertyChangedEventArgs changedProperty);
            public event BikeProfileChangedEventHandler BikeProfileChanged;

            private bool m_HasCadenceSensor = false;
            private bool m_HasPowerSensor = false;
            private bool m_AutoWheelSize = true;
            private string m_Name = "";
            private double m_OdometerInMeters = 0;
            private double m_WeightInPounds = 0;
            private UInt16 m_WheelSize = 2100;
        }

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

            // Bike profiles
            m_Bikes[0] = new GarminBikeProfile("Road");
            m_Bikes[1] = new GarminBikeProfile("MTB");
            m_Bikes[2] = new GarminBikeProfile("Spinner");

            for (int i = 0; i < Constants.GarminBikeProfileCount; ++i)
            {
                m_Bikes[i].BikeProfileChanged += new GarminBikeProfile.BikeProfileChangedEventHandler(OnBikeProfileChanged);
            }
        }

        void OnBikeProfileChanged(GarminBikingActivityProfile.GarminBikeProfile sender, PropertyChangedEventArgs changedProperty)
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
                    if(ReadBikeProfile(bikeProfilesRead, currentChild))
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

        private bool ReadBikeProfile(int index, XmlNode parentNode)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminBikeProfileCount);

            bool cadenceSensorRead = false;
            bool powerSensorRead = false;
            bool nameRead = false;
            bool odometerRead = false;
            bool weightRead = false;
            bool autoWheelSizeRead = false;
            bool wheelSizeRead = false;
            bool hasCadenceSensor = false;
            bool hasPowerSensor = false;
            string name = "";
            double odometer = 0;
            double weight = 0;
            bool autoWheelSize = true;
            UInt16 wheelSize = 2100;

            for (int i = 0; i < parentNode.Attributes.Count; ++i)
            {
                XmlAttribute currentAtttribute = parentNode.Attributes[i];

                if (currentAtttribute.Name == Constants.HasCadenceTCXString)
                {
                    hasCadenceSensor = currentAtttribute.Value == bool.TrueString.ToLower();
                    cadenceSensorRead = true;
                }
                else if (currentAtttribute.Name == Constants.HasPowerTCXString)
                {
                    hasPowerSensor = currentAtttribute.Value == bool.TrueString.ToLower();
                    powerSensorRead = true;
                }
            }

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
                else if (currentChild.Name == Constants.OdometerTCXString &&
                         currentChild.ChildNodes.Count == 1 &&
                         currentChild.FirstChild.GetType() == typeof(XmlText))
                {
                    odometerRead = double.TryParse(currentChild.FirstChild.Value, out odometer);
                }
                else if (currentChild.Name == Constants.WeightTCXString &&
                         currentChild.ChildNodes.Count == 1 &&
                         currentChild.FirstChild.GetType() == typeof(XmlText))
                {
                    if (!Utils.IsTextFloatInRange(currentChild.FirstChild.Value, Constants.MinWeight, Constants.MaxWeight))
                    {
                        return false;
                    }

                    weightRead = double.TryParse(currentChild.FirstChild.Value, out weight);
                    weight = Weight.Convert(weight, Weight.Units.Kilogram, Weight.Units.Pound);
                }
                else if (currentChild.Name == Constants.WheelSizeTCXString &&
                         currentChild.ChildNodes.Count == 1)
                {
                    if (currentChild.Attributes.Count == 1 &&
                        currentChild.Attributes[0].Name == Constants.AutoWheelSizeTCXString &&
                        currentChild.ChildNodes.Count == 1 &&
                        currentChild.FirstChild.Name == Constants.SizeMillimetersTCXString &&
                        currentChild.FirstChild.ChildNodes.Count == 1 &&
                        currentChild.FirstChild.FirstChild.GetType() == typeof(XmlText))
                    {
                        if(!Utils.IsTextInteger(currentChild.FirstChild.FirstChild.Value))
                        {
                            return false;
                        }

                        autoWheelSize = currentChild.Attributes[0].Value == bool.TrueString.ToLower();
                        autoWheelSizeRead = true;

                        wheelSizeRead = UInt16.TryParse(currentChild.FirstChild.FirstChild.Value, out wheelSize);
                    }
                }
            }

            // Check if all was read successfully
            if (!cadenceSensorRead || !powerSensorRead || !nameRead || !odometerRead ||
                !weightRead || !autoWheelSizeRead || !wheelSizeRead)
            {
                return false;
            }

            // Officialize
            m_Bikes[index].SetData(hasCadenceSensor, hasPowerSensor, name,
                                   odometer, weight, autoWheelSize, wheelSize);

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

        private List<GarminFitnessValueRange<UInt16>> m_PowerZones = new List<GarminFitnessValueRange<UInt16>>();
        private GarminBikeProfile[] m_Bikes = new GarminBikeProfile[3];
    }
}
