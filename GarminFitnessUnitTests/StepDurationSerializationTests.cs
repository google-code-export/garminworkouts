using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NUnit.Framework;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.Controller;
using System.IO;

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

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
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
            Assert.AreNotEqual(durationPosition1, durationPosition2, "Time durations serialization don't differ");

            // Distance
            DistanceDuration distanceDuration = new DistanceDuration(1, Length.Units.Kilometer, placeholderStep);
            distanceDuration.Serialize(database, "DistanceDuration1", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(distanceDurationResult1);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid distance duration serialization");

            distanceDuration = new DistanceDuration(1, Length.Units.Mile, placeholderStep);
            distanceDuration.Serialize(database, "DistanceDuration2", testDocument);
            durationPosition2 = testDocument.InnerXml.IndexOf(distanceDurationResult2);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid distance duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "Distance durations serialization don't differ");

            // Calories
            CaloriesDuration caloriesDuration = new CaloriesDuration(550, placeholderStep);
            caloriesDuration.Serialize(database, "CaloriesDuration1", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(caloriesDurationResult1);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid calories duration serialization");

            caloriesDuration.CaloriesToSpend = 100;
            caloriesDuration.Serialize(database, "CaloriesDuration2", testDocument);
            durationPosition2 = testDocument.InnerXml.IndexOf(caloriesDurationResult2);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid calories duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "Calories durations serialization don't differ");

            // HR above
            HeartRateAboveDuration hrAboveDuration = new HeartRateAboveDuration(160, false, placeholderStep);
            hrAboveDuration.Serialize(database, "HRAboveDuration1", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(hrAboveDurationResult1);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid HRAbove duration serialization");

            hrAboveDuration.MaxHeartRate = 130;
            hrAboveDuration.Serialize(database, "HRAboveDuration2", testDocument);
            durationPosition2 = testDocument.InnerXml.IndexOf(hrAboveDurationResult2);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid HRAbove duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "HRAbove durations serialization don't differ");

            hrAboveDuration.IsPercentageMaxHeartRate = true;
            hrAboveDuration.MaxHeartRate = 50;
            hrAboveDuration.Serialize(database, "HRAboveDuration3", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(hrAboveDurationResult3);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid HRAbove %Max duration serialization");

            hrAboveDuration.MaxHeartRate = 70;
            hrAboveDuration.Serialize(database, "HRAboveDuration4", testDocument);
            durationPosition2 = testDocument.InnerXml.IndexOf(hrAboveDurationResult4);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid HRAbove duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "HRAbove %Max durations serialization don't differ");

            // HR Below
            HeartRateBelowDuration hrBelowDuration = new HeartRateBelowDuration(160, false, placeholderStep);
            hrBelowDuration.Serialize(database, "HRBelowDuration1", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(hrBelowDurationResult1);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid HRBelow duration serialization");

            hrBelowDuration.MinHeartRate = 130;
            hrBelowDuration.Serialize(database, "HRBelowDuration2", testDocument);
            durationPosition2 = testDocument.InnerXml.IndexOf(hrBelowDurationResult2);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid HRBelow duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "HRBelow durations serialization don't differ");

            hrBelowDuration.IsPercentageMaxHeartRate = true;
            hrBelowDuration.MinHeartRate = 50;
            hrBelowDuration.Serialize(database, "HRBelowDuration3", testDocument);
            durationPosition1 = testDocument.InnerXml.IndexOf(hrBelowDurationResult3);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid HRBelow %Max duration serialization");

            hrBelowDuration.MinHeartRate = 70;
            hrBelowDuration.Serialize(database, "HRBelowDuration4", testDocument);
            durationPosition2 = testDocument.InnerXml.IndexOf(hrBelowDurationResult4);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid HRBelow duration serialization");
            Assert.AreNotEqual(durationPosition1, durationPosition2, "HRBelow %Max durations serialization don't differ");

            // Power above
            try
            {
                PowerAboveDuration powerAboveDuration = new PowerAboveDuration(160, false, placeholderStep);
                powerAboveDuration.Serialize(database, "PowerAboveDuration1", testDocument);
                Assert.Fail("PowerAbove duration was serialized in TCX");
            }
            catch (NotSupportedException)
            {
            }

            // Power Below
            try
            {
                PowerBelowDuration powerBelowDuration = new PowerBelowDuration(160, false, placeholderStep);
                powerBelowDuration.Serialize(database, "PowerBelowDuration1", testDocument);
                Assert.Fail("PowerBelow duration was serialized in TCX");
            }
            catch (NotSupportedException)
            {
            }
        }

        [Test]
        public void TestTCXDeserialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode readNode;
            XmlNode database;
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep placeholderStep = new RegularStep(placeholderWorkout);
            IDuration loadedDuration = null;

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            XmlAttribute attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);
            readNode = testDocument.CreateElement("TestNode");
            database.AppendChild(readNode);

            // Lap button
            readNode.InnerXml = lapDurationResult;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "Lap duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is LapButtonDuration, "Lap duration wasn't deserialized as proper type");

            // Time
            readNode.InnerXml = timeDurationResult1;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "Time duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is TimeDuration, "Time duration wasn't deserialized as proper type");
            TimeDuration timeDuration = loadedDuration as TimeDuration;
            Assert.AreEqual(500, timeDuration.TimeInSeconds, "Time duration didn't deserialize the proper time");

            readNode.InnerXml = timeDurationResult2;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "Time duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is TimeDuration, "Time duration wasn't deserialized as proper type");
            timeDuration = loadedDuration as TimeDuration;
            Assert.AreEqual(300, timeDuration.TimeInSeconds, "Time duration didn't deserialize the proper time");

            // Distance
            readNode.InnerXml = distanceDurationResult1;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "Distance duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is DistanceDuration, "Distance duration wasn't deserialized as proper type");
            DistanceDuration distanceDuration = loadedDuration as DistanceDuration;
            Assert.AreEqual(1,
                            Length.Convert(distanceDuration.GetDistanceInBaseUnit(), distanceDuration.BaseUnit, Length.Units.Kilometer),
                            "Distance duration didn't deserialize the proper distance");

            readNode.InnerXml = distanceDurationResult2;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "Distance duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is DistanceDuration, "Distance duration wasn't deserialized as proper type");
            distanceDuration = loadedDuration as DistanceDuration;
            Assert.AreEqual(1,
                            Length.Convert(distanceDuration.GetDistanceInBaseUnit(), distanceDuration.BaseUnit, Length.Units.Mile),
                            0.5,
                            "Distance duration didn't deserialize the proper distance");

            // Calories
            readNode.InnerXml = caloriesDurationResult1;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "Calories duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is CaloriesDuration, "Calories duration wasn't deserialized as proper type");
            CaloriesDuration caloriesDuration = loadedDuration as CaloriesDuration;
            Assert.AreEqual(550, caloriesDuration.CaloriesToSpend, "Calories duration didn't deserialize the proper calories");

            readNode.InnerXml = caloriesDurationResult2;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "Calories duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is CaloriesDuration, "Calories duration wasn't deserialized as proper type");
            caloriesDuration = loadedDuration as CaloriesDuration;
            Assert.AreEqual(100, caloriesDuration.CaloriesToSpend, "Calories duration didn't deserialize the proper calories");

            // HR above
            readNode.InnerXml = hrAboveDurationResult1;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "HRAbove duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is HeartRateAboveDuration, "HRAbove duration wasn't deserialized as proper type");
            HeartRateAboveDuration hrAboveDuration = loadedDuration as HeartRateAboveDuration;
            Assert.IsFalse(hrAboveDuration.IsPercentageMaxHeartRate, "HRAbove duration didn't deserialize the proper hr %Max");
            Assert.AreEqual(160, hrAboveDuration.MaxHeartRate, "HRAbove duration didn't deserialize the proper hr");

            readNode.InnerXml = hrAboveDurationResult2;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "HRAbove duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is HeartRateAboveDuration, "HRAbove duration wasn't deserialized as proper type");
            hrAboveDuration = loadedDuration as HeartRateAboveDuration;
            Assert.IsFalse(hrAboveDuration.IsPercentageMaxHeartRate, "HRAbove duration didn't deserialize the proper hr %Max");
            Assert.AreEqual(130, hrAboveDuration.MaxHeartRate, "HRAbove duration didn't deserialize the proper hr");

            readNode.InnerXml = hrAboveDurationResult3;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "HRAbove duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is HeartRateAboveDuration, "HRAbove duration wasn't deserialized as proper type");
            hrAboveDuration = loadedDuration as HeartRateAboveDuration;
            Assert.IsTrue(hrAboveDuration.IsPercentageMaxHeartRate, "HRAbove duration didn't deserialize the proper hr %Max");
            Assert.AreEqual(50, hrAboveDuration.MaxHeartRate, "HRAbove duration didn't deserialize the proper hr");

            readNode.InnerXml = hrAboveDurationResult4;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "HRAbove duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is HeartRateAboveDuration, "HRAbove duration wasn't deserialized as proper type");
            hrAboveDuration = loadedDuration as HeartRateAboveDuration;
            Assert.IsTrue(hrAboveDuration.IsPercentageMaxHeartRate, "HRAbove duration didn't deserialize the proper hr %Max");
            Assert.AreEqual(70, hrAboveDuration.MaxHeartRate, "HRAbove duration didn't deserialize the proper hr");

            // HR Below
            readNode.InnerXml = hrBelowDurationResult1;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "HRBelow duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is HeartRateBelowDuration, "HRBelow duration wasn't deserialized as proper type");
            HeartRateBelowDuration hrBelowDuration = loadedDuration as HeartRateBelowDuration;
            Assert.IsFalse(hrBelowDuration.IsPercentageMaxHeartRate, "HRBelow duration didn't deserialize the proper hr %Max");
            Assert.AreEqual(160, hrBelowDuration.MinHeartRate, "HRBelow duration didn't deserialize the proper hr");

            readNode.InnerXml = hrBelowDurationResult2;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "HRBelow duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is HeartRateBelowDuration, "HRBelow duration wasn't deserialized as proper type");
            hrBelowDuration = loadedDuration as HeartRateBelowDuration;
            Assert.IsFalse(hrBelowDuration.IsPercentageMaxHeartRate, "HRBelow duration didn't deserialize the proper hr %Max");
            Assert.AreEqual(130, hrBelowDuration.MinHeartRate, "HRBelow duration didn't deserialize the proper hr");

            readNode.InnerXml = hrBelowDurationResult3;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "HRBelow duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is HeartRateBelowDuration, "HRBelow duration wasn't deserialized as proper type");
            hrBelowDuration = loadedDuration as HeartRateBelowDuration;
            Assert.IsTrue(hrBelowDuration.IsPercentageMaxHeartRate, "HRBelow duration didn't deserialize the proper hr %Max");
            Assert.AreEqual(50, hrBelowDuration.MinHeartRate, "HRBelow duration didn't deserialize the proper hr");

            readNode.InnerXml = hrBelowDurationResult4;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedDuration, "HRBelow duration wasn't properly deserialized");
            Assert.IsTrue(loadedDuration is HeartRateBelowDuration, "HRBelow duration wasn't deserialized as proper type");
            hrBelowDuration = loadedDuration as HeartRateBelowDuration;
            Assert.IsTrue(hrBelowDuration.IsPercentageMaxHeartRate, "HRBelow duration didn't deserialize the proper hr %Max");
            Assert.AreEqual(70, hrBelowDuration.MinHeartRate, "HRBelow duration didn't deserialize the proper hr");

            // Invalid duration
            readNode.InnerXml = invalidDuration1;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNull(loadedDuration, "Invalid duration was properly deserialized");

            readNode.InnerXml = invalidDuration2;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNull(loadedDuration, "Empty type duration was properly deserialized");

            readNode.InnerXml = invalidDuration3;
            loadedDuration = DurationFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNull(loadedDuration, "No type duration was properly deserialized");
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
        const String invalidDuration1 = "<Invalid xsi:type=\"Invalid_t\">Test invalid</Invalid>";
        const String invalidDuration2 = "<Invalid xsi:type=\"\">Test empty type</Invalid>";
        const String invalidDuration3 = "<Invalid>Test no type</Invalid>";
    }
}
