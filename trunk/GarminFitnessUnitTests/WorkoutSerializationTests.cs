using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Microsoft.Win32;
using NUnit.Framework;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessUnitTests
{
    // Class needed for the test regarding time zones below (taken from http://social.msdn.microsoft.com/forums/en-US/csharpgeneral/thread/fa563f41-344c-465d-bfbe-1859299d2491/)
    class Time
    {
        #region DLL Imports

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetTimeZoneInformation(out TimeZoneInformation lpTimeZoneInformation);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetTimeZoneInformation(ref TimeZoneInformation lpTimeZoneInformation);

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TimeZoneInformation
        {
            public int bias;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string standardName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string daylightName;

            SYSTEMTIME standardDate;
            SYSTEMTIME daylightDate;
            public int standardBias;
            public int daylightBias;
        }

        #endregion

        public static TimeZone CurrentTimeZone
        {
            get { return TimeZone.CurrentTimeZone; }
        }

        public static bool SetTimeZone(TimeZoneInformation timeZone)
        {
            //ComputerManager.EnableToken("SeTimeZonePrivilege", Process.GetCurrentProcess().Handle);

            // Set local system timezone
            return SetTimeZoneInformation(ref timeZone);
        }

        public static Dictionary<String, TimeZoneInformation> GetTimeZones()
        {
            //open key where all time zones are located in the registry
            RegistryKey timeZoneKeys = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones");

            //create a new hashtable which will store the name
            //of the timezone and the associate time zone information struct
            Dictionary<string, TimeZoneInformation> zones = new Dictionary<String, TimeZoneInformation>();

            //iterate through each time zone in the registry and add it to the hash table
            foreach (string zonekey in timeZoneKeys.GetSubKeyNames())
            {
                //get current time zone key
                RegistryKey individualZone = timeZoneKeys.OpenSubKey(zonekey);

                //create new TZI struct and populate it with values from key
                TimeZoneInformation TZI = new TimeZoneInformation();

                TZI.standardName = individualZone.GetValue("Std").ToString();
                TZI.daylightName = individualZone.GetValue("Dlt").ToString();

                //read binary TZI data, convert to byte array
                byte[] b = (byte[])individualZone.GetValue("TZI");

                TZI.bias = BitConverter.ToInt32(b, 0);

                //add the name and TZI struct to hash table
                zones.Add(TZI.standardName, TZI);
            }

            return zones;
        }
    }

    [TestFixture]
    class WorkoutSerializationTests
    {
        [Test]
        public void TestTCXSerialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;
            ILogbook logbook = PluginMain.GetApplication().Logbook;
            Workout workout = new Workout("Test", logbook.ActivityCategories[0]);
            RegularStep step = workout.Steps[0] as RegularStep;
            int resultPosition;

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);

            // Name
            workout.Name = "WorkoutTest1";
            workout.Serialize(database, "WorkoutTest1", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult1);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout TCX serialization for active step");

            workout.Name = "WorkoutTest2";
            workout.Serialize(database, "WorkoutTest2", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult2);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout TCX serialization for active step");

            // Category/Sport
            workout.Name = "WorkoutTest3";
            workout.Category = logbook.ActivityCategories[0].SubCategories[5];
            workout.Serialize(database, "WorkoutTest3", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult3);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout TCX serialization for active step");

            workout.Name = "WorkoutTest4";
            workout.Category = logbook.ActivityCategories[0].SubCategories[6];
            workout.Serialize(database, "WorkoutTest4", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult4);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout TCX serialization for active step");

            // Scheduled dates
            workout.Name = "WorkoutTest5";
            workout.ScheduleWorkout(DateTime.Now.ToLocalTime());
            workout.Serialize(database, "WorkoutTest5", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult5);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout TCX serialization for current scheduled dates");

            workout.Name = "WorkoutTest6";
            workout.ScheduleWorkout(DateTime.Now.ToLocalTime().AddDays(1));
            workout.Serialize(database, "WorkoutTest6", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult6);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout TCX serialization for multiple scheduled dates");

            // Test past dates should not schedule anything
            workout.Name = "WorkoutTest6b";
            workout.ScheduleWorkout(new DateTime(1999, 12, 31, 1, 0, 0));
            workout.Serialize(database, "WorkoutTest6b", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult6b);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout TCX serialization for past scheduled dates");

            // New Zealand time (UST+12H) is a case where we had problems because the offset changes the day
            String currentZoneName = Time.CurrentTimeZone.StandardName;
            workout.Name = "WorkoutTest7";
            workout.ScheduledDates.Clear();
            Time.SetTimeZone(Time.GetTimeZones()["New Zealand Standard Time"]);
            workout.ScheduleWorkout(DateTime.Now.ToLocalTime());
            workout.Serialize(database, "WorkoutTest7", testDocument);
            Time.SetTimeZone(Time.GetTimeZones()[currentZoneName]);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult7);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout TCX serialization for UST+12 scheduled dates");

            // Notes
            workout.Name = "WorkoutTest8";
            workout.ScheduledDates.Clear();
            workout.Notes = "Notes test 1";
            workout.Serialize(database, "WorkoutTest8", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult8);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout TCX serialization for notes");

            workout.Name = "WorkoutTest9";
            workout.ScheduledDates.Clear();
            workout.Notes = "Notes test 2";
            workout.Serialize(database, "WorkoutTest9", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult9);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout TCX serialization for notes");
        }

        [Test]
        public void TestTCXDeserialization()
        {
            Assert.Fail("Not implemented");
        }

        [Test]
        public void TestFITSerialization()
        {
            Assert.Fail("Not implemented");
        }

        [Test]
        public void TestFITDeserialization()
        {
            Assert.Fail("Not implemented");
        }

        [Test]
        public void TestMultiPartSerialization()
        {
            Assert.Fail("Not implemented");
        }

        const String workoutTestResult1 = "<WorkoutTest1 Sport=\"Other\"><Name>WorkoutTest1</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>fa756214-cf71-11db-9705-005056c00008</SportTracksCategory><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></WorkoutTest1>";
        const String workoutTestResult2 = "<WorkoutTest2 Sport=\"Other\"><Name>WorkoutTest2</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>fa756214-cf71-11db-9705-005056c00008</SportTracksCategory><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></WorkoutTest2>";
        const String workoutTestResult3 = "<WorkoutTest3 Sport=\"Biking\"><Name>WorkoutTest3</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>e9e39447-e1f1-422f-ae5d-eedf021a84a2</SportTracksCategory><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></WorkoutTest3>";
        const String workoutTestResult4 = "<WorkoutTest4 Sport=\"Running\"><Name>WorkoutTest4</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>662a48c8-29f7-46a0-a488-903f69191a2e</SportTracksCategory><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></WorkoutTest4>";
        readonly String workoutTestResult5 = "<WorkoutTest5 Sport=\"Running\"><Name>WorkoutTest5</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><ScheduledOn>" + DateTime.Now.ToString("yyyy-MM-dd") + "</ScheduledOn><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>662a48c8-29f7-46a0-a488-903f69191a2e</SportTracksCategory><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></WorkoutTest5>";
        readonly String workoutTestResult6 = "<WorkoutTest6 Sport=\"Running\"><Name>WorkoutTest6</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><ScheduledOn>" + DateTime.Now.ToString("yyyy-MM-dd") + "</ScheduledOn><ScheduledOn>" + DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + "</ScheduledOn><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>662a48c8-29f7-46a0-a488-903f69191a2e</SportTracksCategory><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></WorkoutTest6>";
        readonly String workoutTestResult6b = "<WorkoutTest6b Sport=\"Running\"><Name>WorkoutTest6b</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><ScheduledOn>" + DateTime.Now.ToString("yyyy-MM-dd") + "</ScheduledOn><ScheduledOn>" + DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + "</ScheduledOn><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>662a48c8-29f7-46a0-a488-903f69191a2e</SportTracksCategory><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></WorkoutTest6b>";
        readonly String workoutTestResult7 = "<WorkoutTest7 Sport=\"Running\"><Name>WorkoutTest7</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><ScheduledOn>" + DateTime.Now.ToString("yyyy-MM-dd") + "</ScheduledOn><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>662a48c8-29f7-46a0-a488-903f69191a2e</SportTracksCategory><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></WorkoutTest7>";
        const String workoutTestResult8 = "<WorkoutTest8 Sport=\"Running\"><Name>WorkoutTest8</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><Notes>Notes test 1</Notes><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>662a48c8-29f7-46a0-a488-903f69191a2e</SportTracksCategory><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></WorkoutTest8>";
        const String workoutTestResult9 = "<WorkoutTest9 Sport=\"Running\"><Name>WorkoutTest9</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><Notes>Notes test 2</Notes><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>662a48c8-29f7-46a0-a488-903f69191a2e</SportTracksCategory><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></WorkoutTest9>";
    }
}
