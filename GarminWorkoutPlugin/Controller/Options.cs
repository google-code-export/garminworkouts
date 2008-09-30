using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminWorkoutPlugin.Data;

namespace GarminWorkoutPlugin.Controller
{
    class Options
    {
        public static void ValidateAfterZoneCategoryChanged(IZoneCategory categoryChanged)
        {
            if(!Utils.ZoneCategoryStillExists(PluginMain.GetApplication().Logbook.CadenceZones, m_CadenceZoneCategory))
            {
                m_CadenceZoneCategory = PluginMain.GetApplication().Logbook.CadenceZones[0];
                IsCadenceZoneDirty = true;
            }

            if(!Utils.ZoneCategoryStillExists(PluginMain.GetApplication().Logbook.PowerZones, m_PowerZoneCategory))
            {
                m_PowerZoneCategory = PluginMain.GetApplication().Logbook.PowerZones[0];
                IsPowerZoneDirty = true;
            }
        }

        public static int CategoriesPanelSplitSize
        {
            get { return m_CategoriesPanelSplitSize; }
            set { m_CategoriesPanelSplitSize = value;  }
        }

        public static int WorkoutPanelSplitSize
        {
            get { return m_WorkoutPanelSplitSize; }
            set { m_WorkoutPanelSplitSize = value; }
        }

        public static int StepPanelSplitSize
        {
            get { return m_StepPanelSplitSize; }
            set { m_StepPanelSplitSize = value; }
        }

        public static int CalendarPanelSplitSize
        {
            get { return m_CalendarPanelSplitSize; }
            set { m_CalendarPanelSplitSize = value; }
        }

        public static bool UseSportTracksHeartRateZones
        {
            get { return m_UseSportTracksHeartRateZones; }
            set { m_UseSportTracksHeartRateZones = value; }
        }

        public static bool UseSportTracksSpeedZones
        {
            get { return m_UseSportTracksSpeedZones; }
            set { m_UseSportTracksSpeedZones = value; }
        }

        public static bool UseSportTracksPowerZones
        {
            get { return m_UseSportTracksPowerZones; }
            set { m_UseSportTracksPowerZones = value; }
        }

        public static IZoneCategory CadenceZoneCategory
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
                        WorkoutManager.Instance.MarkAllCadenceSTZoneTargetsAsDirty();
                    }
                }
            }
        }

        public static IZoneCategory PowerZoneCategory
        {
            get { return m_PowerZoneCategory; }
            set
            {
                if (m_PowerZoneCategory != value)
                {
                    m_PowerZoneCategory = value;
                    IsPowerZoneDirty = false;

                    if (m_CadenceZoneCategory != null)
                    {
                        WorkoutManager.Instance.MarkAllPowerSTZoneTargetsAsDirty();
                    }
                }
            }
        }

        public static String DefaultExportDirectory
        {
            get { return m_DefaultExportDirectory; }
            set { m_DefaultExportDirectory = value; }
        }

        public static bool IsPowerZoneDirty
        {
            get { return m_IsPowerZoneDirty; }
            set { m_IsPowerZoneDirty = value; }
        }

        public static bool IsCadenceZoneDirty
        {
            get { return m_IsCadenceZoneDirty; }
            set { m_IsCadenceZoneDirty = value; }
        }

        private static int m_CategoriesPanelSplitSize = 200;
        private static int m_WorkoutPanelSplitSize = 180;
        private static int m_StepPanelSplitSize = 220;
        private static int m_CalendarPanelSplitSize = 350;

        private static bool m_UseSportTracksHeartRateZones = true;
        private static bool m_UseSportTracksSpeedZones = true;
        private static bool m_UseSportTracksPowerZones = true;
        private static IZoneCategory m_CadenceZoneCategory;
        private static IZoneCategory m_PowerZoneCategory;

        private static String m_DefaultExportDirectory = Assembly.GetCallingAssembly().Location.Substring(0, Assembly.GetCallingAssembly().Location.LastIndexOf('\\'));

        private static bool m_IsPowerZoneDirty = false;
        private static bool m_IsCadenceZoneDirty = false;
    }
}
