using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
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
            m_GearWeight = 0;
            m_HRIsInPercentMax = true;
            m_SpeedIsInPace = false;

            // HR Zones
            Byte startHR = 100;
            for(int i = 0; i < Constants.GarminHRZoneCount; ++i)
            {
                m_HeartRateZonesMaximums.Add(startHR);

                startHR += 20;
            }

            // Speed Zones
            float startSpeed = 10;
            for(int i = 0; i < Constants.GarminSpeedZoneCount; ++i)
            {
                String zoneName = GarminFitnessView.ResourceManager.GetString("GTCSpeedZone" + (i + 1).ToString() + "Text", GarminFitnessView.UICulture);

                m_SpeedZonesNames.Add(zoneName);
                m_SpeedZonesMaximums.Add(startSpeed);

                startSpeed += 5;
            }
        }

        public override void Serialize(Stream stream)
        {
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
        }

        public void Deserialize_V8(Stream stream, DataVersion version)
        {
        }

        public virtual void Serialize(XmlNode parentNode, XmlDocument document)
        {
        }

        public virtual bool Deserialize(XmlNode parentNode)
        {
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

            Byte BPMValue;

            if (index == 0)
            {
                BPMValue = Constants.MinHRInBPM;
            }
            else
            {
                BPMValue = m_HeartRateZonesMaximums[index - 1];
            }

            if (HRIsInPercentMax)
            {
                return (Byte)Math.Round((BPMValue * 100) / (float)MaximumHeartRate, 0, MidpointRounding.AwayFromZero);
            }
            else
            {
                return BPMValue;
            }
        }

        public Byte GetHeartRateHighLimit(int index)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminHRZoneCount);

            Byte BPMValue;
            if (index == Constants.GarminHRZoneCount - 1)
            {
                BPMValue = MaximumHeartRate;
            }
            else
            {
                BPMValue = m_HeartRateZonesMaximums[index];
            }

            if (HRIsInPercentMax)
            {
                return (Byte)Math.Round((BPMValue * 100) / (float)MaximumHeartRate, 0, MidpointRounding.AwayFromZero);
            }
            else
            {
                return BPMValue;
            }
        }

        public void SetHeartRateLowLimit(int index, Byte value)
        {
            Trace.Assert(index > 0 && index < Constants.GarminHRZoneCount);
            Byte BPMValue = value;

            // Convert to BPM if value is in % HRMax
            if (HRIsInPercentMax)
            {
                BPMValue = (Byte)Math.Round((value / 100.0f) * MaximumHeartRate, 0, MidpointRounding.AwayFromZero);
            }

            if (m_HeartRateZonesMaximums[index - 1] != BPMValue)
            {
                m_HeartRateZonesMaximums[index - 1] = BPMValue;

                TriggerChangedEvent(new PropertyChangedEventArgs("HeartRateZoneLimit"));
            }
        }

        public void SetHeartRateHighLimit(int index, Byte value)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminHRZoneCount - 1);
            Byte BPMValue = value;

            // Convert to BPM if value is in % HRMax
            if (HRIsInPercentMax)
            {
                BPMValue = (Byte)Math.Round((value / 100.0f) * MaximumHeartRate, 0, MidpointRounding.AwayFromZero);
            }

            if (m_HeartRateZonesMaximums[index] != BPMValue)
            {
                m_HeartRateZonesMaximums[index] = BPMValue;

                TriggerChangedEvent(new PropertyChangedEventArgs("HeartRateZoneLimit"));
            }
        }

        public double GetSpeedLowLimit(int index)
        {
            Trace.Assert(index >= 0 && index < Constants.GarminSpeedZoneCount);

            double speedValue;
            if (index == 0)
            {
                speedValue = 0;
            }
            else
            {
                speedValue = m_SpeedZonesMaximums[index - 1];
            }

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

            double speedValue = m_SpeedZonesMaximums[index];

            if (SpeedIsInPace)
            {
                return Utils.SpeedToPace(speedValue);
            }
            else
            {
                return speedValue;
            }
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
                    m_MaxHeartRate = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("MaximumHeartRate"));
                }
            }
        }

        public double GearWeight
        {
            get { return m_GearWeight; }
            set
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

        public delegate void ActivityProfileChangedEventHandler(GarminActivityProfile sender, PropertyChangedEventArgs changedProperty);
        public event ActivityProfileChangedEventHandler ActivityProfileChanged;

        private GarminCategories m_Category;
        private Byte m_MaxHeartRate;
        private double m_GearWeight;
        private bool m_HRIsInPercentMax;
        private bool m_SpeedIsInPace;
        private List<Byte> m_HeartRateZonesMaximums = new List<Byte>();
        private List<double> m_SpeedZonesMaximums = new List<double>();
        private List<string> m_SpeedZonesNames = new List<string>();
    }
}
