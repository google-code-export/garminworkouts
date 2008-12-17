using System;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.Common.Data.Fitness;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessNamedLowHighZone : INamedLowHighZone
    {
        public GarminFitnessNamedLowHighZone(float min, float max, string name)
        {
            m_MinValue = min;
            m_MaxValue = max;
            m_Name = name;
        }

#region INamedLowHighZone Members

        public float Low
        {
            get { return m_MinValue; }
            set
            {
                if (value > m_MaxValue)
                {
                    m_MinValue = value;
                    m_MaxValue = value;
                }
                if (m_MinValue != value)
                {
                    m_MinValue = value;
                }
            }
        }

        public float High
        {
            get { return m_MaxValue; }
            set
            {
                if (value < m_MinValue)
                {
                    m_MinValue = value;
                    m_MaxValue = value;
                }
                if (m_MaxValue != value)
                {
                    m_MaxValue = value;
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
                }
            }
        }

#endregion

        private float m_MinValue;
        private float m_MaxValue;
        private string m_Name;
    }
}
