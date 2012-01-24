using System;
using System.Xml;
using NUnit.Framework;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.Data;

namespace GarminFitnessUnitTests
{
    [TestFixture]
    class StepTargetSerializationTests
    {
        [Test]
        public void TestTCXSerialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep placeholderStep = new RegularStep(placeholderWorkout);
            ILogbook logbook = PluginMain.GetApplication().Logbook;
            bool exportHRAsMax = Options.Instance.ExportSportTracksHeartRateAsPercentMax;

            placeholderWorkout.Steps.AddStepToRoot(placeholderStep);

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);

            // No target
            NullTarget noTarget = new NullTarget(placeholderStep);
            noTarget.Serialize(database, "NoTarget", testDocument);
            int targetPosition1 = testDocument.InnerXml.IndexOf(noTargetResult);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid no target serialization");

            // Cadence targets
            // Cadence range
            // Cadence ST zone
            Assert.Fail("Not implemented");

            // Speed targets
            // Speed range
            // Speed Garmin zone
            // Speed ST zone

            // Heart rate targets
            BaseHeartRateTarget hrTarget = new BaseHeartRateTarget(placeholderStep);

            // HR range
            hrTarget.ConcreteTarget = new HeartRateRangeTarget(130, 170, false, hrTarget);
            hrTarget.Serialize(database, "HRRangeTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(hrRangeTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRange target serialization");

            hrTarget.ConcreteTarget = new HeartRateRangeTarget(100, 190, false, hrTarget);
            hrTarget.Serialize(database, "HRRangeTarget2", testDocument);
            int targetPosition2 = testDocument.InnerXml.IndexOf(hrRangeTargetResult2);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRange target serialization");
            Assert.GreaterOrEqual(targetPosition2, targetPosition1, "HRRange target serialization don't differ");

            hrTarget.ConcreteTarget = new HeartRateRangeTarget(50, 70, true, hrTarget);
            hrTarget.Serialize(database, "HRRangeTarget3", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(hrRangeTargetResult3);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRange %Max target serialization");

            hrTarget.ConcreteTarget = new HeartRateRangeTarget(75, 95, true, hrTarget);
            hrTarget.Serialize(database, "HRRangeTarget4", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(hrRangeTargetResult4);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRange %Max target serialization");
            Assert.GreaterOrEqual(targetPosition2, targetPosition1, "HRRange %Max target serialization don't differ");

            // HR Garmin zone
            hrTarget.ConcreteTarget = new HeartRateZoneGTCTarget(1, hrTarget);
            hrTarget.Serialize(database, "HRGTCTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(hrGTCTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRGTC target serialization");

            hrTarget.ConcreteTarget = new HeartRateZoneGTCTarget(3, hrTarget);
            hrTarget.Serialize(database, "HRGTCTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(hrGTCTargetResult2);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRGTC target serialization");
            Assert.GreaterOrEqual(targetPosition2, targetPosition1, "HRGTC target serialization don't differ");

            // HR ST zone
            Options.Instance.ExportSportTracksHeartRateAsPercentMax = false;
            hrTarget.ConcreteTarget = new HeartRateZoneSTTarget(logbook.HeartRateZones[0].Zones[2], hrTarget);
            hrTarget.Serialize(database, "HRSTTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(hrSTTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRST target serialization");

            hrTarget.ConcreteTarget = new HeartRateZoneSTTarget(logbook.HeartRateZones[0].Zones[4], hrTarget);
            hrTarget.Serialize(database, "HRSTTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(hrSTTargetResult2);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRST target serialization");
            Assert.GreaterOrEqual(targetPosition2, targetPosition1, "HRRST target serialization don't differ");

            // Validate profile max HR
            Assert.AreEqual(190,
                            GarminProfileManager.Instance.UserProfile.GetProfileForActivity(Options.Instance.GetGarminCategory(placeholderWorkout.Category)).MaximumHeartRate,
                            "Invalid athlete max HR");
            Options.Instance.ExportSportTracksHeartRateAsPercentMax = true;
            hrTarget.ConcreteTarget = new HeartRateZoneSTTarget(logbook.HeartRateZones[0].Zones[2], hrTarget);
            hrTarget.Serialize(database, "HRSTTarget3", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(hrSTTargetResult3);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRST %Max target serialization");

            hrTarget.ConcreteTarget = new HeartRateZoneSTTarget(logbook.HeartRateZones[0].Zones[4], hrTarget);
            hrTarget.Serialize(database, "HRSTTarget4", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(hrSTTargetResult4);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRST %Max target serialization");
            Assert.GreaterOrEqual(targetPosition2, targetPosition1, "HRST %Max target serialization don't differ");

            // Power targets
            // Power range
            // Power Garmin zone
            // Power ST zone

            // Make sure to reset options to previous values
            Options.Instance.ExportSportTracksHeartRateAsPercentMax = exportHRAsMax;
        }

        [Test]
        public void TestTCXDeserialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode readNode;
            XmlNode database;
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep placeholderStep = new RegularStep(placeholderWorkout);
            ITarget loadedTarget = null;

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            XmlAttribute attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);
            readNode = testDocument.CreateElement("TestNode");
            database.AppendChild(readNode);

            Assert.Fail("Not implemented");

            // No target

            // Cadence targets
            // Cadence range

            // Speed targets
            // Speed range
            // Speed Garmin zone

            // Heart rate targets
            // HR range
            // HR Garmin zone

            // Power targets
            // Power range
            // Power Garmin zone
        }

        [Test]
        public void TestSerializeSTTargetExtensions()
        {
            Assert.Fail("Not implemented");
            // Cadence ST zone

            // Speed ST zone

            // HR ST zone

            // Power ST zone
        }

        [Test]
        public void TestDeserializeSTTargetExtensions()
        {
            Assert.Fail("Not implemented");
            // Cadence ST zone

            // Speed ST zone

            // HR ST zone

            // Power ST zone
        }

        const String noTargetResult = "<NoTarget xsi:type=\"None_t\" />";
        const String hrRangeTargetResult1 = "<HRRangeTarget1 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>130</Value></Low><High xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>170</Value></High></HeartRateZone></HRRangeTarget1>";
        const String hrRangeTargetResult2 = "<HRRangeTarget2 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>100</Value></Low><High xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>190</Value></High></HeartRateZone></HRRangeTarget2>";
        const String hrRangeTargetResult3 = "<HRRangeTarget3 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>50</Value></Low><High xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>70</Value></High></HeartRateZone></HRRangeTarget3>";
        const String hrRangeTargetResult4 = "<HRRangeTarget4 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>75</Value></Low><High xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>95</Value></High></HeartRateZone></HRRangeTarget4>";
        const String hrGTCTargetResult1 = "<HRGTCTarget1 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"PredefinedHeartRateZone_t\"><Number>1</Number></HeartRateZone></HRGTCTarget1>";
        const String hrGTCTargetResult2 = "<HRGTCTarget2 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"PredefinedHeartRateZone_t\"><Number>3</Number></HeartRateZone></HRGTCTarget2>";
        const String hrSTTargetResult1 = "<HRSTTarget1 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>140</Value></Low><High xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>160</Value></High></HeartRateZone></HRSTTarget1>";
        const String hrSTTargetResult2 = "<HRSTTarget2 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>180</Value></Low><High xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>240</Value></High></HeartRateZone></HRSTTarget2>";
        const String hrSTTargetResult3 = "<HRSTTarget3 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>74</Value></Low><High xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>84</Value></High></HeartRateZone></HRSTTarget3>";
        const String hrSTTargetResult4 = "<HRSTTarget4 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>95</Value></Low><High xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>100</Value></High></HeartRateZone></HRSTTarget4>";

        /*                "Speed_t",
                "Cadence_t",
                "HeartRate_t",
                "Power_t"*/
    }
}
