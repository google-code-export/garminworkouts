using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using GarminWorkoutPlugin.Data;
using GarminWorkoutPlugin.View;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin
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

                    if (Options.CadenceZoneCategory == null)
                    {
                        Options.CadenceZoneCategory = m_CurrentLogbook.CadenceZones[0];
                    }

                    if (Options.PowerZoneCategory == null)
                    {
                        Options.PowerZoneCategory = m_CurrentLogbook.PowerZones[0];
                    }

                    if (m_PluginOptions != null)
                    {
                        for (int i = 0; i < m_PluginOptions.ChildNodes.Count; ++i)
                        {
                            XmlNode child = m_PluginOptions.ChildNodes[i];

                            // HR
                            if (child.Name == "UseSTHeartRateZones")
                            {
                                Options.UseSportTracksHeartRateZones = bool.Parse(child.FirstChild.Value);
                            }
                            // Speed
                            else if (child.Name == "UseSTSpeedZones")
                            {
                                Options.UseSportTracksSpeedZones = bool.Parse(child.FirstChild.Value);
                            }
                            // Cadence
                            else if (child.Name == "CadenceZoneRefId")
                            {
                                Options.CadenceZoneCategory = Utils.FindZoneCategoryByID(m_CurrentLogbook.CadenceZones, child.FirstChild.Value);
                            }
                            // Power
                            else if (child.Name == "UseSTPowerZones")
                            {
                                Options.UseSportTracksPowerZones = bool.Parse(child.FirstChild.Value);
                            }
                            else if (child.Name == "PowerZoneRefId")
                            {
                                Options.PowerZoneCategory = Utils.FindZoneCategoryByID(m_CurrentLogbook.PowerZones, child.FirstChild.Value);
                            }
                            // Split distances
                            else if (child.Name == "CategoriesSplitDistance")
                            {
                                Options.CategoriesPanelSplitSize = int.Parse(child.FirstChild.Value);
                            }
                            else if (child.Name == "WorkoutSplitDistance")
                            {
                                Options.WorkoutPanelSplitSize = int.Parse(child.FirstChild.Value);
                            }
                            else if (child.Name == "StepSplitDistance")
                            {
                                Options.StepPanelSplitSize = int.Parse(child.FirstChild.Value);
                            }
                            else if (child.Name == "CalendarSplitDistance")
                            {
                                Options.CalendarPanelSplitSize = int.Parse(child.FirstChild.Value);
                            }
                            // Default export directory
                            else if (child.Name == "DefaultExportDirectory")
                            {
                                Options.DefaultExportDirectory = child.FirstChild.Value;
                            }
                        }
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
                Trace.Assert(referenceList.Count == 1);

                if (referenceList[0].GetType().Name == "ZoneCategory")
                {
                    if (ZoneCategoryChanged != null)
                    {
                        ZoneCategoryChanged(this, (IZoneCategory)referenceList[0]);
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
        }

        public string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(3); }
        }

        public void WriteOptions(XmlDocument xmlDoc, XmlElement pluginNode)
        {
            XmlNode child;

            // HR
            child = xmlDoc.CreateElement("UseSTHeartRateZones");
            child.AppendChild(xmlDoc.CreateTextNode(Options.UseSportTracksHeartRateZones.ToString()));
            pluginNode.AppendChild(child);

            // Speed
            child = xmlDoc.CreateElement("UseSTSpeedZones");
            child.AppendChild(xmlDoc.CreateTextNode(Options.UseSportTracksSpeedZones.ToString()));
            pluginNode.AppendChild(child);

            // Power
            child = xmlDoc.CreateElement("UseSTPowerZones");
            child.AppendChild(xmlDoc.CreateTextNode(Options.UseSportTracksPowerZones.ToString()));
            pluginNode.AppendChild(child);

            // Default export directory
            child = xmlDoc.CreateElement("DefaultExportDirectory");
            child.AppendChild(xmlDoc.CreateTextNode(Options.DefaultExportDirectory));
            pluginNode.AppendChild(child);

            // SplitPanel sizes
            child = xmlDoc.CreateElement("CategoriesSplitDistance");
            child.AppendChild(xmlDoc.CreateTextNode(Options.CategoriesPanelSplitSize.ToString()));
            pluginNode.AppendChild(child);

            child = xmlDoc.CreateElement("WorkoutSplitDistance");
            child.AppendChild(xmlDoc.CreateTextNode(Options.WorkoutPanelSplitSize.ToString()));
            pluginNode.AppendChild(child);

            child = xmlDoc.CreateElement("StepSplitDistance");
            child.AppendChild(xmlDoc.CreateTextNode(Options.StepPanelSplitSize.ToString()));
            pluginNode.AppendChild(child);

            child = xmlDoc.CreateElement("CalendarSplitDistance");
            child.AppendChild(xmlDoc.CreateTextNode(Options.CalendarPanelSplitSize.ToString()));
            pluginNode.AppendChild(child);

            // Default export directory
            child = xmlDoc.CreateElement("DefaultExportDirectory");
            child.AppendChild(xmlDoc.CreateTextNode(Options.DefaultExportDirectory));
            pluginNode.AppendChild(child);
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
                    WorkoutManager.Instance.Deserialize(stream);
                }
                catch (Data.DataTooRecentException)
                {
                    MessageBox.Show("Cannot open this data with this version of the GarminWorkoutPlugin, please update to the latest version",
                                    "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                WorkoutManager.Instance.Workouts.Clear();
            }

            stream.Close();
        }

        public delegate void LogbookChangedEventHandler(object sender, ILogbook oldLogbook, ILogbook newLogbook); 
        public static event LogbookChangedEventHandler LogbookChanged;

        public delegate void ZoneCategoryChangedEventHandler(object sender, IZoneCategory category);
        public static event ZoneCategoryChangedEventHandler ZoneCategoryChanged;

        private static ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                Assembly.GetExecutingAssembly());
        private static IApplication m_App = null;
        private static ILogbook m_CurrentLogbook = null;
        XmlNode m_PluginOptions;
    }
}
