using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NUnit.Framework;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin;
using GarminFitnessPlugin.Data;

namespace GarminFitnessUnitTests
{
    [TestFixture]
    class StepDurationSerializationTests
    {
        [Test]
        public void TestTCXSerialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep placeholderStep = new RegularStep(placeholderWorkout);

            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);

            // xmlns:xsi namespace attribute
            attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);

            // Lap button
            LapButtonDuration lapDuration = new LapButtonDuration(placeholderStep);
            lapDuration.Serialize(database, "LapDuration", testDocument);
            int durationPosition1 = testDocument.InnerXml.IndexOf(lapDurationResult);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid lap duration serialization");

            // Time
            TimeDuration timeDuration = new TimeDuration(500, placeholderStep);
            timeDuration.Serialize(database, "TimeDuration1", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(timeDurationResult1);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid time duration serialization");

            timeDuration.TimeInSeconds = 300;
            timeDuration.Serialize(database, "TimeDuration2", testDocument);
            int durationPosition2 = testDocument.InnerXml.IndexOf(timeDurationResult2);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid time duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "Different time durations serialization");

            // Distance
            DistanceDuration distanceDuration = new DistanceDuration(1, Length.Units.Kilometer, placeholderStep);
            distanceDuration.Serialize(database, "DistanceDuration1", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(distanceDurationResult1);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid distance duration serialization");

            distanceDuration = new DistanceDuration(1, Length.Units.Mile, placeholderStep);
            distanceDuration.Serialize(database, "DistanceDuration2", testDocument);
            durationPosition2 = testDocument.InnerXml.IndexOf(distanceDurationResult2);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid distance duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "Different distance durations serialization");

            // Calories
            CaloriesDuration caloriesDuration = new CaloriesDuration(550, placeholderStep);
            caloriesDuration.Serialize(database, "CaloriesDuration1", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(caloriesDurationResult1);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid calories duration serialization");

            caloriesDuration.CaloriesToSpend = 100;
            caloriesDuration.Serialize(database, "CaloriesDuration2", testDocument);
            durationPosition2 = testDocument.InnerXml.IndexOf(caloriesDurationResult2);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid calories duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "Different calories durations serialization");

            // HR above
            HeartRateAboveDuration hrAboveDuration = new HeartRateAboveDuration(160, false, placeholderStep);
            hrAboveDuration.Serialize(database, "HRAboveDuration1", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(hrAboveDurationResult1);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid calories duration serialization");

            hrAboveDuration.MaxHeartRate = 130;
            hrAboveDuration.Serialize(database, "HRAboveDuration2", testDocument);
            durationPosition2 = testDocument.InnerXml.IndexOf(hrAboveDurationResult2);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid HRAbove duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "Different HRAbove durations serialization");

            hrAboveDuration.IsPercentageMaxHeartRate = true;
            hrAboveDuration.MaxHeartRate = 50;
            hrAboveDuration.Serialize(database, "HRAboveDuration3", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(hrAboveDurationResult3);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid HRAbove %Max duration serialization");

            hrAboveDuration.MaxHeartRate = 70;
            hrAboveDuration.Serialize(database, "HRAboveDuration4", testDocument);
            durationPosition2 = testDocument.InnerXml.IndexOf(hrAboveDurationResult4);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid HRAbove duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "Different HRAbove %Max durations serialization");

            // HR Below
            HeartRateBelowDuration hrBelowDuration = new HeartRateBelowDuration(160, false, placeholderStep);
            hrBelowDuration.Serialize(database, "HRBelowDuration1", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(hrBelowDurationResult1);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid calories duration serialization");

            hrBelowDuration.MinHeartRate = 130;
            hrBelowDuration.Serialize(database, "HRBelowDuration2", testDocument);
            durationPosition2 = testDocument.InnerXml.IndexOf(hrBelowDurationResult2);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid HRBelow duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "Different HRBelow durations serialization");

            hrBelowDuration.IsPercentageMaxHeartRate = true;
            hrBelowDuration.MinHeartRate = 50;
            hrBelowDuration.Serialize(database, "HRBelowDuration3", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(hrBelowDurationResult3);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid HRBelow %Max duration serialization");

            hrBelowDuration.MinHeartRate = 70;
            hrBelowDuration.Serialize(database, "HRBelowDuration4", testDocument);
            durationPosition2 = testDocument.InnerXml.IndexOf(hrBelowDurationResult4);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid HRBelow duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "Different HRBelow %Max durations serialization");

            // Power above
            try
            {
                PowerAboveDuration powerAboveDuration = new PowerAboveDuration(160, false, placeholderStep);
                powerAboveDuration.Serialize(database, "PowerAboveDuration1", testDocument);
                Assert.Fail("PowerAbove duration was serialized in TCX");
            }
            catch (NotSupportedException e)
            {
            }

            // Power Below
            try
            {
                PowerBelowDuration powerBelowDuration = new PowerBelowDuration(160, false, placeholderStep);
                powerBelowDuration.Serialize(database, "PowerBelowDuration1", testDocument);
                Assert.Fail("PowerBelow duration was serialized in TCX");
            }
            catch (NotSupportedException e)
            {
            }
        }

        const String lapDurationResult = "<LapDuration xsi:type=\"UserInitiated_t\" />";
        const String timeDurationResult1 = "<TimeDuration1 xsi:type=\"Time_t\"><Seconds>500</Seconds></TimeDuration1>";
        const String timeDurationResult2 = "<TimeDuration2 xsi:type=\"Time_t\"><Seconds>300</Seconds></TimeDuration2>";
        const String distanceDurationResult1 = "<DistanceDuration1 xsi:type=\"Distance_t\"><Meters>1000</Meters></DistanceDuration1>";
        const String distanceDurationResult2 = "<DistanceDuration2 xsi:type=\"Distance_t\"><Meters>1609</Meters></DistanceDuration2>";
        const String caloriesDurationResult1 = "<CaloriesDuration1 xsi:type=\"CaloriesBurned_t\"><Calories>550</Calories></CaloriesDuration1>";
        const String caloriesDurationResult2 = "<CaloriesDuration2 xsi:type=\"CaloriesBurned_t\"><Calories>100</Calories></CaloriesDuration2>";
        const String hrAboveDurationResult1 = "<HRAboveDuration1 xsi:type=\"HeartRateAbove_t\"><HeartRate xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>160</Value></HeartRate></HRAboveDuration1>";
        const String hrAboveDurationResult2 = "<HRAboveDuration2 xsi:type=\"HeartRateAbove_t\"><HeartRate xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>130</Value></HeartRate></HRAboveDuration2>";
        const String hrAboveDurationResult3 = "<HRAboveDuration3 xsi:type=\"HeartRateAbove_t\"><HeartRate xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>50</Value></HeartRate></HRAboveDuration3>";
        const String hrAboveDurationResult4 = "<HRAboveDuration4 xsi:type=\"HeartRateAbove_t\"><HeartRate xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>70</Value></HeartRate></HRAboveDuration4>";
        const String hrBelowDurationResult1 = "<HRBelowDuration1 xsi:type=\"HeartRateBelow_t\"><HeartRate xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>160</Value></HeartRate></HRBelowDuration1>";
        const String hrBelowDurationResult2 = "<HRBelowDuration2 xsi:type=\"HeartRateBelow_t\"><HeartRate xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>130</Value></HeartRate></HRBelowDuration2>";
        const String hrBelowDurationResult3 = "<HRBelowDuration3 xsi:type=\"HeartRateBelow_t\"><HeartRate xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>50</Value></HeartRate></HRBelowDuration3>";
        const String hrBelowDurationResult4 = "<HRBelowDuration4 xsi:type=\"HeartRateBelow_t\"><HeartRate xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>70</Value></HeartRate></HRBelowDuration4>";
    }
}
