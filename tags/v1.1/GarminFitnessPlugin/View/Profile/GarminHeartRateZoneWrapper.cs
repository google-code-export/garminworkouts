using System;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    class GarminHeartRateZoneWrapper : TreeList.TreeListNode, IGarminZoneWrapper
    {
        public GarminHeartRateZoneWrapper(GarminCategories category, int zoneIndex)
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
            get { return (m_ZoneIndex + 1).ToString(); }
        }

        public String Low
        {
            get { return m_Profile.GetHeartRateLowLimit(m_ZoneIndex).ToString(); }
        }

        public String High
        {
            get { return m_Profile.GetHeartRateHighLimit(m_ZoneIndex).ToString(); }
        }

        private int m_ZoneIndex;
        private GarminActivityProfile m_Profile;
    }
}
