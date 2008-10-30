using System.IO;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class GarminProfileManager : IPluginSerializable
    {
        private GarminProfileManager()
        {
            m_GarminProfiles = new GarminActivityProfile[]
                {
                    new GarminActivityProfile(GarminCategories.Running),
                    new GarminExtendedActivityProfile(GarminCategories.Cycling),
                    new GarminActivityProfile(GarminCategories.Other)
                };
        }

        public override void Serialize(Stream stream)
        {
            for (int i = 0; i < (int)GarminCategories.GarminCategoriesCount; ++i)
            {
                m_GarminProfiles[i].Serialize(stream);
            }
        }

        public void Deserialize_V8(Stream stream, DataVersion version)
        {
            for (int i = 0; i < (int)GarminCategories.GarminCategoriesCount; ++i)
            {
                m_GarminProfiles[i].Deserialize(stream, version);
            }
        }

        public static GarminProfileManager Instance
        {
            get { return m_Instance; }
        }

        private static GarminProfileManager m_Instance = new GarminProfileManager();
        private GarminActivityProfile[] m_GarminProfiles;
    }
}
