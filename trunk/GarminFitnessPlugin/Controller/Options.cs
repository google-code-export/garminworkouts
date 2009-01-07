using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class Options : IPluginSerializable, IXMLSerializable
    {
        public override void Serialize(Stream stream)
        {
            // Write the different options that are logbook related
            // Use ST HR zones
            stream.Write(BitConverter.GetBytes(UseSportTracksHeartRateZones), 0, sizeof(bool));

            // Use ST speed zones
            stream.Write(BitConverter.GetBytes(UseSportTracksSpeedZones), 0, sizeof(bool));

            // Cadence
            stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(Options.Instance.CadenceZoneCategory.ReferenceId)), 0, sizeof(int));
            stream.Write(Encoding.UTF8.GetBytes(CadenceZoneCategory.ReferenceId), 0, Encoding.UTF8.GetByteCount(Options.Instance.CadenceZoneCategory.ReferenceId));

            // Cadence dirty flag
            stream.Write(BitConverter.GetBytes(IsCadenceZoneDirty), 0, sizeof(bool));

            // Power
            // Use ST power zones
            stream.Write(BitConverter.GetBytes(UseSportTracksPowerZones), 0, sizeof(bool));
            stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(Options.Instance.PowerZoneCategory.ReferenceId)), 0, sizeof(int));
            stream.Write(Encoding.UTF8.GetBytes(PowerZoneCategory.ReferenceId), 0, Encoding.UTF8.GetByteCount(Options.Instance.PowerZoneCategory.ReferenceId));

            // Power dirty flag
            stream.Write(BitConverter.GetBytes(IsPowerZoneDirty), 0, sizeof(bool));

            // Garmin to ST category map
            Dictionary<IActivityCategory, GarminCategories>.Enumerator iter = Options.Instance.STToGarminCategoryMap.GetEnumerator();
            stream.Write(BitConverter.GetBytes(STToGarminCategoryMap.Count), 0, sizeof(int));
            while (iter.MoveNext())
            {
                // ST category Id
                stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(iter.Current.Key.ReferenceId)), 0, sizeof(int));
                stream.Write(Encoding.UTF8.GetBytes(iter.Current.Key.ReferenceId), 0, Encoding.UTF8.GetByteCount(iter.Current.Key.ReferenceId));

                // Mapped Garmin category
                stream.Write(BitConverter.GetBytes((int)iter.Current.Value), 0, sizeof(int));
            }
        }

        public new void Deserialize(Stream stream, DataVersion version)
        {
            IsDeserializing = true;

            // Set default Garmin to ST category map values
            ResetLogbookSettings();

            base.Deserialize(stream, version);

            IsDeserializing = false;
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            CadenceZoneCategory = PluginMain.GetApplication().Logbook.CadenceZones[0];
            PowerZoneCategory = PluginMain.GetApplication().Logbook.PowerZones[0];
        }

        public void Deserialize_V2(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] stringBuffer;
            int stringLength;

            // Read options that are stored in logbook
            // Cadence zone
            stream.Read(intBuffer, 0, sizeof(int));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            CadenceZoneCategory = Utils.FindZoneCategoryByID(PluginMain.GetApplication().Logbook.CadenceZones, Encoding.UTF8.GetString(stringBuffer));

            // Power zone
            stream.Read(intBuffer, 0, sizeof(int));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            PowerZoneCategory = Utils.FindZoneCategoryByID(PluginMain.GetApplication().Logbook.PowerZones, Encoding.UTF8.GetString(stringBuffer));
        }

        public void Deserialize_V3(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] boolBuffer = new byte[sizeof(bool)];
            byte[] stringBuffer;
            int stringLength;

            // Read options that are stored in logbook
            // Cadence zone
            stream.Read(intBuffer, 0, sizeof(int));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            CadenceZoneCategory = Utils.FindZoneCategoryByID(PluginMain.GetApplication().Logbook.CadenceZones, Encoding.UTF8.GetString(stringBuffer));

            // Cadence dirty flag
            stream.Read(boolBuffer, 0, sizeof(bool));
            IsCadenceZoneDirty = BitConverter.ToBoolean(boolBuffer, 0);

            // Power zone
            stream.Read(intBuffer, 0, sizeof(int));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            PowerZoneCategory = Utils.FindZoneCategoryByID(PluginMain.GetApplication().Logbook.PowerZones, Encoding.UTF8.GetString(stringBuffer));

            // Power dirty flag
            stream.Read(boolBuffer, 0, sizeof(bool));
            IsPowerZoneDirty = BitConverter.ToBoolean(boolBuffer, 0);
        }

        public void Deserialize_V7(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] stringBuffer;
            int mappingCount;
            int stringLength;

            Deserialize_V3(stream, version);

            // Load Garmin to ST category map
            stream.Read(intBuffer, 0, sizeof(int));
            mappingCount = BitConverter.ToInt32(intBuffer, 0);

            for (int i = 0; i < mappingCount; ++i)
            {
                int garminCategory;

                // ST category Id
                stream.Read(intBuffer, 0, sizeof(int));
                stringLength = BitConverter.ToInt32(intBuffer, 0);
                stringBuffer = new byte[stringLength];
                stream.Read(stringBuffer, 0, stringLength);

                // Mapped Garmin category
                stream.Read(intBuffer, 0, sizeof(int));
                garminCategory = BitConverter.ToInt32(intBuffer, 0);

                STToGarminCategoryMap[Utils.FindCategoryByIDSafe(Encoding.UTF8.GetString(stringBuffer))] = (GarminCategories)garminCategory;
            }
        }

        public void Deserialize_V8(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] boolBuffer = new byte[sizeof(bool)];
            byte[] stringBuffer;
            int mappingCount;
            int stringLength;

            // Read options that are stored in logbook
            // Use ST HR zones
            stream.Read(boolBuffer, 0, sizeof(bool));
            UseSportTracksHeartRateZones = BitConverter.ToBoolean(boolBuffer, 0);

            // Use ST speed zones
            stream.Read(boolBuffer, 0, sizeof(bool));
            UseSportTracksSpeedZones = BitConverter.ToBoolean(boolBuffer, 0);

            // Cadence zone
            stream.Read(intBuffer, 0, sizeof(int));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            CadenceZoneCategory = Utils.FindZoneCategoryByID(PluginMain.GetApplication().Logbook.CadenceZones, Encoding.UTF8.GetString(stringBuffer));

            // Cadence dirty flag
            stream.Read(boolBuffer, 0, sizeof(bool));
            IsCadenceZoneDirty = BitConverter.ToBoolean(boolBuffer, 0);

            // Use ST power zones
            stream.Read(boolBuffer, 0, sizeof(bool));
            UseSportTracksPowerZones = BitConverter.ToBoolean(boolBuffer, 0);

            // Power zone
            stream.Read(intBuffer, 0, sizeof(int));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            PowerZoneCategory = Utils.FindZoneCategoryByID(PluginMain.GetApplication().Logbook.PowerZones, Encoding.UTF8.GetString(stringBuffer));

            // Power dirty flag
            stream.Read(boolBuffer, 0, sizeof(bool));
            IsPowerZoneDirty = BitConverter.ToBoolean(boolBuffer, 0);

            // Load Garmin to ST category map
            stream.Read(intBuffer, 0, sizeof(int));
            mappingCount = BitConverter.ToInt32(intBuffer, 0);

            for (int i = 0; i < mappingCount; ++i)
            {
                int garminCategory;

                // ST category Id
                stream.Read(intBuffer, 0, sizeof(int));
                stringLength = BitConverter.ToInt32(intBuffer, 0);
                stringBuffer = new byte[stringLength];
                stream.Read(stringBuffer, 0, stringLength);

                // Mapped Garmin category
                stream.Read(intBuffer, 0, sizeof(int));
                garminCategory = BitConverter.ToInt32(intBuffer, 0);

                STToGarminCategoryMap[Utils.FindCategoryByIDSafe(Encoding.UTF8.GetString(stringBuffer))] = (GarminCategories)garminCategory;
            }
        }
        public void Serialize(System.Xml.XmlNode parentNode, String nodeName, System.Xml.XmlDocument document)
        {
            XmlNode child;

            // Default export directory
            child = document.CreateElement("DefaultExportDirectory");
            child.AppendChild(document.CreateTextNode(DefaultExportDirectory));
            parentNode.AppendChild(child);

            // SplitPanel sizes
            child = document.CreateElement("CategoriesSplitDistance");
            child.AppendChild(document.CreateTextNode(CategoriesPanelSplitSize.ToString()));
            parentNode.AppendChild(child);

            child = document.CreateElement("WorkoutSplitDistance");
            child.AppendChild(document.CreateTextNode(WorkoutPanelSplitSize.ToString()));
            parentNode.AppendChild(child);

            child = document.CreateElement("StepSplitDistance");
            child.AppendChild(document.CreateTextNode(StepPanelSplitSize.ToString()));
            parentNode.AppendChild(child);

            child = document.CreateElement("CalendarSplitDistance");
            child.AppendChild(document.CreateTextNode(CalendarPanelSplitSize.ToString()));
            parentNode.AppendChild(child);

            child = document.CreateElement("StepNotesSplitDistance");
            child.AppendChild(document.CreateTextNode(StepNotesSplitSize.ToString()));
            parentNode.AppendChild(child);

            child = document.CreateElement("DonationReminderDate");
            child.AppendChild(document.CreateTextNode(DonationReminderDate.Ticks.ToString()));
            parentNode.AppendChild(child);

            child = document.CreateElement("EnableAutoSplitWorkouts");
            child.AppendChild(document.CreateTextNode(AllowSplitWorkouts.ToString()));
            parentNode.AppendChild(child);
        }

        public void Deserialize(System.Xml.XmlNode parentNode)
        {
            IsDeserializing = true;

            // Set default cadence zone
            if (Options.Instance.CadenceZoneCategory == null)
            {
                Options.Instance.CadenceZoneCategory = PluginMain.GetApplication().Logbook.CadenceZones[0];
            }

            // Set default power zone
            if (Options.Instance.PowerZoneCategory == null)
            {
                Options.Instance.PowerZoneCategory = PluginMain.GetApplication().Logbook.PowerZones[0];
            }

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode child = parentNode.ChildNodes[i];

                ///////////////////////////////////////////////////////////
                // These are here for backwards compatibility
                ///////////////////////////////////////////////////////////
                // HR
                if (child.Name == "UseSTHeartRateZones" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.UseSportTracksHeartRateZones = bool.Parse(child.FirstChild.Value);
                }
                // Speed
                else if (child.Name == "UseSTSpeedZones" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.UseSportTracksSpeedZones = bool.Parse(child.FirstChild.Value);
                }
                // Cadence
                else if (child.Name == "CadenceZoneRefId" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.CadenceZoneCategory = Utils.FindZoneCategoryByID(PluginMain.GetApplication().Logbook.CadenceZones, child.FirstChild.Value);
                }
                // Power
                else if (child.Name == "UseSTPowerZones" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.UseSportTracksPowerZones = bool.Parse(child.FirstChild.Value);
                }
                else if (child.Name == "PowerZoneRefId" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.PowerZoneCategory = Utils.FindZoneCategoryByID(PluginMain.GetApplication().Logbook.PowerZones, child.FirstChild.Value);
                }
                ///////////////////////////////////////////////////////////
                // End backwards compatibility
                ///////////////////////////////////////////////////////////

                // Default export directory
                else if (child.Name == "DefaultExportDirectory" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.DefaultExportDirectory = child.FirstChild.Value;
                }
                // Split distances
                else if (child.Name == "CategoriesSplitDistance" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.CategoriesPanelSplitSize = int.Parse(child.FirstChild.Value);
                }
                else if (child.Name == "WorkoutSplitDistance" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.WorkoutPanelSplitSize = int.Parse(child.FirstChild.Value);
                }
                else if (child.Name == "StepSplitDistance" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.StepPanelSplitSize = int.Parse(child.FirstChild.Value);
                }
                else if (child.Name == "CalendarSplitDistance" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.CalendarPanelSplitSize = int.Parse(child.FirstChild.Value);
                }
                else if (child.Name == "StepNotesSplitDistance" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.StepNotesSplitSize = int.Parse(child.FirstChild.Value);
                }
                else if (child.Name == "DonationReminderDate" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.DonationReminderDate = new DateTime(long.Parse(child.FirstChild.Value));
                }
                else if (child.Name == "EnableAutoSplitWorkouts" && child.ChildNodes.Count == 1)
                {
                    Options.Instance.AllowSplitWorkouts = bool.Parse(child.FirstChild.Value);
                }
            }

            IsDeserializing = false;
        }

        public void OnActivityCategoryChanged(object sender, IActivityCategory categoryChanged)
        {
            List<IActivityCategory> itemsToDelete = new List<IActivityCategory>();
            Dictionary<IActivityCategory, GarminCategories>.KeyCollection.Enumerator iter = m_STToGarminCategoryMap.Keys.GetEnumerator();
            while(iter.MoveNext())
            {
                if (Utils.FindCategoryByID(iter.Current.ReferenceId) == null)
                {
                    itemsToDelete.Add(iter.Current);
                }
            }

            for (int i = 0; i < itemsToDelete.Count; ++i)
            {
                m_STToGarminCategoryMap.Remove(itemsToDelete[i]);
            }
        }

        public void ResetLogbookSettings()
        {
            // Logbook settings
            m_UseSportTracksHeartRateZones = true;
            m_UseSportTracksSpeedZones = true;
            m_UseSportTracksPowerZones = true;
            m_CadenceZoneCategory = PluginMain.GetApplication().Logbook.CadenceZones[0];
            m_PowerZoneCategory = PluginMain.GetApplication().Logbook.PowerZones[0];

            m_IsPowerZoneDirty = false;
            m_IsCadenceZoneDirty = false;

            // Set default Garmin to ST category map values
            m_STToGarminCategoryMap = new Dictionary<IActivityCategory, GarminCategories>();
            ClearAllGarminCategories();
            for (int i = 0; i < PluginMain.GetApplication().Logbook.ActivityCategories.Count; ++i)
            {
                SetGarminCategory(PluginMain.GetApplication().Logbook.ActivityCategories[i], GarminCategories.Other);
            }

            TriggerOptionsChangedEvent("");
        }

        protected void TriggerOptionsChangedEvent(string propertyName)
        {
            if (!IsDeserializing)
            {
                Utils.SaveWorkoutsToLogbook();
            }

            if (OptionsChanged != null)
            {
                OptionsChanged(new PropertyChangedEventArgs(propertyName));
            }
        }

        public GarminCategories GetGarminCategory(IActivityCategory STCategory)
        {
            if (m_STToGarminCategoryMap.ContainsKey(STCategory))
            {
                return m_STToGarminCategoryMap[STCategory];
            }
            else
            {
                return GetGarminCategory(STCategory.Parent);
            }
        }

        public bool IsCustomGarminCategory(IActivityCategory STCategory)
        {
            return STToGarminCategoryMap.ContainsKey(STCategory);
        }

        public void ClearAllGarminCategories()
        {
            STToGarminCategoryMap.Clear();
        }

        public void SetGarminCategory(IActivityCategory STCategory, GarminCategories GarminCategory)
        {
            if (!m_STToGarminCategoryMap.ContainsKey(STCategory) ||
                m_STToGarminCategoryMap[STCategory] != GarminCategory)
            {
                m_STToGarminCategoryMap[STCategory] = GarminCategory;

                TriggerOptionsChangedEvent("STToGarminCategoryMap");
            }
        }

        public void RemoveGarminCategory(IActivityCategory STCategory)
        {
            if (m_STToGarminCategoryMap.ContainsKey(STCategory))
            {
                m_STToGarminCategoryMap.Remove(STCategory);

                TriggerOptionsChangedEvent("STToGarminCategoryMap");
            }
        }

        public static Options Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new Options();
                }

                return m_Instance;
            }
        }

        public int CategoriesPanelSplitSize
        {
            get { return m_CategoriesPanelSplitSize; }
            set
            {
                if (m_CategoriesPanelSplitSize != value)
                {
                    m_CategoriesPanelSplitSize = value;
                }
            }
        }

        public int WorkoutPanelSplitSize
        {
            get { return m_WorkoutPanelSplitSize; }
            set
            {
                if(m_WorkoutPanelSplitSize != value)
                {
                    m_WorkoutPanelSplitSize = value;
                }
            }
        }

        public int StepPanelSplitSize
        {
            get { return m_StepPanelSplitSize; }
            set
            {
                if (m_StepPanelSplitSize != value)
                {
                    m_StepPanelSplitSize = value;
                }
            }
        }

        public int CalendarPanelSplitSize
        {
            get { return m_CalendarPanelSplitSize; }
            set
            {
                if (m_CalendarPanelSplitSize != value)
                {
                    m_CalendarPanelSplitSize = value;
                }
            }
        }

        public int StepNotesSplitSize
        {
            get { return m_StepNotesSplitSize; }
            set
            {
                if (m_StepNotesSplitSize != value)
                {
                    m_StepNotesSplitSize = value;
                }
            }
        }

        public bool UseSportTracksHeartRateZones
        {
            get { return m_UseSportTracksHeartRateZones; }
            set
            {
                if (m_UseSportTracksHeartRateZones != value)
                {
                    m_UseSportTracksHeartRateZones = value;

                    TriggerOptionsChangedEvent("UseSportTracksHeartRateZones");
                }
            }
        }

        public bool UseSportTracksSpeedZones
        {
            get { return m_UseSportTracksSpeedZones; }
            set
            {
                if (m_UseSportTracksSpeedZones != value)
                {
                    m_UseSportTracksSpeedZones = value;

                    TriggerOptionsChangedEvent("UseSportTracksSpeedZones");
                }
            }
        }

        public bool UseSportTracksPowerZones
        {
            get { return m_UseSportTracksPowerZones; }
            set
            {
                if (m_UseSportTracksPowerZones != value)
                {
                    m_UseSportTracksPowerZones = value;

                    TriggerOptionsChangedEvent("UseSportTracksPowerZones");
                }
            }
        }

        public IZoneCategory CadenceZoneCategory
        {
            get { return m_CadenceZoneCategory; }
            set
            {
                if (m_CadenceZoneCategory != value)
                {
                    m_CadenceZoneCategory = value;
                    IsCadenceZoneDirty = false;

                    if (m_CadenceZoneCategory != null)
                    {
                        GarminWorkoutManager.Instance.MarkAllCadenceSTZoneTargetsAsDirty();
                    }

                    TriggerOptionsChangedEvent("CadenceZoneCategory");
                }
            }
        }

        public IZoneCategory PowerZoneCategory
        {
            get { return m_PowerZoneCategory; }
            set
            {
                if (m_PowerZoneCategory != value)
                {
                    m_PowerZoneCategory = value;
                    IsPowerZoneDirty = false;

                    if (m_PowerZoneCategory != null)
                    {
                        GarminWorkoutManager.Instance.MarkAllPowerSTZoneTargetsAsDirty();
                    }

                    TriggerOptionsChangedEvent("PowerZoneCategory");
                }
            }
        }

        public String DefaultExportDirectory
        {
            get { return m_DefaultExportDirectory; }
            set
            {
                if (m_DefaultExportDirectory != value)
                {
                    m_DefaultExportDirectory = value;

                    TriggerOptionsChangedEvent("DefaultExportDirectory");
                }
            }
        }

        public bool IsCadenceZoneDirty
        {
            get { return m_IsCadenceZoneDirty; }
            set
            {
                if (m_IsCadenceZoneDirty != value)
                {
                    m_IsCadenceZoneDirty = value;

                    TriggerOptionsChangedEvent("IsCadenceZoneDirty");
                }
            }
        }

        public bool IsPowerZoneDirty
        {
            get { return m_IsPowerZoneDirty; }
            set
            {
                if (m_IsPowerZoneDirty != value)
                {
                    m_IsPowerZoneDirty = value;

                    TriggerOptionsChangedEvent("IsPowerZoneDirty");
                }
            }
        }

        public DateTime DonationReminderDate
        {
            get { return m_DonationReminderDate; }
            set { m_DonationReminderDate = value; }
        }

        public bool AllowSplitWorkouts
        {
            get { return m_EnableAutoSplitWorkouts; }
            set
            {
                if (AllowSplitWorkouts != value)
                {
                    m_EnableAutoSplitWorkouts = value;

                    // Don't trigger when deserializing so the popup doesn't appear...
                    if (!IsDeserializing)
                    {
                        TriggerOptionsChangedEvent("EnableAutoSplitWorkouts");
                    }
                }
            }
        }

        private Dictionary<IActivityCategory, GarminCategories>  STToGarminCategoryMap
        {
            get { return m_STToGarminCategoryMap; }
            set
            {
                if (m_STToGarminCategoryMap != value)
                {
                    m_STToGarminCategoryMap = value;

                    TriggerOptionsChangedEvent("STToGarminCategoryMap");
                }
            }
        }

        private bool IsDeserializing
        {
            get { return m_IsDeserializing; }
            set { m_IsDeserializing = value; }
        }

        private static Options m_Instance = null;

        private bool m_IsDeserializing = false;

        public delegate void OptionsChangedEventHandler(PropertyChangedEventArgs changedProperty);
        public event OptionsChangedEventHandler OptionsChanged;

        private int m_CategoriesPanelSplitSize = 200;
        private int m_WorkoutPanelSplitSize = 180;
        private int m_StepPanelSplitSize = 500;
        private int m_CalendarPanelSplitSize = 325;
        private int m_StepNotesSplitSize = 250;

        private bool m_UseSportTracksHeartRateZones;
        private bool m_UseSportTracksSpeedZones;
        private bool m_UseSportTracksPowerZones;
        private IZoneCategory m_CadenceZoneCategory;
        private IZoneCategory m_PowerZoneCategory;
        private Dictionary<IActivityCategory, GarminCategories> m_STToGarminCategoryMap = new Dictionary<IActivityCategory, GarminCategories>();
        private String m_DefaultExportDirectory;

        private DateTime m_DonationReminderDate = DateTime.Today;

        private bool m_EnableAutoSplitWorkouts = true;

        private bool m_IsPowerZoneDirty = false;
        private bool m_IsCadenceZoneDirty = false;
    }
}
