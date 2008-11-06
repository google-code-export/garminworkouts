using System;
using System.Collections.Generic;
using System.Text;

namespace GarminFitnessPlugin.Data
{
    class DataTooRecentException : Exception
    {
        public DataTooRecentException(Byte dataVersionNumber)
        {
            m_DataVersionNumber = dataVersionNumber;
        }

        public Byte DataVersionNumber
        {
            get { return m_DataVersionNumber; }
        }

        private Byte m_DataVersionNumber;
    }
}
