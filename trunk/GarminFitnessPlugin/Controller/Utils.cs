using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Text;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class Utils
    {
        public static void HijackMainWindow()
        {
            Control viewControl = PluginMain.GetApplication().ActiveView.CreatePageControl();
            Control mainWindow = viewControl.TopLevelControl;

            for (int i = 0; i < mainWindow.Controls.Count; ++i)
            {
                mainWindow.Controls[i].Enabled = false;
            }
            mainWindow.Cursor = Cursors.WaitCursor;
        }

        public static void ReleaseMainWindow()
        {
            Control viewControl = PluginMain.GetApplication().ActiveView.CreatePageControl();
            Control mainWindow = viewControl.TopLevelControl;

            for (int i = 0; i < mainWindow.Controls.Count; ++i)
            {
                mainWindow.Controls[i].Enabled = true;
            }
            mainWindow.Cursor = Cursors.Default;
        }

        public static IActivityCategory FindCategoryByID(string categoryID)
        {
            return FindCategoryByIDInList(categoryID, PluginMain.GetApplication().Logbook.ActivityCategories);
        }

        public static IActivityCategory FindCategoryByIDSafe(string categoryID)
        {
            IActivityCategory category = FindCategoryByID(categoryID);
            if (category != null)
            {
                return category;
            }

            // We didn't find the old category, place it in the first one
            Trace.Assert(PluginMain.GetApplication().Logbook.ActivityCategories.Count > 0);
            return GetDefaultCategory();
        }

        private static IActivityCategory FindCategoryByIDInList(string categoryID, IList<IActivityCategory> list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].ReferenceId == categoryID)
                {
                    return list[i];
                }
                else if (list[i].SubCategories.Count > 0)
                {
                    IActivityCategory child = FindCategoryByIDInList(categoryID, list[i].SubCategories);
                    if (child != null)
                    {
                        return child;
                    }
                }
            }

            return null;
        }

        public static IZoneCategory FindZoneCategoryByID(IList<IZoneCategory> list, string categoryID)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].ReferenceId == categoryID)
                {
                    return list[i];
                }
            }

            return list[0];
        }

        public static bool NamedZoneStillExists(IList<IZoneCategory> list, INamedLowHighZone zone)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                IZoneCategory currentZoneCategory = list[i];

                for (int j = 0; j < currentZoneCategory.Zones.Count; ++j)
                {
                    if (currentZoneCategory.Zones[j] == zone)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool ZoneCategoryStillExists(IList<IZoneCategory> list, IZoneCategory zone)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] == zone)
                {
                    return true;
                }
            }

            return false;
        }

        public static IActivityCategory GetDefaultCategory()
        {
            return PluginMain.GetApplication().Logbook.ActivityCategories[0];
        }

        public static void SaveWorkoutsToLogbook()
        {
            // Save Workouts to logbook
            MemoryStream stream = new MemoryStream();

            // Data version number
            stream.Write(Encoding.UTF8.GetBytes(Constants.DataHeaderIdString), 0, Encoding.UTF8.GetByteCount(Constants.DataHeaderIdString));
            stream.WriteByte(Constants.CurrentVersion.VersionNumber);

            Options.Instance.Serialize(stream);
            GarminWorkoutManager.Instance.Serialize(stream);
            GarminProfileManager.Instance.Serialize(stream);

            PluginMain.GetApplication().Logbook.SetExtensionData(GUIDs.PluginMain, stream.GetBuffer());
            PluginMain.GetApplication().Logbook.Modified = true;
            stream.Close();
        }

        public static bool IsTextIntegerInRange(string text, UInt16 minRange, UInt16 maxRange)
        {
            UInt16 value;

            if (UInt16.TryParse(text, out value))
            {
                if (value >= minRange && value <= maxRange)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsTextInteger(string text)
        {
            UInt64 value;

            return UInt64.TryParse(text, out value);
        }

        public static bool IsTextFloatInRange(string text, double minRange, double maxRange)
        {
            double value;

            if (double.TryParse(text, out value))
            {
                if (value >= minRange && value <= maxRange)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsTextTimeInRange(string text, double minRange, double maxRange)
        {
            try
            {
                float value = TimeToFloat(text);

                if (value < minRange || value > maxRange)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static float TimeToFloat(string time)
        {
            if (time.IndexOf(':') != -1)
            {
                string minutes = time.Substring(0, time.IndexOf(':'));
                string seconds = time.Substring(time.IndexOf(':') + 1);

                float value = 0;

                if (minutes != String.Empty)
                {
                    value += float.Parse(minutes);
                }
                value += float.Parse(seconds) / Constants.SecondsPerMinute;

                return value;
            }
            else
            {
                // We only have minutes
                return float.Parse(time);
            }
        }

        public static string DoubleToTimeString(double time)
        {
            UInt16 min, sec;

            DoubleToTime(time, out min, out sec);

            return String.Format("{0:00}:{1:00}", min, sec);
        }

        public static void DoubleToTime(double time, out UInt16 minutes, out UInt16 seconds)
        {
            minutes = (UInt16)(time);
            seconds = (UInt16)Math.Round((time - (UInt16)time) * Constants.SecondsPerMinute, MidpointRounding.AwayFromZero);

            if(seconds == Constants.SecondsPerMinute)
            {
                minutes++;
                seconds = 0;
            }
        }

        public static double SpeedToPace(double speed)
        {
            return Constants.MinutesPerHour / speed;
        }

        public static double PaceToSpeed(double pace)
        {
            return Constants.MinutesPerHour / pace;
        }

        public static int FindIndexForZone(IList<INamedLowHighZone> list, INamedLowHighZone zone)
        {
            Trace.Assert(list.Count > 0);

            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].Name == zone.Name)
                {
                    return i;
                }
            }

            return 0;
        }

        public static int FindIndexForZoneCategory(IList<IZoneCategory> list, IZoneCategory zone)
        {
            Trace.Assert(list.Count > 0);

            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] == zone)
                {
                    return i;
                }
            }

            return 0;
        }

        public static int GetStepExportId(IStep step)
        {
            return GetStepExportIdInternal(step.ParentWorkout.Steps, step);
        }

        private static int GetStepExportIdInternal(IList<IStep> steps, IStep step)
        {
            int currentId = 0;

            for (int i = 0; i < steps.Count; ++i)
            {
                IStep currentStep = steps[i];

                if (currentStep == step)
                {
                    return currentId + currentStep.GetStepCount();
                }
                else if (currentStep.Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)currentStep;
                    int temp = GetStepExportIdInternal(concreteStep.StepsToRepeat, step);

                    if (temp != -1)
                    {
                        return currentId + temp;
                    }
                }

                currentId += currentStep.GetStepCount();
            }

            return -1;
        }

        public static bool GetStepInfo(IStep step, List<IStep> referenceList, out List<IStep> owningList, out UInt16 index)
        {
            owningList = null;
            index = 0;

            for (UInt16 i = 0; i < referenceList.Count; ++i)
            {
                IStep currentStep = referenceList[i];

                if (currentStep == step)
                {
                    owningList = referenceList;
                    index = i;
                    return true;
                }
                else if (currentStep.Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)currentStep;

                    if (GetStepInfo(step, concreteStep.StepsToRepeat, out owningList, out index))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string GetWorkoutFilename(Workout workout)
        {
            string fileName = workout.Name;

            // Replace invalid characters by underscores
            fileName = fileName.Replace('\\', '_');
            fileName = fileName.Replace('/', '_');
            fileName = fileName.Replace(':', '_');
            fileName = fileName.Replace('*', '_');
            fileName = fileName.Replace('?', '_');
            fileName = fileName.Replace('"', '_');
            fileName = fileName.Replace('<', '_');
            fileName = fileName.Replace('>', '_');
            fileName = fileName.Replace('|', '_');
            fileName += ".tcx";

            return fileName;
        }

        public static double Clamp(double value, double min, double max)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        public static bool IsMetric(Length.Units unit)
        {
            return (int)unit <= (int)Length.Units.Kilometer;
        }

        public static bool IsStatute(Length.Units unit)
        {
            return !IsMetric(unit);
        }
    }
}
