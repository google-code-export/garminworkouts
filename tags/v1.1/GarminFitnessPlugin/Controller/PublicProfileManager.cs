using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using GarminFitnessPublic;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class PublicProfileManager : IPublicProfileManager
    {
        private PublicProfileManager()
        {
            GarminProfileManager.Instance.ProfileChanged += new GarminProfileManager.ProfileChangedEventHandler(OnManagerProfileChanged);
            GarminProfileManager.Instance.ActivityProfileChanged += new GarminProfileManager.ActivityProfileChangedEventHandler(OnManagerActivityProfileChanged);
        }

        void OnManagerProfileChanged(object sender, System.ComponentModel.PropertyChangedEventArgs changedProperty)
        {
            if (ProfileChanged != null)
            {
                ProfileChanged(this, new PropertyChangedEventArgs("Profile"));
            }
        }

        void OnManagerActivityProfileChanged(GarminActivityProfile profileChanged, System.ComponentModel.PropertyChangedEventArgs changedProperty)
        {
            if (ProfileChanged != null)
            {
                ProfileChanged(this, new PropertyChangedEventArgs("ActivityProfile"));
            }
        }

        public static IPublicProfileManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new PublicProfileManager();
                }

                return m_Instance;
            }
        }

#region IPublicProfileManager Members

        public void SerializeProfile(String directory)
        {
            ProfileExporter.ExportProfileToFIT(directory);
        }

        public event PropertyChangedEventHandler ProfileChanged;

#endregion

        private static PublicProfileManager m_Instance = null;
    }
}
