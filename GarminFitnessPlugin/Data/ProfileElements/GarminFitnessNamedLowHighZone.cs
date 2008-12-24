using System;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.Common.Data.Fitness;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessNamedSpeedZone
    {
        public GarminFitnessNamedSpeedZone(double min, double max, string name)
        {
            Low = Math.Min(min, max);
            High = Math.Max(min, max);
            Name = name;
        }

        public double Low
        {
            get { return m_MinValue; }
            set
            {
                if (value.CompareTo(m_MaxValue) > 0)
                {
                    m_MinValue.Value = value;
                    m_MaxValue.Value = value;
                }
                if (Low != value)
                {
                    m_MinValue.Value = value;
                }
            }
        }

        public double High
        {
            get { return m_MaxValue; }
            set
            {
                if (value.CompareTo(m_MinValue) < 0)
                {
                    m_MinValue.Value = value;
                    m_MaxValue.Value = value;
                }
                if (High != value)
                {
                    m_MaxValue.Value = value;
                }
            }
        }

        public string Name
        {
            get { return m_Name; }
            set
            {
                if (Name != value)
                {
                    m_Name.Value = value;
                }
            }
        }

        public GarminFitnessDoubleRange InternalLow
        {
            get { return m_MinValue; }
        }

        public GarminFitnessDoubleRange InternalHigh
        {
            get { return m_MaxValue; }
        }

        public GarminFitnessString InternalName
        {
            get { return m_Name; }
        }

        private GarminFitnessDoubleRange m_MinValue = new GarminFitnessDoubleRange(Constants.MinSpeedMetersPerSecond, Constants.MinSpeedMetersPerSecond, Constants.MaxSpeedMetersPerSecond);
        private GarminFitnessDoubleRange m_MaxValue = new GarminFitnessDoubleRange(Constants.MinSpeedMetersPerSecond, Constants.MinSpeedMetersPerSecond, Constants.MaxSpeedMetersPerSecond);
        private GarminFitnessString m_Name = new GarminFitnessString("", 15);
    }
}
