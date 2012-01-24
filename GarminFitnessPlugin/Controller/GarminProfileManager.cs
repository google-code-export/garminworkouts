using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    public class GarminProfileManager : IPluginSerializable, IXMLSerializable
    {
        private GarminProfileManager()
        {
            UserProfile.Cleanup();

            UserProfile.ProfileChanged += new GarminProfile.ProfileChangedEventHandler(OnUserProfileChanged);
            UserProfile.ActivityProfileChanged += new GarminProfile.ActivityProfileChangedEventHandler(OnUserActivityProfileChanged);
        }

        private void OnUserProfileChanged(object sender, PropertyChangedEventArgs changedProperty)
        {
            if (ProfileChanged != null)
            {
                ProfileChanged(sender, changedProperty);
            }
        }

        private void OnUserActivityProfileChanged(GarminActivityProfile profileChanged, PropertyChangedEventArgs changedProperty)
        {
            if (ActivityProfileChanged != null)
            {
                ActivityProfileChanged(profileChanged, changedProperty);
            }
        }

        public override void Serialize(Stream stream)
        {
            m_UserProfile.Serialize(stream);
        }

        public void Deserialize_V8(Stream stream, DataVersion version)
        {
            UserProfile.Deserialize(stream, version);
        }
        
        public virtual void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            UserProfile.Serialize(parentNode, nodeName, document);
        }

        public virtual void Deserialize(XmlNode parentNode)
        {
            GarminProfile newProfile = UserProfile.Clone();

            newProfile.Deserialize(parentNode);

            // Deserialization succeeded, use the new copy
            UserProfile.ProfileChanged -= new GarminProfile.ProfileChangedEventHandler(OnUserProfileChanged);
            UserProfile.ActivityProfileChanged -= new GarminProfile.ActivityProfileChangedEventHandler(OnUserActivityProfileChanged);

            UserProfile = newProfile;

            UserProfile.ProfileChanged += new GarminProfile.ProfileChangedEventHandler(OnUserProfileChanged);
            UserProfile.ActivityProfileChanged += new GarminProfile.ActivityProfileChangedEventHandler(OnUserActivityProfileChanged);

            if (ProfileChanged != null)
            {
                ProfileChanged(UserProfile, new PropertyChangedEventArgs("Profile"));
            }

            if (ActivityProfileChanged != null)
            {
                for (int i = 0; i < (int)GarminCategories.GarminCategoriesCount; ++i)
                {
                    ActivityProfileChanged(GetProfileForActivity((GarminCategories)i), new PropertyChangedEventArgs("ActivityProfile"));
                }
            }
        }

        public GarminActivityProfile GetProfileForActivity(GarminCategories category)
        {
            return UserProfile.GetProfileForActivity(category);
        }

        public static GarminProfileManager Instance
        {
            get { return m_Instance; }
        }

        public GarminProfile UserProfile
        {
            get { return m_UserProfile; }
            set
            {
                m_UserProfile = value;
            }
        }

        public delegate void ProfileChangedEventHandler(object sender, PropertyChangedEventArgs changedProperty);
        public event ProfileChangedEventHandler ProfileChanged;

        public delegate void ActivityProfileChangedEventHandler(GarminActivityProfile profileChanged, PropertyChangedEventArgs changedProperty);
        public event ActivityProfileChangedEventHandler ActivityProfileChanged;


        private static GarminProfileManager m_Instance = new GarminProfileManager();

        private GarminProfile m_UserProfile = new GarminProfile();
    }
}
