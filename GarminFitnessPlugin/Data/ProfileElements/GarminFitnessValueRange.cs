using System;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.Common.Data;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessValueRange<T> : IValueRange<T> where T : IComparable<T>
    {
        public GarminFitnessValueRange(T lower, T upper)
        {
            Lower = lower;
            Upper = upper;
        }

        public T Lower
        {
            get { return m_Lower; }
            set
            {
                int comparisonResult = value.CompareTo(Upper);

                if (comparisonResult > 0)
                {
                    m_Lower = value;
                    m_Upper = value;
                }
                else if (comparisonResult != 0)
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
                int comparisonResult = value.CompareTo(Lower);

                if (comparisonResult < 0)
                {
                    m_Lower = value;
                    m_Upper = value;
                }
                else if (comparisonResult != 0)
                {
                    m_Upper = value;
                }
            }
        }

        private T m_Lower;
        private T m_Upper;
    }
}
