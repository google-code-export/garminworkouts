using System;
using System.IO;
using System.Xml;
using System.ComponentModel;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class GarminBikeProfile : IPluginSerializable, IXMLSerializable
    {
        public GarminBikeProfile()
        {
            SetData(true, false, "Bike", 0, 0, true, 2100);
        }

        public GarminBikeProfile(string name)
        {
            SetData(true, false, name, 0, 0, true, 2100);
        }

        public override void Serialize(Stream stream)
        {
        }

        public void Deserialize_V8(Stream stream, DataVersion version)
        {
        }

        public void Serialize(XmlNode parentNode, XmlDocument document)
        {
        }

        public bool Deserialize(XmlNode parentNode)
        {
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

                if (currentChild.Name == "Name")
                {
                    if (currentChild.ChildNodes.Count == 1 &&
                    currentChild.FirstChild.GetType() == typeof(XmlText))
                    {
                        name = currentChild.FirstChild.Value;
                    }
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
                        if (!Utils.IsTextInteger(currentChild.FirstChild.FirstChild.Value))
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
            SetData(hasCadenceSensor, hasPowerSensor, name,
                    odometer, weight, autoWheelSize, wheelSize);
            
            return true;
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
}
