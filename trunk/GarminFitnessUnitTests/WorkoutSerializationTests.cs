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
            public SYSTEMTIME standardDate;
            public int standardBias;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string daylightName;
            public SYSTEMTIME daylightDate;
            public int daylightBias;
        }

        #endregion

        public static TimeZoneInformation CurrentTimeZone
        {
            get
            {
                TimeZoneInformation result;

                GetTimeZoneInformation(out result);

                return result;
            }
        }

        public static SYSTEMTIME ByteArrayToSystemTime(Byte[] array, int dataOffset)
        {
            SYSTEMTIME result = new SYSTEMTIME();

            result.wYear = BitConverter.ToInt16(array, dataOffset);
            result.wMonth = BitConverter.ToInt16(array, dataOffset + 2);
            result.wDayOfWeek = BitConverter.ToInt16(array, dataOffset + 4);
            result.wDay = BitConverter.ToInt16(array, dataOffset + 6);
            result.wHour = BitConverter.ToInt16(array, dataOffset + 8);
            result.wMinute = BitConverter.ToInt16(array, dataOffset + 10);
            result.wSecond = BitConverter.ToInt16(array, dataOffset + 12);
            result.wMilliseconds = BitConverter.ToInt16(array, dataOffset + 14);

            return result;
        }

        public static bool SetTimeZone(String timeZoneStdName)
        {
            //ComputerManager.EnableToken("SeTimeZonePrivilege", Process.GetCurrentProcess().Handle);

            // Set local system timezone
            TimeZoneInformation timeZone = Time.GetTimeZones()[timeZoneStdName];

            bool result = SetTimeZoneInformation(ref timeZone);

            RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\TimeZoneInformation", true);
            key.SetValue("TimeZoneKeyName", timeZoneStdName);
            key.SetValue("DynamicDaylightTimeDisabled", 0);

            return result;
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

                TZI.standardName = individualZone.GetValue("MUI_Std").ToString();
                TZI.daylightName = individualZone.GetValue("MUI_Dlt").ToString();

                //read binary TZI data, convert to byte array
                byte[] b = (byte[])individualZone.GetValue("TZI");

                TZI.bias = BitConverter.ToInt32(b, 0);
                TZI.standardBias = BitConverter.ToInt32(b, 4);
                TZI.daylightBias = BitConverter.ToInt32(b, 8);
                TZI.standardDate = ByteArrayToSystemTime(b, 12);
                TZI.daylightDate = ByteArrayToSystemTime(b, 28);

                // Add the name and TZI struct to hash table
                zones.Add(individualZone.GetValue("Std").ToString(), TZI);
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
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout name TCX serialization");

            workout.Name = "WorkoutTest2";
            workout.Serialize(database, "WorkoutTest2", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult2);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout name TCX serialization");

            // Category/Sport
            workout.Name = "WorkoutTest3";
            workout.Category = logbook.ActivityCategories[0].SubCategories[5];
            workout.Serialize(database, "WorkoutTest3", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult3);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid sport TCX serialization");

            workout.Name = "WorkoutTest4";
            workout.Category = logbook.ActivityCategories[0].SubCategories[6];
            workout.Serialize(database, "WorkoutTest4", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult4);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid sport TCX serialization");

            // Scheduled dates
            workout.Name = "WorkoutTest5";
            workout.ScheduleWorkout(DateTime.Now.ToLocalTime());
            workout.Serialize(database, "WorkoutTest5", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult5);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid scheduled dates TCX serialization");

            workout.Name = "WorkoutTest6";
            workout.ScheduleWorkout(DateTime.Now.ToLocalTime().AddDays(1));
            workout.Serialize(database, "WorkoutTest6", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult6);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid multiple scheduled dates TCX serialization");

            // Test past dates should not schedule anything
            workout.Name = "WorkoutTest6b";
            workout.ScheduleWorkout(new DateTime(1999, 12, 31, 1, 0, 0));
            workout.Serialize(database, "WorkoutTest6b", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult6b);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid past scheduled dates TCX serialization");

            // New Zealand time (UST+12H) is a case where we had problems because the offset changes the day
            String currentZoneName = Time.CurrentTimeZone.standardName;
            workout.Name = "WorkoutTest7";
            workout.ScheduledDates.Clear();
            bool res = Time.SetTimeZone("New Zealand Standard Time");
            workout.ScheduleWorkout(DateTime.Now.ToLocalTime());
            workout.Serialize(database, "WorkoutTest7", testDocument);
            res = Time.SetTimeZone(currentZoneName);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult7);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid UST+12 scheduled dates TCX serialization");

            // Notes
            workout.Name = "WorkoutTest8";
            workout.ScheduledDates.Clear();
            workout.Notes = "Notes test 1";
            workout.Serialize(database, "WorkoutTest8", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult8);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout notes TCX serialization");

            workout.Name = "WorkoutTest9";
            workout.ScheduledDates.Clear();
            workout.Notes = "Notes test 2";
            workout.Serialize(database, "WorkoutTest9", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(workoutTestResult9);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid workout notes TCX serialization");
        }

        [Test]
        public void TestTCXDeserialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode readNode;
            XmlNode database;
            XmlAttribute attribute;
            Workout workout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            ILogbook logbook = PluginMain.GetApplication().Logbook;

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);
            readNode = testDocument.CreateElement("TestNode");
            database.AppendChild(readNode);

            // Workout name
            readNode.InnerXml = workoutTestResult1;
            workout.Deserialize(readNode.FirstChild);
            Assert.AreEqual("WorkoutTest1", workout.Name, "Invalid workout name in TCX deserialization");

            readNode.InnerXml = workoutTestResult2;
            workout.Deserialize(readNode.FirstChild);
            Assert.AreEqual("WorkoutTest2", workout.Name, "Invalid workout name in TCX deserialization");

            // Category/Sport
            readNode.InnerXml = workoutTestResult3;
            workout.Deserialize(readNode.FirstChild);
            Assert.AreEqual("WorkoutTest3", workout.Name, "Invalid workout name in TCX deserialization");
            Assert.AreEqual(logbook.ActivityCategories[0].SubCategories[5], workout.Category, "Invalid SportTracks category in TCX deserialization");

            readNode.InnerXml = workoutTestResult4;
            workout.Deserialize(readNode.FirstChild);
            Assert.AreEqual("WorkoutTest4", workout.Name, "Invalid workout name in TCX deserialization");
            Assert.AreEqual(logbook.ActivityCategories[0].SubCategories[6], workout.Category, "Invalid SportTracks category in TCX deserialization");

            // Scheduled dates
            readNode.InnerXml = workoutTestResult5;
            workout.Deserialize(readNode.FirstChild);
            Assert.AreEqual("WorkoutTest5", workout.Name, "Invalid workout name in TCX deserialization");
            Assert.AreEqual(1, workout.ScheduledDates.Count, "Invalid number of scheduled dates in TCX deserialization");
            Assert.LessOrEqual(0, (DateTime.Now - workout.ScheduledDates[0]).TotalDays, "Invalid scheduled date in TCX deserialization");

            readNode.InnerXml = workoutTestResult6;
            workout.Deserialize(readNode.FirstChild);
            Assert.AreEqual("WorkoutTest6", workout.Name, "Invalid workout name in TCX deserialization");
            Assert.AreEqual(2, workout.ScheduledDates.Count, "Invalid number of scheduled dates in TCX deserialization");
            Assert.LessOrEqual(0, (DateTime.Now - workout.ScheduledDates[0]).TotalDays, "Invalid scheduled date in TCX deserialization");
            Assert.LessOrEqual(0, (DateTime.Now.AddDays(1) - workout.ScheduledDates[0]).TotalDays, "Invalid future scheduled date in TCX deserialization");

            // New Zealand time (UST+12H) is a case where we had problems because the offset changes the day
            String currentZoneName = Time.CurrentTimeZone.standardName;
            Time.SetTimeZone("New Zealand Standard Time");
            readNode.InnerXml = workoutTestResult7;
            workout.Deserialize(readNode.FirstChild);
            Time.SetTimeZone(currentZoneName);
            Assert.AreEqual("WorkoutTest7", workout.Name, "Invalid workout name in TCX deserialization");
            Assert.AreEqual(1, workout.ScheduledDates.Count, "Invalid number of scheduled dates in TCX deserialization");
            Assert.LessOrEqual(0, (DateTime.Now - workout.ScheduledDates[0]).TotalDays, "Invalid scheduled date in TCX deserialization");

            readNode.InnerXml = workoutTestResult8;
            workout.Deserialize(readNode.FirstChild);
            Assert.AreEqual("WorkoutTest8", workout.Name, "Invalid workout name in TCX deserialization");
            Assert.AreEqual("Notes test 1", workout.Notes, "Invalid workout notes in TCX deserialization");

            readNode.InnerXml = workoutTestResult9;
            workout.Deserialize(readNode.FirstChild);
            Assert.AreEqual("WorkoutTest9", workout.Name, "Invalid workout name in TCX deserialization");
            Assert.AreEqual("Notes test 2", workout.Notes, "Invalid workout notes in TCX deserialization");

            // Erase old value for successive deserializations
            Assert.AreNotEqual(logbook.ActivityCategories[1], workout.Category, "No SportTracks category TCX deserialization setup failure");
            readNode.InnerXml = workoutTestResult10;
            workout.Category = logbook.ActivityCategories[1];
            workout.Deserialize(readNode.FirstChild);
            Assert.AreEqual("WorkoutTest10", workout.Name, "Invalid workout name in TCX deserialization");
            Assert.AreEqual(logbook.ActivityCategories[1], workout.Category, "No SportTracks category TCX deserialization won't request category");
            Assert.IsTrue(String.IsNullOrEmpty(workout.Notes), "TCXDeserialization without notes didn't erase old notes");
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
        const String workoutTestResult10 = "<WorkoutTest10 Sport=\"Biking\"><Name>WorkoutTest10</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator></WorkoutTest10>";
    }
}
