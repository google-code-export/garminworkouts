using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.View;

namespace GarminFitnessPlugin.Controller
{
    class Utils
    {
        [DllImport("gdi32.dll")]
        private static extern int BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
                                         IntPtr hdcSrc, int nXSrc, int nYSrc, Int32 dwRop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        static public void RenderBitmapToGraphics(Bitmap source, Graphics destination, Rectangle destinationRect)
        {
            try
            {
                IntPtr destHdc = destination.GetHdc();
                IntPtr bitmapHdc = CreateCompatibleDC(destHdc);
                IntPtr hBitmap = source.GetHbitmap();
                IntPtr oldHdc = SelectObject(bitmapHdc, hBitmap);

                BitBlt(destHdc,
                       destinationRect.Left, destinationRect.Top,
                       destinationRect.Width, destinationRect.Height,
                       bitmapHdc, 0, 0, 0x00CC0020);

                destination.ReleaseHdc(destHdc);
                SelectObject(bitmapHdc, oldHdc);
                DeleteDC(bitmapHdc);
                DeleteObject(hBitmap);
            }
            catch//(System.DllNotFoundException e)
            {
                // Mono/Linux - Just go on right now
                //throw e;
            }
        }

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
            Debug.Assert(PluginMain.GetApplication().Logbook.ActivityCategories.Count > 0);
            return GetDefaultCategory();
        }

        private static IActivityCategory FindCategoryByIDInList(string categoryID, IEnumerable<IActivityCategory> list)
        {
            foreach (IActivityCategory category in list)
            {
                if (category.ReferenceId == categoryID)
                {
                    return category;
                }
                else if (category.SubCategories.Count > 0)
                {
                    IActivityCategory child = FindCategoryByIDInList(categoryID, category.SubCategories);
                    if (child != null)
                    {
                        return child;
                    }
                }
            }

            return null;
        }

        public static IZoneCategory FindZoneCategoryByID(IZoneCategoryList list, string categoryID)
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

        public static bool NamedZoneStillExists(IZoneCategoryList list, INamedLowHighZone zone)
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

        public static bool ZoneCategoryStillExists(IZoneCategoryList list, IZoneCategory zone)
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

        public static void SaveDataToLogbook()
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

        public static bool IsTextIntegerInRange(string text, Int32 minRange, Int32 maxRange)
        {
            Int32 value;

            if (Int32.TryParse(text, out value))
            {
                if (value >= minRange && value <= maxRange)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsTextIntegerInRange(string text, UInt32 minRange, UInt32 maxRange)
        {
            UInt32 value;

            if (UInt32.TryParse(text, out value))
            {
                if (value >= minRange && value <= maxRange)
                {
                    return true;
                }
            }

            return false;
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
            return IsTextFloatInRange(text, minRange, maxRange, GarminFitnessView.UICulture);
        }

        public static bool IsTextFloatInRange(string text, double minRange, double maxRange, CultureInfo culture)
        {
            double value;

            if (double.TryParse(text, NumberStyles.Float, culture.NumberFormat, out value))
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

        public static void DoubleToTime(double timeInMinutes, out UInt16 minutes, out UInt16 seconds)
        {
            minutes = (UInt16)(timeInMinutes);
            seconds = (UInt16)Math.Round((timeInMinutes - (UInt16)timeInMinutes) * Constants.SecondsPerMinute, MidpointRounding.AwayFromZero);

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

        public static void SerializeSTZoneInfoXML(IStep step, IZoneCategory categoryZones, INamedLowHighZone zone, XmlDocument document)
        {
            int index = categoryZones.Zones.IndexOf(zone);

            if (index != -1)
            {
                XmlNode extensionNode;
                XmlNode categoryNode;
                XmlNode valueNode;

                extensionNode = document.CreateElement("TargetOverride");

                // Step Id node
                valueNode = document.CreateElement("StepId");
                extensionNode.AppendChild(valueNode);
                valueNode.AppendChild(document.CreateTextNode(step.ParentWorkout.GetStepExportId(step).ToString()));

                // Category node
                categoryNode = document.CreateElement("Category");
                extensionNode.AppendChild(categoryNode);

                // RefId
                GarminFitnessString categoryRefID = new GarminFitnessString(categoryZones.ReferenceId);
                categoryRefID.Serialize(categoryNode, "Id", document);

                // Zone index
                GarminFitnessInt32Range zoneIndex = new GarminFitnessInt32Range(index);
                zoneIndex.Serialize(categoryNode, "Index", document);

                step.ParentWorkout.AddSportTracksExtension(extensionNode);
            }
        }

        public static string GetWorkoutFilename(IWorkout workout, GarminWorkoutManager.FileFormats format)
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
            fileName = fileName.Replace('.', '_');
            fileName = fileName.Replace('&', '_');
            fileName = fileName.Replace('%', '_');

            if (format == GarminWorkoutManager.FileFormats.TCX)
            {
                fileName += ".tcx";
            }
            else if (format == GarminWorkoutManager.FileFormats.FIT)
            {
                fileName += ".fit";
            }
            else
            {
                Debug.Assert(false);
            }

            return fileName;
        }

        public static FITSports GetFITSport(GarminCategories category)
        {
            if (category == GarminCategories.Biking)
            {
                return FITSports.Cycling;
            }
            else if (category == GarminCategories.Running)
            {
                return FITSports.Running;
            }

            return FITSports.Other;
        }

        public static string GetFITSportName(GarminCategories sport)
        {
            switch(sport)
            {
                case GarminCategories.Running:
                {
                    return "Running";
                }
                case GarminCategories.Biking:
                {
                    return "Cycling";
                }
                default:
                {
                    return "Other";
                }
            }
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
