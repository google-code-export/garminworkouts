using System;
using System.Collections.Generic;
using System.Text;

namespace GarminWorkoutPlugin.Data
{
    public class DataVersion
    {
        public DataVersion(byte versionNumber)
        {
            m_VersionNumber = versionNumber;
        }

        public byte VersionNumber
        {
            get { return m_VersionNumber; }
            set { m_VersionNumber = value; }
        }

        byte m_VersionNumber;
    }
}
