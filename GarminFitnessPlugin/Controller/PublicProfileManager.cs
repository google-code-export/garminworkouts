using System;
using System.Collections.Generic;
using System.IO;
using GarminFitnessPublic;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class PublicProfileManager : IPublicProfileManager
    {
        private PublicProfileManager()
        {
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

        void IPublicProfileManager.SerializeProfile(String directory)
        {
            ProfileExporter.ExportProfileToFIT(directory);
        }

#endregion

        private static PublicProfileManager m_Instance = null;
    }
}
