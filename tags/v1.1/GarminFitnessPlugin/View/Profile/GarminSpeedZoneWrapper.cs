using System;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    class GarminSpeedZoneWrapper : TreeList.TreeListNode, IGarminZoneWrapper
    {
        public GarminSpeedZoneWrapper(GarminCategories category, int zoneIndex)
            : base(null, null)
        {
            m_Profile = GarminProfileManager.Instance.GetProfileForActivity(category);
            m_ZoneIndex = zoneIndex;
        }

        public void UpdateProfile(GarminCategories category)
        {
            m_Profile = GarminProfileManager.Instance.GetProfileForActivity(category);
        }

        public int Index
        {
            get { return m_ZoneIndex; }
        }

        public String Name
        {
            get { return m_Profile.GetSpeedZoneName(m_ZoneIndex); }
        }

        public String Low
        {
            get
            {
                double value = m_Profile.GetSpeedLowLimit(m_ZoneIndex);
                string result;

                if (m_Profile.SpeedIsInPace)
                {
                    UInt16 min, sec;

                    Utils.DoubleToTime(value, out min, out sec);
                    result = String.Format("{0:00}:{1:00}", min, sec);
                }
                else
                {
                    result = value.ToString("0.0");
                }

                return result;
            }
        }

        public String High
        {
            get
            {
                double value = m_Profile.GetSpeedHighLimit(m_ZoneIndex);
                string result;

                if (m_Profile.SpeedIsInPace)
                {
                    UInt16 min, sec;

                    Utils.DoubleToTime(value, out min, out sec);
                    result = String.Format("{0:00}:{1:00}", min, sec);
                }
                else
                {
                    result = value.ToString("0.0");
                }

                return result;
            }
        }

        private int m_ZoneIndex;
        private GarminActivityProfile m_Profile;
    }
}
