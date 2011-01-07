using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class GarminProfile : IPluginSerializable, IXMLSerializable
    {
        public GarminProfile()
        {
            Cleanup();
        }

        void OnActivityProfileChanged(GarminActivityProfile profileModified, PropertyChangedEventArgs changedProperty)
        {
            TriggerActivityProfileChangedEvent(profileModified, changedProperty);
        }

        public GarminProfile Clone()
        {
            GarminProfile clone = new GarminProfile();
            MemoryStream tempStream = new MemoryStream();

            Serialize(tempStream);
            tempStream.Position = 0;
            clone.Deserialize(tempStream, Constants.CurrentVersion);
            tempStream.Close();

            return clone;
        }

        public void Cleanup()
        {
            if (m_ActivityProfiles != null)
            {
                for (int i = 0; i < m_ActivityProfiles.Length; ++i)
                {
                    m_ActivityProfiles[i].ActivityProfileChanged -= new GarminActivityProfile.ActivityProfileChangedEventHandler(OnActivityProfileChanged);
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
                m_ActivityProfiles[i].ActivityProfileChanged += new GarminActivityProfile.ActivityProfileChangedEventHandler(OnActivityProfileChanged);
            }

            ProfileName = "New User";
            IsMale = true;
            WeightInKilos = 70;
            BirthDate = DateTime.Today;
        }

        public override void Serialize(Stream stream)
        {
            m_ProfileName.Serialize(stream);

            m_IsGenderMale.Serialize(stream);

            m_WeightInKilos.Serialize(stream);

            m_BirthDate.Serialize(stream);

            m_RestingHeartRate.Serialize(stream);

            // Activity profiles
            for (int i = 0; i < (int)GarminCategories.GarminCategoriesCount; ++i)
            {
                m_ActivityProfiles[i].Serialize(stream);
            }
        }

        public void SerializeToFITSettings(Stream outputStream)
        {
            FITMessage userProfileMessage = new FITMessage(FITGlobalMessageIds.UserProfile);
            FITMessageField profileName = new FITMessageField((Byte)FITUserProfileFieldIds.FriendlyName);
            FITMessageField gender = new FITMessageField((Byte)FITUserProfileFieldIds.Gender);
            FITMessageField weight = new FITMessageField((Byte)FITUserProfileFieldIds.Weight);
            FITMessageField restingHR = new FITMessageField((Byte)FITUserProfileFieldIds.RestingHR);

            profileName.SetString(ProfileName, 16);
            userProfileMessage.AddField(profileName);

            gender.SetEnum(IsMale ? (Byte)FITGenders.Male : (Byte)FITGenders.Female);
            userProfileMessage.AddField(gender);

            weight.SetUInt16((UInt16)(WeightInKilos * 10));
            userProfileMessage.AddField(weight);

            restingHR.SetUInt8(RestingHeartRate);
            userProfileMessage.AddField(restingHR);

            userProfileMessage.Serialize(outputStream);

            (m_ActivityProfiles[(int)GarminCategories.Biking] as GarminBikingActivityProfile).SerializeBikeProfiles(outputStream);
        }

        public void SerializeToFITSport(Stream outputStream, GarminCategories category)
        {
            m_ActivityProfiles[(int)category].SerializeToFITSport(outputStream);
        }

        public new void Deserialize(Stream stream, DataVersion version)
        {
            IsDeserializing = true;

            Cleanup();
            base.Deserialize(stream, version);

            IsDeserializing = false;
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
        }

        public void Deserialize_V8(Stream stream, DataVersion version)
        {
            Deserialize_V10(stream, version);

            // Weight units changed from lbs to kg
            WeightInKilos = Weight.Convert(m_WeightInKilos, Weight.Units.Pound, Weight.Units.Kilogram);
        }

        public void Deserialize_V10(Stream stream, DataVersion version)
        {
            m_ProfileName.Deserialize(stream, version);

            m_IsGenderMale.Deserialize(stream, version);

            m_WeightInKilos.Deserialize(stream, version);

            m_BirthDate.Deserialize(stream, version);

            m_RestingHeartRate.Deserialize(stream, version);

            for (int i = 0; i < (int)GarminCategories.GarminCategoriesCount; ++i)
            {
                m_ActivityProfiles[i].Deserialize(stream, version);
            }
        }

        public void DeserializeFromFIT(Stream importStream)
        {
            FITMessageField profileName = null;
            FITMessageField gender = null;
            FITMessageField weight = null;
            FITMessageField restingHR = null;

            GarminActivityProfile[] profiles;

            profiles = new GarminActivityProfile[]
                {
                    m_ActivityProfiles[0].Clone(),
                    m_ActivityProfiles[1].Clone(),
                    m_ActivityProfiles[2].Clone()
                };

            Logger.Instance.LogText("Init parser in profile");

            if (FITParser.Instance.Init(importStream))
            {
                FITMessage parsedMessage = null;
                FITMessage sportMessage = null;
                FITSports fileSportType = FITSports.Other;

                do
                {
                    parsedMessage = FITParser.Instance.ReadNextMessage();

                    if (parsedMessage != null)
                    {
                        Logger.Instance.LogText(String.Format("FIT parsed message type={0:0}", (int)parsedMessage.GlobalMessageType));

                        switch (parsedMessage.GlobalMessageType)
                        {
                            case FITGlobalMessageIds.FileId:
                                {
                                    // Make sure we have a profile file (settings or sport)
                                    FITMessageField fileTypeField = parsedMessage.GetField((Byte)FITFileIdFieldsIds.FileType);

                                    if (fileTypeField == null ||
                                        ((FITFileTypes)fileTypeField.GetEnum() != FITFileTypes.Settings &&
                                         (FITFileTypes)fileTypeField.GetEnum() != FITFileTypes.Sport))
                                    {
                                        Logger.Instance.LogText("Not a profile FIT file");
                                        return;
                                    }

                                    if ((FITFileTypes)fileTypeField.GetEnum() == FITFileTypes.Sport)
                                    {
                                        // Prefetch the sport message so we know what to deal with before parsing the other
                                        //  messages that contain sport relative data
                                        sportMessage = FITParser.Instance.PrefetchMessageOfType(FITGlobalMessageIds.Sport);

                                        fileSportType = (FITSports)sportMessage.GetField((Byte)FITSportFieldIds.Sport).GetEnum();
                                    }

                                    break;
                                }
                            case FITGlobalMessageIds.UserProfile:
                                {
                                    profileName = parsedMessage.GetField((Byte)FITUserProfileFieldIds.FriendlyName);
                                    gender = parsedMessage.GetField((Byte)FITUserProfileFieldIds.Gender);
                                    weight = parsedMessage.GetField((Byte)FITUserProfileFieldIds.Weight);
                                    restingHR = parsedMessage.GetField((Byte)FITUserProfileFieldIds.RestingHR);
                                    break;
                                }
                            case FITGlobalMessageIds.BikeProfile:
                                {
                                    (profiles[(int)GarminCategories.Biking] as GarminBikingActivityProfile).DeserializeBikeProfile(parsedMessage);
                                    break;
                                }
                            case FITGlobalMessageIds.ZonesTarget:
                                {
                                    if (sportMessage != null)
                                    {
                                        GarminActivityProfile activityProfile = profiles[(int)Options.Instance.GetGarminCategory(fileSportType)];

                                        activityProfile.DeserializeZonesTargetFromFIT(parsedMessage);
                                    }
                                    else
                                    {
                                        throw new FITParserException("No sport defined for zones target");
                                    }
                                    break;
                                }
                            case FITGlobalMessageIds.HRZones:
                                {
                                    if (sportMessage != null)
                                    {
                                        GarminActivityProfile activityProfile = profiles[(int)Options.Instance.GetGarminCategory(fileSportType)];

                                        activityProfile.DeserializeHRZonesFromFIT(parsedMessage);
                                    }
                                    else
                                    {
                                        throw new FITParserException("No sport defined for HR zones");
                                    }
                                    break;
                                }
                            case FITGlobalMessageIds.SpeedZones:
                                {
                                    if (sportMessage != null)
                                    {
                                        GarminActivityProfile activityProfile = profiles[(int)Options.Instance.GetGarminCategory(fileSportType)];

                                        activityProfile.DeserializeSpeedZonesFromFIT(parsedMessage);
                                    }
                                    else
                                    {
                                        throw new FITParserException("No sport defined for speed zones");
                                    }
                                    break;
                                }
                            case FITGlobalMessageIds.PowerZones:
                                {
                                    if (sportMessage != null)
                                    {
                                        GarminActivityProfile activityProfile = profiles[(int)Options.Instance.GetGarminCategory(fileSportType)];

                                        activityProfile.DeserializePowerZonesFromFIT(parsedMessage);
                                    }
                                    else
                                    {
                                        throw new FITParserException("No sport defined for power zones");
                                    }
                                    break;
                                }
                            default:
                                {
                                    // Nothing to do, unsupported message
                                    break;
                                }
                        }
                    }
                }
                while (parsedMessage != null);

                FITParser.Instance.Close();

                // Saved loaded values
                if (profileName != null)
                {
                    ProfileName = profileName.GetString();
                }

                if (gender != null)
                {
                    IsMale = gender.GetEnum() == (Byte)FITGenders.Male;
                }

                if (weight != null)
                {
                    WeightInKilos = weight.GetUInt16() / 10.0;
                }

                if (restingHR != null)
                {
                    RestingHeartRate = restingHR.GetUInt8();
                }

                // Everything was correctly deserialized, so update with the new profiles
                for (int i = 0; i < m_ActivityProfiles.Length; ++i)
                {
                    m_ActivityProfiles[i].ActivityProfileChanged -= new GarminActivityProfile.ActivityProfileChangedEventHandler(OnActivityProfileChanged);
                }

                m_ActivityProfiles = profiles;

                for (int i = 0; i < m_ActivityProfiles.Length; ++i)
                {
                    m_ActivityProfiles[i].ActivityProfileChanged += new GarminActivityProfile.ActivityProfileChangedEventHandler(OnActivityProfileChanged);
                }

                TriggerActivityProfileChangedEvent(m_ActivityProfiles[0], new PropertyChangedEventArgs(""));
                TriggerActivityProfileChangedEvent(m_ActivityProfiles[1], new PropertyChangedEventArgs(""));
                TriggerActivityProfileChangedEvent(m_ActivityProfiles[2], new PropertyChangedEventArgs(""));
            }
        }

        public virtual void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            XmlNode profileNode;
            XmlNode currentChild;

            profileNode = document.CreateElement(Constants.ProfileTCXString);
            parentNode.AppendChild(profileNode);

            XmlAttribute attributeNode = document.CreateAttribute("xmlns");
            attributeNode.Value = "http://www.garmin.com/xmlschemas/UserProfile/v2";
            profileNode.Attributes.Append(attributeNode);

            m_BirthDate.Serialize(profileNode, Constants.BirthDateTCXString, document);

            m_WeightInKilos.Serialize(profileNode, Constants.WeightTCXString, document);

            m_IsGenderMale.Serialize(profileNode, Constants.GenderTCXString, document);

            // Activities
            for (int i = 0; i < (int)GarminCategories.GarminCategoriesCount; ++i)
            {
                m_ActivityProfiles[i].Serialize(profileNode, Constants.ActivitiesTCXString, document);
            }

            // Timestamp
            currentChild = document.CreateElement(Constants.TimeStampTCXString);
            currentChild.AppendChild(document.CreateTextNode(DateTime.Now.ToString("yyyy-MM-ddThh:mm:ssZ")));
            profileNode.AppendChild(currentChild);
        }

        public virtual void Deserialize(XmlNode parentNode)
        {
            bool birthDateRead = false;
            bool weightRead = false;
            bool genderRead = false;
            int activitiesReadCount = 0;
            GarminActivityProfile[] profiles;

            profiles = new GarminActivityProfile[]
                {
                    m_ActivityProfiles[0].Clone(),
                    m_ActivityProfiles[1].Clone(),
                    m_ActivityProfiles[2].Clone()
                };

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode currentChild = parentNode.ChildNodes[i];

                if (currentChild.Name == Constants.BirthDateTCXString)
                {
                    m_BirthDate.Deserialize(currentChild);
                    birthDateRead = true;
                }
                else if (currentChild.Name == Constants.WeightTCXString)
                {
                    m_WeightInKilos.Deserialize(currentChild);
                    weightRead = true;
                }
                else if (currentChild.Name == Constants.GenderTCXString)
                {
                    m_IsGenderMale.Deserialize(currentChild);
                    genderRead = true;
                }
                else if (currentChild.Name == Constants.ActivitiesTCXString)
                {
                    string activityType = PeekActivityType(currentChild);

                    for (int j = 0; j < (int)GarminCategories.GarminCategoriesCount; ++j)
                    {
                        if (activityType == Constants.GarminCategoryTCXString[j])
                        {
                            profiles[j].Deserialize(currentChild);
                            activitiesReadCount++;
                            break;
                        }
                    }
                }
            }

            // Check if all was read successfully
            if (!birthDateRead || !weightRead || !genderRead || activitiesReadCount != 3)
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in profile XML node", parentNode);
            }

            // Everything was correctly deserialized, so update with the new profiles
            for (int i = 0; i < m_ActivityProfiles.Length; ++i)
            {
                m_ActivityProfiles[i].ActivityProfileChanged -= new GarminActivityProfile.ActivityProfileChangedEventHandler(OnActivityProfileChanged);
            }

            m_ActivityProfiles = profiles;

            for (int i = 0; i < m_ActivityProfiles.Length; ++i)
            {
                m_ActivityProfiles[i].ActivityProfileChanged += new GarminActivityProfile.ActivityProfileChangedEventHandler(OnActivityProfileChanged);
            }

            TriggerActivityProfileChangedEvent(m_ActivityProfiles[0], new PropertyChangedEventArgs(""));
            TriggerActivityProfileChangedEvent(m_ActivityProfiles[1], new PropertyChangedEventArgs(""));
            TriggerActivityProfileChangedEvent(m_ActivityProfiles[2], new PropertyChangedEventArgs(""));
        }

        public void Serialize(GarXFaceNet._FitnessUserProfile userProfile)
        {
            userProfile.SetBirthDay((UInt32)BirthDate.Day);
            userProfile.SetBirthMonth((UInt32)BirthDate.Month);
            userProfile.SetBirthYear((UInt32)BirthDate.Year);
            userProfile.SetGender(IsMale ? GarXFaceNet._FitnessUserProfile.GenderTypes.Male : GarXFaceNet._FitnessUserProfile.GenderTypes.Female);
            userProfile.SetUsersWeight((float)WeightInKilos);

            for(UInt32 i = 0; i < (int)GarminCategories.GarminCategoriesCount; ++i)
            {
                GarminActivityProfile activity  = GetProfileForActivity((GarminCategories)i);

                activity.Serialize(userProfile.GetActivity(i));
            }
        }

        public void Deserialize(GarXFaceNet._FitnessUserProfile userProfile)
        {
            BirthDate = new DateTime((int)userProfile.GetBirthYear(), (int)userProfile.GetBirthMonth(), (int)userProfile.GetBirthDay());
            IsMale = (userProfile.GetGender() == GarXFaceNet._FitnessUserProfile.GenderTypes.Male);
            WeightInKilos = userProfile.GetUsersWeight();

            for (UInt32 i = 0; i < (int)GarminCategories.GarminCategoriesCount; ++i)
            {
                GarminActivityProfile activity = GetProfileForActivity((GarminCategories)i);

                activity.Deserialize(userProfile.GetActivity(i));
            }
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
            WeightInKilos = Weight.Convert(weight, unit, Weight.Units.Kilogram);
        }

        public double GetWeightInUnits(Weight.Units unit)
        {
            // Convert to unit
            return Weight.Convert(WeightInKilos, Weight.Units.Kilogram, unit);
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

        public String ProfileName
        {
            get { return m_ProfileName; }
            set
            {
                if (m_ProfileName != value)
                {
                    m_ProfileName.Value = value;

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
                    m_IsGenderMale.Value = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("Gender"));
                }
            }
        }

        private double WeightInKilos
        {
            get { return m_WeightInKilos; }
            set
            {
                if (m_WeightInKilos != value)
                {
                    m_WeightInKilos.Value = value;

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
                    m_BirthDate.Value = value;

                    TriggerChangedEvent(new PropertyChangedEventArgs("BirthDate"));
                }
            }
        }

        public GarminFitnessByteRange InternalRestingHeartRate
        {
            get { return m_RestingHeartRate; }
        }

        public Byte RestingHeartRate
        {
            get
            {
                return m_RestingHeartRate;
            }
            set
            {
                if (m_RestingHeartRate != value)
                {
                    m_RestingHeartRate.Value = value;

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

        private GarminFitnessString m_ProfileName = new GarminFitnessString("New User");
        private GarminFitnessByteRange m_RestingHeartRate = new GarminFitnessByteRange(60, Constants.MinHRInBPM, Constants.MaxHRInBPM);
        private GarminFitnessBool m_IsGenderMale = new GarminFitnessBool(true, Constants.GenderMaleTCXString, Constants.GenderFemaleTCXString);
        private GarminFitnessDoubleRange m_WeightInKilos = new GarminFitnessDoubleRange(70, Constants.MinWeight, Constants.MaxWeightInKg);
        private GarminFitnessDate m_BirthDate = new GarminFitnessDate(DateTime.Today);
        private GarminActivityProfile[] m_ActivityProfiles;

        private bool m_IsDeserializing = false;
    }
}
