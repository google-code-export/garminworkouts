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
    class GarminProfileManager : IPluginSerializable, IXMLSerializable
    {
        private GarminProfileManager()
        {
            Cleanup();
        }

        void OnProfileManagerActivityProfileChanged(GarminActivityProfile profileModified, PropertyChangedEventArgs changedProperty)
        {
            TriggerActivityProfileChangedEvent(profileModified, changedProperty);
        }

        public void Cleanup()
        {
            if (m_ActivityProfiles != null)
            {
                for (int i = 0; i < m_ActivityProfiles.Length; ++i)
                {
                    m_ActivityProfiles[i].ActivityProfileChanged -= new GarminActivityProfile.ActivityProfileChangedEventHandler(OnProfileManagerActivityProfileChanged);
                }
            }

            m_ActivityProfiles = new GarminActivityProfile[]
                {
                    new GarminActivityProfile(GarminCategories.Running),
                    new GarminBikingActivityProfile(GarminCategories.Biking),
                    new GarminActivityProfile(GarminCategories.Other)
                };

            for (int i = 0; i < m_ActivityProfiles.Length; ++i)
            {
                m_ActivityProfiles[i].ActivityProfileChanged += new GarminActivityProfile.ActivityProfileChangedEventHandler(OnProfileManagerActivityProfileChanged);
            }

            m_ProfileName = "New User";
            m_IsGenderMale = true;
            m_WeightInPounds = 150.0f;
            m_BirthDate = DateTime.Today;
        }

        public override void Serialize(Stream stream)
        {
            // Name
            stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(ProfileName)), 0, sizeof(Int32));
            stream.Write(Encoding.UTF8.GetBytes(ProfileName), 0, Encoding.UTF8.GetByteCount(ProfileName));

            // Gender
            stream.Write(BitConverter.GetBytes(IsMale), 0, sizeof(bool));

            // Weight in pounds
            stream.Write(BitConverter.GetBytes(WeightInPounds), 0, sizeof(double));

            // Birth date
            stream.Write(BitConverter.GetBytes(BirthDate.Ticks), 0, sizeof(long));

            // Resting HR
            stream.WriteByte(RestingHeartRate);

            // Activity profiles
            for (int i = 0; i < (int)GarminCategories.GarminCategoriesCount; ++i)
            {
                m_ActivityProfiles[i].Serialize(stream);
            }
        }

        public new void Deserialize(Stream stream, DataVersion version)
        {
            IsDeserializing = true;

            Cleanup();
            base.Deserialize(stream, version);

            IsDeserializing = false;
        }

        public override void Deserialize_V0(Stream stream, DataVersion version)
        {
            Cleanup();
        }

        public void Deserialize_V8(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(int)];
            byte[] boolBuffer = new byte[sizeof(bool)];
            byte[] doubleBuffer = new byte[sizeof(double)];
            byte[] longBuffer = new byte[sizeof(long)];
            byte[] stringBuffer;
            Int32 stringLength;

            // Name
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            ProfileName = Encoding.UTF8.GetString(stringBuffer);

            // Gender
            stream.Read(boolBuffer, 0, sizeof(bool));
            IsMale = BitConverter.ToBoolean(boolBuffer, 0);

            // Weight in pounds
            stream.Read(doubleBuffer, 0, sizeof(double));
            WeightInPounds = BitConverter.ToDouble(doubleBuffer, 0);

            // Birth date
            stream.Read(longBuffer, 0, sizeof(long));
            BirthDate = new DateTime(BitConverter.ToInt64(longBuffer, 0));

            // Resting HR
            RestingHeartRate = (Byte)stream.ReadByte();

            for (int i = 0; i < (int)GarminCategories.GarminCategoriesCount; ++i)
            {
                m_ActivityProfiles[i].Deserialize(stream, version);
            }
        }

        public virtual void Serialize(XmlNode parentNode, XmlDocument document)
        {
        }

        public virtual bool Deserialize(XmlNode parentNode)
        {
            bool birthDateRead = false;
            bool weightRead = false;
            bool genderRead = false;
            int activitiesReadCount = 0;
            DateTime birthDate = DateTime.Now;
            double weightInPounds = 0;
            bool isMale = true;
            GarminActivityProfile[] profiles;

            profiles = new GarminActivityProfile[]
                {
                    new GarminActivityProfile(GarminCategories.Running),
                    new GarminBikingActivityProfile(GarminCategories.Biking),
                    new GarminActivityProfile(GarminCategories.Other)
                };

            for(int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == Constants.BirthDateTCXString &&
                    currentChild.ChildNodes.Count == 1 &&
                    currentChild.FirstChild.GetType() == typeof(XmlText))
                {
                    string date = currentChild.FirstChild.Value;
                    CultureInfo culture = new CultureInfo("en-us");

                    birthDateRead = DateTime.TryParse(date, culture.DateTimeFormat, DateTimeStyles.None, out birthDate);
                }
                else if (currentChild.Name == Constants.WeightTCXString &&
                         currentChild.ChildNodes.Count == 1 &&
                         currentChild.FirstChild.GetType() == typeof(XmlText))
                {
                    double weight;

                    if (!Utils.IsTextFloatInRange(currentChild.FirstChild.Value, Constants.MinWeight, Constants.MaxWeight))
                    {
                        return false;
                    }

                    weight = double.Parse(currentChild.FirstChild.Value);
                    weightInPounds = Weight.Convert(weight, Weight.Units.Kilogram, Weight.Units.Pound);
                    weightRead = true;
                }
                else if (currentChild.Name == Constants.GenderTCXString &&
                         currentChild.ChildNodes.Count == 1 &&
                         currentChild.FirstChild.GetType() == typeof(XmlText))
                {
                    isMale = currentChild.FirstChild.Value == Constants.GenderMaleTCXString;
                    genderRead = true;
                }
                else if (currentChild.Name == "Activities")
                {
                    string activityType = PeekActivityType(currentChild);

                    for (int j = 0; j < (int)GarminCategories.GarminCategoriesCount; ++j)
                    {
                        if(activityType == Constants.GarminCategoryTCXString[j])
                        {
                            if (!profiles[j].Deserialize(currentChild))
                            {
                                return false;
                            }

                            activitiesReadCount++;

                            break;
                        }
                    }
                }
            }

            // Check if all was read successfully
            if (!birthDateRead || !weightRead || !genderRead || activitiesReadCount != 3)
            {
                return false;
            }

            // Officialize
            BirthDate = birthDate;
            WeightInPounds = weightInPounds;
            IsMale = isMale;

            for (int i = 0; i < m_ActivityProfiles.Length; ++i)
            {
                m_ActivityProfiles[i].ActivityProfileChanged -= new GarminActivityProfile.ActivityProfileChangedEventHandler(OnProfileManagerActivityProfileChanged);
            }

            m_ActivityProfiles = profiles;

            for (int i = 0; i < m_ActivityProfiles.Length; ++i)
            {
                m_ActivityProfiles[i].ActivityProfileChanged += new GarminActivityProfile.ActivityProfileChangedEventHandler(OnProfileManagerActivityProfileChanged);
            }

            TriggerActivityProfileChangedEvent(m_ActivityProfiles[0], new PropertyChangedEventArgs(""));
            TriggerActivityProfileChangedEvent(m_ActivityProfiles[1], new PropertyChangedEventArgs(""));
            TriggerActivityProfileChangedEvent(m_ActivityProfiles[2], new PropertyChangedEventArgs(""));

            return true;
        }

        private string PeekActivityType(XmlNode activityNode)
        {
            for (int i = 0; i < activityNode.Attributes.Count; ++i)
            {
                XmlAttribute attribute = activityNode.Attributes[i];

                if (attribute.Name == "Sport")
                {
                    return attribute.Value;
                }
            }

            return String.Empty;
        }

        public void SetWeightInUnits(double weight, Weight.Units unit)
        {
            // Convert to pounds
            WeightInPounds = Weight.Convert(weight, unit, Weight.Units.Pound);
        }

        private void TriggerChangedEvent(PropertyChangedEventArgs args)
        {
            if (!IsDeserializing)
            {
                Utils.SaveWorkoutsToLogbook();
            }

            if (ProfileChanged != null)
            {
                ProfileChanged(this, args);
            }
        }

        private void TriggerActivityProfileChangedEvent(GarminActivityProfile profileModified, PropertyChangedEventArgs args)
        {
            if (!IsDeserializing)
            {
                Utils.SaveWorkoutsToLogbook();
            }

            if (ActivityProfileChanged != null)
            {
                ActivityProfileChanged(profileModified, args);
            }
        }

        public static GarminProfileManager Instance
        {
            get { return m_Instance; }
        }

        public String ProfileName
        {
            get { return m_ProfileName; }
            set
            {
                if (m_ProfileName != value)
                {
                    m_ProfileName = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("ProfileName"));
                }
            }
        }

        public bool IsMale
        {
            get { return m_IsGenderMale; }
            set
            {
                if (m_IsGenderMale != value)
                {
                    m_IsGenderMale = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("Gender"));
                }
            }
        }

        public double WeightInPounds
        {
            get { return m_WeightInPounds; }
            private set
            {
                if (m_WeightInPounds != value)
                {
                    m_WeightInPounds = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("Weight"));
                }
            }
        }

        public DateTime BirthDate
        {
            get { return m_BirthDate; }
            set
            {
                if (m_BirthDate != value)
                {
                    m_BirthDate = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("BirthDate"));
                }
            }
        }

        public Byte RestingHeartRate
        {
            get
            {
                Trace.Assert(m_ActivityProfiles[2].RestingHeartRate == m_ActivityProfiles[1].RestingHeartRate &&
                             m_ActivityProfiles[1].RestingHeartRate == m_ActivityProfiles[0].RestingHeartRate);

                return m_ActivityProfiles[0].RestingHeartRate;
            }
            set
            {
                if (RestingHeartRate != value)
                {
                    for (int i = 0; i < (int)GarminCategories.GarminCategoriesCount; ++i)
                    {
                        m_ActivityProfiles[i].RestingHeartRate = value;
                    }

                    TriggerChangedEvent(new PropertyChangedEventArgs("RestingHeartRate"));
                }
            }
        }

        public GarminActivityProfile GetProfileForActivity(GarminCategories category)
        {
            if (m_ActivityProfiles.Length == (int)GarminCategories.GarminCategoriesCount)
            {
                return m_ActivityProfiles[(int)category];
            }

            return null;
        }

        private bool IsDeserializing
        {
            get { return m_IsDeserializing; }
            set { m_IsDeserializing = value; }
        }

        public delegate void ProfileChangedEventHandler(object sender, PropertyChangedEventArgs changedProperty);
        public event ProfileChangedEventHandler ProfileChanged;

        public delegate void ActivityProfileChangedEventHandler(GarminActivityProfile profileChanged, PropertyChangedEventArgs changedProperty);
        public event ActivityProfileChangedEventHandler ActivityProfileChanged;

        private static GarminProfileManager m_Instance = new GarminProfileManager();
        private String m_ProfileName;
        private bool m_IsGenderMale;
        private double m_WeightInPounds;
        private DateTime m_BirthDate;
        private GarminActivityProfile[] m_ActivityProfiles;

        private bool m_IsDeserializing = false;
    }
}
