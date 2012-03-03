using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessUnitTests
{
    [TestFixture]
    class RepeatDurationSerializationTests
    {
        [Test]
        public void TestTCXSerialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RepeatStep placeholderStep = new RepeatStep(placeholderWorkout);

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);

            // Repeat count
            RepeatCountDuration countDuration = new RepeatCountDuration(placeholderStep);
            placeholderStep.Duration = countDuration;
            countDuration.RepetitionCount = 4;
            countDuration.Serialize(database, "RepeatCountDuration1", testDocument);
            int durationPosition1 = testDocument.InnerXml.IndexOf(repeatCountDurationResult1);
            Assert.GreaterOrEqual(durationPosition1, 0, "Invalid repeat count duration serialization");
            countDuration.RepetitionCount = 10;
            countDuration.Serialize(database, "RepeatCountDuration2", testDocument);
            int durationPosition2 = testDocument.InnerXml.IndexOf(repeatCountDurationResult2);
            Assert.GreaterOrEqual(durationPosition2, 0, "Invalid repeat count duration serialization");
            Assert.Greater(durationPosition2, durationPosition1, "Repeat count durations serialization don't differ");

            // All other repeat durtions should not serialize in TCX
            try
            {
                RepeatUntilCaloriesDuration caloriesDuration = new RepeatUntilCaloriesDuration(placeholderStep);
                caloriesDuration.Serialize(database, "RepeatUntilCaloriesDuration", testDocument);
                Assert.Fail("Repeat until calories should not serialize");
            }
            catch (NotSupportedException)
            {            	
            }

            try
            {
                RepeatUntilDistanceDuration distanceDuration = new RepeatUntilDistanceDuration(placeholderStep);
                distanceDuration.Serialize(database, "RepeatUntilDistanceDuration", testDocument);
                Assert.Fail("Repeat until distance should not serialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilHeartRateAboveDuration hrAboveDuration = new RepeatUntilHeartRateAboveDuration(placeholderStep);
                hrAboveDuration.Serialize(database, "RepeatUntilHRAboveDuration", testDocument);
                Assert.Fail("Repeat until HRAbove should not serialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilHeartRateBelowDuration hrBelowDuration = new RepeatUntilHeartRateBelowDuration(placeholderStep);
                hrBelowDuration.Serialize(database, "RepeatUntilHRBelowDuration", testDocument);
                Assert.Fail("Repeat until HRBelow should not serialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilPowerAboveDuration powerAboveDuration = new RepeatUntilPowerAboveDuration(placeholderStep);
                powerAboveDuration.Serialize(database, "RepeatUntilPowerAboveDuration", testDocument);
                Assert.Fail("Repeat until PowerAbove should not serialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilPowerBelowDuration powerBelowDuration = new RepeatUntilPowerBelowDuration(placeholderStep);
                powerBelowDuration.Serialize(database, "RepeatUntilPowerBelowDuration", testDocument);
                Assert.Fail("Repeat until PowerBelow should not serialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilTimeDuration timeDuration = new RepeatUntilTimeDuration(placeholderStep);
                timeDuration.Serialize(database, "RepeatUntilTimeDuration", testDocument);
                Assert.Fail("Repeat until time should not serialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilCaloriesDuration caloriesDuration = new RepeatUntilCaloriesDuration(placeholderStep);
                caloriesDuration.Serialize(database, "RepeatUntilCaloriesDuration", testDocument);
                Assert.Fail("Repeat until calories should not serialize");
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
            RepeatStep placeholderStep = new RepeatStep(placeholderWorkout);

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            XmlAttribute attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);
            readNode = testDocument.CreateElement("TestNode");
            database.AppendChild(readNode);

            // Repeat count
            readNode.InnerXml = repeatCountDurationResult1;
            RepeatCountDuration countDuration = new RepeatCountDuration(placeholderStep);
            countDuration.Deserialize(readNode.FirstChild);
            Assert.AreEqual(4, countDuration.RepetitionCount, "Invalid number of repetitions deserialized");

            readNode.InnerXml = repeatCountDurationResult2;
            countDuration = new RepeatCountDuration(placeholderStep);
            countDuration.Deserialize(readNode.FirstChild);
            Assert.AreEqual(10, countDuration.RepetitionCount, "Invalid number of repetitions deserialized");

            // All other repeat durtions should not deserialize from TCX
            try
            {
                RepeatUntilCaloriesDuration caloriesDuration = new RepeatUntilCaloriesDuration(placeholderStep);
                caloriesDuration.Deserialize(readNode.FirstChild);
                Assert.Fail("Repeat until calories should not deserialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilDistanceDuration distanceDuration = new RepeatUntilDistanceDuration(placeholderStep);
                distanceDuration.Deserialize(readNode.FirstChild);
                Assert.Fail("Repeat until distance should not deserialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilHeartRateAboveDuration hrAboveDuration = new RepeatUntilHeartRateAboveDuration(placeholderStep);
                hrAboveDuration.Deserialize(readNode.FirstChild);
                Assert.Fail("Repeat until HRAbove should not deserialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilHeartRateBelowDuration hrBelowDuration = new RepeatUntilHeartRateBelowDuration(placeholderStep);
                hrBelowDuration.Deserialize(readNode.FirstChild);
                Assert.Fail("Repeat until HRBelow should not deserialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilPowerAboveDuration powerAboveDuration = new RepeatUntilPowerAboveDuration(placeholderStep);
                powerAboveDuration.Deserialize(readNode.FirstChild);
                Assert.Fail("Repeat until PowerAbove should not deserialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilPowerBelowDuration powerBelowDuration = new RepeatUntilPowerBelowDuration(placeholderStep);
                powerBelowDuration.Deserialize(readNode.FirstChild);
                Assert.Fail("Repeat until PowerBelow should not deserialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilTimeDuration timeDuration = new RepeatUntilTimeDuration(placeholderStep);
                timeDuration.Deserialize(readNode.FirstChild);
                Assert.Fail("Repeat until time should not deserialize");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                RepeatUntilCaloriesDuration caloriesDuration = new RepeatUntilCaloriesDuration(placeholderStep);
                caloriesDuration.Deserialize(readNode.FirstChild);
                Assert.Fail("Repeat until calories should not deserialize");
            }
            catch (NotSupportedException)
            {
            }
        }

        [Test]
        public void TestFITSerialization()
        {
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RepeatStep placeholderStep = new RepeatStep(placeholderWorkout);
            FITMessage serializedMessage = new FITMessage(FITGlobalMessageIds.WorkoutStep);
            FITMessageField messageField;

            // Repeat count
            RepeatCountDuration repeatDuration = new RepeatCountDuration(placeholderStep);
            repeatDuration.RepetitionCount = 3;
            repeatDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat count duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatCount, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat count duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat count duration");
            Assert.AreEqual(3, messageField.GetUInt32(), "Invalid target value in field for repeat count duration");
            serializedMessage.Clear();

            repeatDuration.RepetitionCount = 7;
            repeatDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat count duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatCount, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat count duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat count duration");
            Assert.AreEqual(7, messageField.GetUInt32(), "Invalid target value in field for repeat count duration");
            serializedMessage.Clear();

            // Repeat until calories
            RepeatUntilCaloriesDuration caloriesDuration = new RepeatUntilCaloriesDuration(placeholderStep);
            caloriesDuration.CaloriesToSpend = 550;
            caloriesDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until calories duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilCalories, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until calories duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until calories duration");
            Assert.AreEqual(550, messageField.GetUInt32(), "Invalid target value in field for repeat until calories duration");
            serializedMessage.Clear();

            caloriesDuration.CaloriesToSpend = 750;
            caloriesDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until calories duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilCalories, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until calories duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until calories duration");
            Assert.AreEqual(750, messageField.GetUInt32(), "Invalid target value in field for repeat until calories duration");
            serializedMessage.Clear();

            // Repeat until distance
            RepeatUntilDistanceDuration distanceDuration = new RepeatUntilDistanceDuration(1, Length.Units.Kilometer, placeholderStep);
            distanceDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until distance duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilDistance, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until distance duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until distance duration");
            Assert.AreEqual(100000, messageField.GetUInt32(), "Invalid target value in field for repeat until distance duration");
            serializedMessage.Clear();

            distanceDuration = new RepeatUntilDistanceDuration(1, Length.Units.Mile, placeholderStep);
            distanceDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until distance duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilDistance, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until distance duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until distance duration");
            Assert.AreEqual(160934, messageField.GetUInt32(), "Invalid target value in field for repeat until distance duration");
            serializedMessage.Clear();

            // Repeat until HRAbove
            RepeatUntilHeartRateAboveDuration hrAboveDuration = new RepeatUntilHeartRateAboveDuration(placeholderStep);
            hrAboveDuration.IsPercentageMaxHeartRate = false;
            hrAboveDuration.MaxHeartRate = 160;
            hrAboveDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until HRAbove duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilHeartRateGreaterThan, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until HRAbove duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until HRAbove duration");
            Assert.AreEqual(260, messageField.GetUInt32(), "Invalid target value in field for repeat until HRAbove duration");
            serializedMessage.Clear();

            hrAboveDuration.IsPercentageMaxHeartRate = true;
            hrAboveDuration.MaxHeartRate = 80;
            hrAboveDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until HRAbove duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilHeartRateGreaterThan, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until HRAbove duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until HRAbove duration");
            Assert.AreEqual(80, messageField.GetUInt32(), "Invalid target value in field for repeat until HRAbove duration");
            serializedMessage.Clear();

            // Repeat until HRBelow
            RepeatUntilHeartRateBelowDuration hrBelowDuration = new RepeatUntilHeartRateBelowDuration(placeholderStep);
            hrBelowDuration.IsPercentageMaxHeartRate = false;
            hrBelowDuration.MinHeartRate = 150;
            hrBelowDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until HRBelow duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilHeartRateLessThan, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until HRBelow duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until HRBelow duration");
            Assert.AreEqual(250, messageField.GetUInt32(), "Invalid target value in field for repeat until HRBelow duration");
            serializedMessage.Clear();

            hrBelowDuration.IsPercentageMaxHeartRate = true;
            hrBelowDuration.MinHeartRate = 70;
            hrBelowDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until HRBelow duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilHeartRateLessThan, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until HRBelow duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until HRBelow duration");
            Assert.AreEqual(70, messageField.GetUInt32(), "Invalid target value in field for repeat until HRBelow duration");
            serializedMessage.Clear();

            // Repeat until PowerAbove
            RepeatUntilPowerAboveDuration powerAboveDuration = new RepeatUntilPowerAboveDuration(placeholderStep);
            powerAboveDuration.IsPercentFTP = false;
            powerAboveDuration.MaxPower = 250;
            powerAboveDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until PowerAbove duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilPowerGreaterThan, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until PowerAbove duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until PowerAbove duration");
            Assert.AreEqual(1250, messageField.GetUInt32(), "Invalid target value in field for repeat until PowerAbove duration");
            serializedMessage.Clear();

            powerAboveDuration.IsPercentFTP = true;
            powerAboveDuration.MaxPower = 100;
            powerAboveDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until PowerAbove duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilPowerGreaterThan, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until PowerAbove duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until PowerAbove duration");
            Assert.AreEqual(100, messageField.GetUInt32(), "Invalid target value in field for repeat until PowerAbove duration");
            serializedMessage.Clear();

            // Repeat until HRBelow
            RepeatUntilPowerBelowDuration powerBelowDuration = new RepeatUntilPowerBelowDuration(placeholderStep);
            powerBelowDuration.IsPercentFTP = false;
            powerBelowDuration.MinPower = 150;
            powerBelowDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until PowerBelow duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilPowerLessThan, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until PowerBelow duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until PowerBelow duration");
            Assert.AreEqual(1150, messageField.GetUInt32(), "Invalid target value in field for repeat until PowerBelow duration");
            serializedMessage.Clear();

            powerBelowDuration.IsPercentFTP = true;
            powerBelowDuration.MinPower = 270;
            powerBelowDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until PowerBelow duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilPowerLessThan, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until PowerBelow duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until PowerBelow duration");
            Assert.AreEqual(270, messageField.GetUInt32(), "Invalid target value in field for repeat until PowerBelow duration");
            serializedMessage.Clear();

            // Repeat until time
            RepeatUntilTimeDuration timeDuration = new RepeatUntilTimeDuration(placeholderStep);
            timeDuration.TimeInSeconds = 600;
            timeDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until time duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilTime, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until time duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until time duration");
            Assert.AreEqual(600000, messageField.GetUInt32(), "Invalid target value in field for repeat until time duration");
            serializedMessage.Clear();

            timeDuration.TimeInSeconds = 350;
            timeDuration.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            Assert.IsNotNull(messageField, "Duration type field not serialized for repeat until time duration");
            Assert.AreEqual(FITWorkoutStepDurationTypes.RepeatUntilTime, (FITWorkoutStepDurationTypes)messageField.GetEnum(), "Invalid duration type in field for repeat until time duration");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            Assert.IsNotNull(messageField, "Target value field not serialized for repeat until time duration");
            Assert.AreEqual(350000, messageField.GetUInt32(), "Invalid target value in field for repeat until time duration");
            serializedMessage.Clear();
        }

        [Test]
        public void TestFITDeserialization()
        {
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RepeatStep placeholderStep = new RepeatStep(placeholderWorkout);
            FITMessage serializedMessage = new FITMessage(FITGlobalMessageIds.WorkoutStep);
            FITMessageField typeField = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField valueField = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);
            IRepeatDuration deserializedDuration;

            serializedMessage.AddField(typeField);
            serializedMessage.AddField(valueField);

            // Repeat count
            typeField.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatCount);
            valueField.SetUInt32(6);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatCountDuration, "Repeat count duration not properly FIT deserialized");
            RepeatCountDuration repeatDuration = deserializedDuration as RepeatCountDuration;
            Assert.AreEqual(6, repeatDuration.RepetitionCount, "Invalid value deserialized for FIT repeat count duration");

            valueField.SetUInt32(2);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatCountDuration, "Repeat count duration not properly FIT deserialized");
            repeatDuration = deserializedDuration as RepeatCountDuration;
            Assert.AreEqual(2, repeatDuration.RepetitionCount, "Invalid value deserialized for FIT repeat count duration");

            // Repeat until calories
            typeField.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilCalories);
            valueField.SetUInt32(666);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilCaloriesDuration, "Repeat until calories duration not properly FIT deserialized");
            RepeatUntilCaloriesDuration caloriesDuration = deserializedDuration as RepeatUntilCaloriesDuration;
            Assert.AreEqual(666, caloriesDuration.CaloriesToSpend, "Invalid value deserialized for FIT repeat until calories duration");

            valueField.SetUInt32(150);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilCaloriesDuration, "Repeat until calories duration not properly FIT deserialized");
            caloriesDuration = deserializedDuration as RepeatUntilCaloriesDuration;
            Assert.AreEqual(150, caloriesDuration.CaloriesToSpend, "Invalid value deserialized for FIT repeat until calories duration");

            // Repeat until distance
            typeField.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilDistance);
            valueField.SetUInt32(100000);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilDistanceDuration, "Repeat until distance duration not properly FIT deserialized");
            RepeatUntilDistanceDuration distanceDuration = deserializedDuration as RepeatUntilDistanceDuration;
            Assert.AreEqual(1,
                            Length.Convert(distanceDuration.GetDistanceInBaseUnit(), distanceDuration.BaseUnit, Length.Units.Kilometer),
                            STCommon.Data.Constants.Delta,
                            "Invalid value deserialized for FIT repeat until distance duration");

            valueField.SetUInt32(160934);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilDistanceDuration, "Repeat until distance duration not properly FIT deserialized");
            distanceDuration = deserializedDuration as RepeatUntilDistanceDuration;
            Assert.AreEqual(1,
                            Length.Convert(distanceDuration.GetDistanceInBaseUnit(), distanceDuration.BaseUnit, Length.Units.Mile),
                            STCommon.Data.Constants.Delta,
                            "Invalid value deserialized for FIT repeat until distance duration");

            // Repeat until HR Above
            typeField.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilHeartRateGreaterThan);
            valueField.SetUInt32(220);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilHeartRateAboveDuration, "Repeat until HRAbove duration not properly FIT deserialized");
            RepeatUntilHeartRateAboveDuration hrAboveDuration = deserializedDuration as RepeatUntilHeartRateAboveDuration;
            Assert.IsFalse(hrAboveDuration.IsPercentageMaxHeartRate, "Invalid value deserialized for FIT repeat until HRAbove duration");
            Assert.AreEqual(120, hrAboveDuration.MaxHeartRate, "Invalid value deserialized for FIT repeat until HRAbove duration");

            valueField.SetUInt32(80);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilHeartRateAboveDuration, "Repeat until HRAbove duration not properly FIT deserialized");
            hrAboveDuration = deserializedDuration as RepeatUntilHeartRateAboveDuration;
            Assert.IsTrue(hrAboveDuration.IsPercentageMaxHeartRate, "Invalid value deserialized for FIT repeat until HRAbove duration");
            Assert.AreEqual(80, hrAboveDuration.MaxHeartRate, "Invalid value deserialized for FIT repeat until HRAbove duration");

            // Repeat until HR Below
            typeField.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilHeartRateLessThan);
            valueField.SetUInt32(230);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilHeartRateBelowDuration, "Repeat until HRBelow duration not properly FIT deserialized");
            RepeatUntilHeartRateBelowDuration hrBelowDuration = deserializedDuration as RepeatUntilHeartRateBelowDuration;
            Assert.IsFalse(hrBelowDuration.IsPercentageMaxHeartRate, "Invalid value deserialized for FIT repeat until HRBelow duration");
            Assert.AreEqual(130, hrBelowDuration.MinHeartRate, "Invalid value deserialized for FIT repeat until HRBelow duration");

            valueField.SetUInt32(65);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilHeartRateBelowDuration, "Repeat until HRBelow duration not properly FIT deserialized");
            hrBelowDuration = deserializedDuration as RepeatUntilHeartRateBelowDuration;
            Assert.IsTrue(hrBelowDuration.IsPercentageMaxHeartRate, "Invalid value deserialized for FIT repeat until HRBelow duration");
            Assert.AreEqual(65, hrBelowDuration.MinHeartRate, "Invalid value deserialized for FIT repeat until HRBelow duration");

            // Repeat until Power Above
            typeField.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilPowerGreaterThan);
            valueField.SetUInt32(1220);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilPowerAboveDuration, "Repeat until PowerAbove duration not properly FIT deserialized");
            RepeatUntilPowerAboveDuration powerAboveDuration = deserializedDuration as RepeatUntilPowerAboveDuration;
            Assert.IsFalse(powerAboveDuration.IsPercentFTP, "Invalid value deserialized for FIT repeat until PowerAbove duration");
            Assert.AreEqual(220, powerAboveDuration.MaxPower, "Invalid value deserialized for FIT repeat until PowerAbove duration");

            valueField.SetUInt32(80);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilPowerAboveDuration, "Repeat until PowerAbove duration not properly FIT deserialized");
            powerAboveDuration = deserializedDuration as RepeatUntilPowerAboveDuration;
            Assert.IsTrue(powerAboveDuration.IsPercentFTP, "Invalid value deserialized for FIT repeat until PowerAbove duration");
            Assert.AreEqual(80, powerAboveDuration.MaxPower, "Invalid value deserialized for FIT repeat until PowerAbove duration");

            // Repeat until Power Below
            typeField.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilPowerLessThan);
            valueField.SetUInt32(1400);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilPowerBelowDuration, "Repeat until PowerBelow duration not properly FIT deserialized");
            RepeatUntilPowerBelowDuration powerBelowDuration = deserializedDuration as RepeatUntilPowerBelowDuration;
            Assert.IsFalse(powerBelowDuration.IsPercentFTP, "Invalid value deserialized for FIT repeat until PowerBelow duration");
            Assert.AreEqual(400, powerBelowDuration.MinPower, "Invalid value deserialized for FIT repeat until PowerBelow duration");

            valueField.SetUInt32(125);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilPowerBelowDuration, "Repeat until PowerBelow duration not properly FIT deserialized");
            powerBelowDuration = deserializedDuration as RepeatUntilPowerBelowDuration;
            Assert.IsTrue(powerBelowDuration.IsPercentFTP, "Invalid value deserialized for FIT repeat until PowerBelow duration");
            Assert.AreEqual(125, powerBelowDuration.MinPower, "Invalid value deserialized for FIT repeat until PowerBelow duration");

            // Repeat until time
            typeField.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilTime);
            valueField.SetUInt32(60000);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilTimeDuration, "Repeat until time duration not properly FIT deserialized");
            RepeatUntilTimeDuration timeDuration = deserializedDuration as RepeatUntilTimeDuration;
            Assert.AreEqual(60, timeDuration.TimeInSeconds, "Invalid value deserialized for FIT repeat until time duration");

            valueField.SetUInt32(600000);
            deserializedDuration = DurationFactory.Create(serializedMessage, placeholderStep);
            Assert.IsTrue(deserializedDuration is RepeatUntilTimeDuration, "Repeat until time duration not properly FIT deserialized");
            timeDuration = deserializedDuration as RepeatUntilTimeDuration;
            Assert.AreEqual(600, timeDuration.TimeInSeconds, "Invalid value deserialized for FIT repeat until time duration");
        }

        const String repeatCountDurationResult1 = "<RepeatCountDuration1>4</RepeatCountDuration1>";
        const String repeatCountDurationResult2 = "<RepeatCountDuration2>10</RepeatCountDuration2>";
    }
}
