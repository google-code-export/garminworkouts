using System;
using System.Globalization;
using System.IO;
using System.Text;
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
        }

        public GarminBikeProfile(string name)
        {
            Name = name;
        }

        public override void Serialize(Stream stream)
        {
            m_HasCadenceSensor.Serialize(stream);

            m_HasPowerSensor.Serialize(stream);

            m_AutoWheelSize.Serialize(stream);

            m_Name.Serialize(stream);

            m_OdometerInMeters.Serialize(stream);

            m_WeightInPounds.Serialize(stream);

            m_WheelSize.Serialize(stream);
        }

        public void Deserialize_V8(Stream stream, DataVersion version)
        {
            byte[] uintBuffer = new byte[sizeof(UInt16)];
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] doubleBuffer = new byte[sizeof(double)];

            m_HasCadenceSensor.Deserialize(stream, version);

            m_HasPowerSensor.Deserialize(stream, version);

            m_AutoWheelSize.Deserialize(stream, version);

            m_Name.Deserialize(stream, version);

            m_OdometerInMeters.Deserialize(stream, version);

            m_WeightInPounds.Deserialize(stream, version);

            m_WheelSize.Deserialize(stream, version);
        }

        public void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            XmlNode bikeNode, wheelInfoChild;

            bikeNode = document.CreateElement(nodeName);
            parentNode.AppendChild(bikeNode);

            m_HasCadenceSensor.SerializeAttribute(bikeNode, Constants.HasCadenceTCXString, document);

            m_HasPowerSensor.SerializeAttribute(bikeNode, Constants.HasPowerTCXString, document);

            m_Name.Serialize(bikeNode, "Name", document);

            m_OdometerInMeters.Serialize(bikeNode, Constants.OdometerTCXString, document);

            m_WeightInPounds.Serialize(bikeNode, Constants.WeightTCXString, document);

            // Wheel info node
            wheelInfoChild = document.CreateElement(Constants.WheelSizeTCXString);
            bikeNode.AppendChild(wheelInfoChild);

            m_AutoWheelSize.SerializeAttribute(wheelInfoChild, Constants.AutoWheelSizeTCXString, document);

            m_WheelSize.Serialize(wheelInfoChild, Constants.SizeMillimetersTCXString, document);
        }

        public void Deserialize(XmlNode parentNode)
        {
            bool cadenceSensorRead = false;
            bool powerSensorRead = false;
            bool nameRead = false;
            bool odometerRead = false;
            bool weightRead = false;
            bool wheelSizeRead = false;

            for (int i = 0; i < parentNode.Attributes.Count; ++i)
            {
                XmlAttribute currentAtttribute = parentNode.Attributes[i];

                if (currentAtttribute.Name == Constants.HasCadenceTCXString)
                {
                    HasCadenceSensor = m_HasCadenceSensor.GetTextValue(currentAtttribute.Value);
                    cadenceSensorRead = true;
                }
                else if (currentAtttribute.Name == Constants.HasPowerTCXString)
                {
                    HasPowerSensor = m_HasPowerSensor.GetTextValue(currentAtttribute.Value);
                    powerSensorRead = true;
                }
            }

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == "Name")
                {
                    m_Name.Deserialize(currentChild);
                    nameRead = true;
                }
                else if (currentChild.Name == Constants.OdometerTCXString)
                {
                    m_OdometerInMeters.Deserialize(currentChild);
                    odometerRead = true;
                }
                else if (currentChild.Name == Constants.WeightTCXString)
                {
                    m_WeightInPounds.Deserialize(currentChild);
                    WeightInPounds = Weight.Convert(WeightInPounds, Weight.Units.Kilogram, Weight.Units.Pound);
                    weightRead = true;
                }
                else if (currentChild.Name == Constants.WheelSizeTCXString &&
                         currentChild.ChildNodes.Count == 1)
                {
                    if (currentChild.Attributes.Count != 1 ||
                        currentChild.Attributes[0].Name != Constants.AutoWheelSizeTCXString ||
                        currentChild.ChildNodes.Count != 1 ||
                        currentChild.FirstChild.Name != Constants.SizeMillimetersTCXString)
                    {
                        throw new GarminFitnesXmlDeserializationException("Invalid bike wheel size XML node", parentNode);
                    }

                    AutoWheelSize = m_AutoWheelSize.GetTextValue(currentChild.Attributes[0].Value);
                    m_WheelSize.Deserialize(currentChild.FirstChild);
                    wheelSizeRead = true;
                }
            }

            // Check if all was read successfully
            if (!cadenceSensorRead || !powerSensorRead || !nameRead ||
                !odometerRead || !weightRead || !wheelSizeRead)
            {
                throw new GarminFitnesXmlDeserializationException("Missing information in bike profile XML node", parentNode);
            }
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
                    m_HasCadenceSensor.Value = value;

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
                    m_HasPowerSensor.Value = value;

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
                    m_AutoWheelSize.Value = value;

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
                    m_Name.Value = value;

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
                    m_OdometerInMeters.Value = value;

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
                    m_WeightInPounds.Value = value;

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
                    m_WheelSize.Value = value;

                    TriggerBikeProfileChangedEvent("WheelSize");
                }
            }
        }

        public delegate void BikeProfileChangedEventHandler(GarminBikeProfile sender, PropertyChangedEventArgs changedProperty);
        public event BikeProfileChangedEventHandler BikeProfileChanged;

        private GarminFitnessBool m_HasCadenceSensor = new GarminFitnessBool(false, true.ToString().ToLower(), false.ToString().ToLower());
        private GarminFitnessBool m_HasPowerSensor = new GarminFitnessBool(false, true.ToString().ToLower(), false.ToString().ToLower());
        private GarminFitnessBool m_AutoWheelSize = new GarminFitnessBool(true, true.ToString().ToLower(), false.ToString().ToLower());
        private GarminFitnessString m_Name = new GarminFitnessString("Bike", 15);
        private GarminFitnessDoubleRange m_OdometerInMeters = new GarminFitnessDoubleRange(0, Constants.MinOdometer, Constants.MaxOdometerMeters);
        private GarminFitnessDoubleRange m_WeightInPounds = new GarminFitnessDoubleRange(0, Constants.MinWeight, Constants.MaxWeight);
        private GarminFitnessUInt16Range m_WheelSize = new GarminFitnessUInt16Range(2100, Constants.MinWheelSize, Constants.MaxWheelSize);
    }
}
