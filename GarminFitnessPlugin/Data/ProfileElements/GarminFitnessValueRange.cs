using System;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.Common.Data;

namespace GarminFitnessPlugin.Data
{
    public class GarminFitnessValueRange<T> : IValueRange<T> where T : IComparable<T>
    {
        public GarminFitnessValueRange(T lower, T upper)
        {
            if (lower.CompareTo(upper) > 0)
            {
                m_Lower = upper;
                m_Upper = lower;
            }
            else
            {
                m_Lower = lower;
                m_Upper = upper;
            }
        }

        public T Lower
        {
            get { return m_Lower; }
            set
            {
                if (value.CompareTo(Upper) > 0)
                {
                    m_Lower = value;
                    m_Upper = value;
                }
                else if (value.CompareTo(Lower) != 0)
                {
                    m_Lower = value;
                }
            }
        }

        public T Upper
        {
            get { return m_Upper; }
            set
            {
                if (value.CompareTo(Lower) < 0)
                {
                    m_Lower = value;
                    m_Upper = value;
                }
                else if (value.CompareTo(Upper) != 0)
                {
                    m_Upper = value;
                }
            }
        }

        private T m_Lower;
        private T m_Upper;
    }
}
