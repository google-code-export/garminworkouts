using System;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    class GarminPowerZoneWrapper : TreeList.TreeListNode, IGarminZoneWrapper
    {
        public GarminPowerZoneWrapper(GarminCategories category, int zoneIndex)
            : base(null, null)
        {
            UpdateProfile(category);
            m_ZoneIndex = zoneIndex;
        }

        public void UpdateProfile(GarminCategories category)
        {
            GarminActivityProfile profile = GarminProfileManager.Instance.GetProfileForActivity(category);

            if (profile.GetType() == typeof(GarminBikingActivityProfile))
            {
                m_Profile = (GarminBikingActivityProfile)profile;
            }
            else
            {
                m_Profile = null;
            }
        }

        public int Index
        {
            get { return m_ZoneIndex; }
        }

        public String Name
        {
            get { return (m_ZoneIndex + 1).ToString(); }
        }

        public String Low
        {
            get
            {
                if (m_Profile != null)
                {
                    return m_Profile.GetPowerLowLimit(m_ZoneIndex).ToString();
                }

                return String.Empty;
            }
        }

        public String High
        {
            get
            {
                if (m_Profile != null)
                {
                    return m_Profile.GetPowerHighLimit(m_ZoneIndex).ToString();
                }

                return String.Empty;
            }
        }

        private int m_ZoneIndex;
        private GarminBikingActivityProfile m_Profile;
    }
}
