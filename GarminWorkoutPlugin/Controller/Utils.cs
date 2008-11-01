using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class Utils
    {
        public static void HijackMainWindow()
        {
            Control viewControl = PluginMain.GetApplication().ActiveView.CreatePageControl();
            Control mainWindow = viewControl.Parent.Parent.Parent.Parent;

            for (int i = 0; i < mainWindow.Controls.Count; ++i)
            {
                mainWindow.Controls[i].Enabled = false;
            }
            mainWindow.Cursor = Cursors.WaitCursor;
        }

        public static void ReleaseMainWindow()
        {
            Control viewControl = PluginMain.GetApplication().ActiveView.CreatePageControl();
            Control mainWindow = viewControl.Parent.Parent.Parent.Parent;

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

            GarminWorkoutManager.Instance.Serialize(stream);
            GarminProfileManager.Instance.Serialize(stream);

            PluginMain.GetApplication().Logbook.SetExtensionData(GUIDs.PluginMain, stream.GetBuffer());
            PluginMain.GetApplication().Logbook.Modified = true;
            stream.Close();
        }

        public static bool IsTextIntegerInRange(string text, UInt16 minRange, UInt16 maxRange)
        {
            try
            {
                UInt16 value = UInt16.Parse(text);

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

        public static bool IsTextInteger(string text)
        {
            try
            {
                UInt64 value = UInt64.Parse(text);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsTextFloatInRange(string text, double minRange, double maxRange)
        {
            try
            {
                float value = float.Parse(text);

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

                float value = float.Parse(minutes);
                value += float.Parse(seconds) / 60.0f;

                return value;
            }
            else
            {
                // We only have minutes
                return float.Parse(time);
            }
        }

        public static void FloatToTime(double time, out UInt16 minutes, out UInt16 seconds)
        {
            minutes = (UInt16)(time);
            seconds = (UInt16)Math.Round((time - (UInt16)time) * Constants.SecondsPerMinute, MidpointRounding.AwayFromZero);

            if(seconds == 60)
            {
                minutes++;
                seconds = 0;
            }
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
    }
}
