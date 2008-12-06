using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.View;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin
{
    class PluginMain : IPlugin
    {
        #region IPlugin Members

        public IApplication Application
        {
            set
            {
                if (m_App != null)
                {
                    m_App.PropertyChanged -= new PropertyChangedEventHandler(AppPropertyChanged);
                }

                m_App = value;

                if (m_App != null)
                {
                    m_App.PropertyChanged += new PropertyChangedEventHandler(AppPropertyChanged);
                }
            }
        }

        void AppPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null && e.PropertyName == "Logbook")
            {
                if (m_CurrentLogbook != null)
                {
                    m_CurrentLogbook.DataChanged -= new ZoneFiveSoftware.Common.Data.NotifyDataChangedEventHandler(LogbookDataChanged);
                }

                LoadWorkoutsFromLogbook();
                if (LogbookChanged != null)
                {
                    LogbookChanged(this, m_CurrentLogbook, m_App.Logbook);
                }

                m_CurrentLogbook = m_App.Logbook;

                if (m_CurrentLogbook != null)
                {
                    m_CurrentLogbook.DataChanged += new ZoneFiveSoftware.Common.Data.NotifyDataChangedEventHandler(LogbookDataChanged);

                    if (m_PluginOptions != null)
                    {
                        Options.Instance.Deserialize(m_PluginOptions);
                    }
                }
            }
        }

        void LogbookDataChanged(object sender, ZoneFiveSoftware.Common.Data.NotifyDataChangedEventArgs e)
        {
            IList referenceList = null;

            if (e.Action == ZoneFiveSoftware.Common.Data.NotifyDataChangedAction.PropertySet)
            {
                referenceList = e.NewItems;
            }
            else if (e.Action == ZoneFiveSoftware.Common.Data.NotifyDataChangedAction.Remove)
            {
                referenceList = e.OldItems;
            }

            if (referenceList != null)
            {
                Debug.Assert(referenceList.Count == 1);

                if (referenceList[0].GetType().FullName == "ZoneFiveSoftware.SportTracks.Data.ZoneCategory")
                {
                    if (ZoneCategoryChanged != null)
                    {
                        ZoneCategoryChanged(this, (IZoneCategory)referenceList[0]);
                    }
                }
                else if (referenceList[0].GetType().FullName == "ZoneFiveSoftware.SportTracks.Data.ActivityCategory")
                {
                    if (ActivityCategoryChanged != null)
                    {
                        ActivityCategoryChanged(this, (IActivityCategory)referenceList[0]);
                    }
                }
            }
        }

        public Guid Id
        {
            get { return GUIDs.PluginMain; }
        }

        public string Name
        {
            get { return GetType().Assembly.GetName().Name; }
        }

        public void ReadOptions(XmlDocument xmlDoc, XmlNamespaceManager nsmgr, XmlElement pluginNode)
        {
            m_PluginOptions = pluginNode;

            ActivityCategoryChanged += new PluginMain.ActivityCategoryChangedEventHandler(Options.Instance.OnActivityCategoryChanged);
        }

        public string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(3); }
        }

        public void WriteOptions(XmlDocument xmlDoc, XmlElement pluginNode)
        {
            Options.Instance.Serialize(pluginNode, xmlDoc);
        }

        #endregion

        public static IApplication GetApplication()
        {
            return m_App;
        }

        private static void LoadWorkoutsFromLogbook()
        {
            // Load data from logbook
            byte[] extensionData = PluginMain.GetApplication().Logbook.GetExtensionData(GUIDs.PluginMain);
            MemoryStream stream = new MemoryStream(extensionData);

            if (extensionData != null && extensionData.Length > 0)
            {
                try
                {
                    byte[] headerBuffer = new byte[Constants.DataHeaderIdString.Length];
                    String headerIdString;
                    DataVersion version = new DataVersion(0);

                    stream.Read(headerBuffer, 0, Constants.DataHeaderIdString.Length);
                    headerIdString = Encoding.UTF8.GetString(headerBuffer);

                    if (headerIdString == Constants.DataHeaderIdString)
                    {
                        Byte versionNumber = (Byte)stream.ReadByte();

                        if (versionNumber <= Constants.CurrentVersion.VersionNumber)
                        {
                            version = new DataVersion(versionNumber);
                        }
                        else
                        {
                            MessageBox.Show(GarminFitnessView.GetLocalizedString("DataTooRecentErrorText"),
                                            GarminFitnessView.GetLocalizedString("ErrorText"),
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return;
                        }
                    }
                    else
                    {
                        // Deserialize using version 0.  Replace at start since we skipped some data
                        stream.Position = 0;
                    }

                    Options.Instance.Deserialize(stream, version);
                    GarminWorkoutManager.Instance.Deserialize(stream, version);
                    GarminProfileManager.Instance.Deserialize(stream, version);
                }
                catch (Exception e)
                {
                     throw e;
                }
            }
            else
            {
                GarminWorkoutManager.Instance.RemoveAllWorkouts();
                GarminProfileManager.Instance.Cleanup();
                Options.Instance.ResetSettings();

                // Show the wizard on first run
                GarminFitnessSetupWizard wizard = new GarminFitnessSetupWizard();

                wizard.ShowDialog();
            }

            stream.Close();
        }

        public delegate void LogbookChangedEventHandler(object sender, ILogbook oldLogbook, ILogbook newLogbook); 
        public static event LogbookChangedEventHandler LogbookChanged;

        public delegate void ZoneCategoryChangedEventHandler(object sender, IZoneCategory category);
        public static event ZoneCategoryChangedEventHandler ZoneCategoryChanged;

        public delegate void ActivityCategoryChangedEventHandler(object sender, IActivityCategory category);
        public static event ActivityCategoryChangedEventHandler ActivityCategoryChanged;

        private static IApplication m_App = null;
        private static ILogbook m_CurrentLogbook = null;
        XmlNode m_PluginOptions;
    }
}
