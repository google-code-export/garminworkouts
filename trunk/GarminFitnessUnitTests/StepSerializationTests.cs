using System;
using System.Xml;
using NUnit.Framework;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessUnitTests
{
    [TestFixture]
    class StepSerializationTests
    {
        [Test]
        public void TestRegularStepTCXSerialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep step = placeholderWorkout.Steps[0] as RegularStep;
            int resultPosition;

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);

            // Active step
            step.Name = "StepTest1";
            step.Intensity = RegularStep.StepIntensity.Active;
            step.Serialize(database, "StepTest1", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(regularStepTestResult1);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for active step");

            // Rest step
            step.Name = "StepTest2";
            step.Intensity = RegularStep.StepIntensity.Rest;
            step.Serialize(database, "StepTest2", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(regularStepTestResult2);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for rest step");

            // Warmup step
            step = new RegularStep(placeholderWorkout);
            placeholderWorkout.Steps.AddStepToRoot(step);
            Options.Instance.TCXExportWarmupAs = RegularStep.StepIntensity.Active;
            step.Name = "StepTest3";
            step.Intensity = RegularStep.StepIntensity.Warmup;
            step.Serialize(database, "StepTest3", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(regularStepTestResult3);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for warmup step");

            step.Name = "StepTest4";
            Options.Instance.TCXExportWarmupAs = RegularStep.StepIntensity.Rest;
            step.Serialize(database, "StepTest4", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(regularStepTestResult4);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for warmup step");

            // Cooldown step
            Options.Instance.TCXExportCooldownAs = RegularStep.StepIntensity.Active;
            step.Name = "StepTest5";
            step.Intensity = RegularStep.StepIntensity.Cooldown;
            step.Serialize(database, "StepTest5", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(regularStepTestResult5);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for cooldown step");

            step.Name = "StepTest6";
            Options.Instance.TCXExportCooldownAs = RegularStep.StepIntensity.Rest;
            step.Serialize(database, "StepTest6", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(regularStepTestResult6);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for cooldown step");
        }

        [Test]
        public void TestRegularStepTCXDeserialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode readNode;
            XmlNode database;
            XmlAttribute attribute;
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep step = placeholderWorkout.Steps[0] as RegularStep;

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);
            readNode = testDocument.CreateElement("TestNode");
            database.AppendChild(readNode);

            // Active step
            readNode.InnerXml = regularStepTestResult1;
            step.Deserialize(readNode.FirstChild);
            Assert.AreEqual("StepTest1", step.Name, "Invalid step name in TCX deserialization");
            Assert.AreEqual(RegularStep.StepIntensity.Active, step.Intensity, "Invalid step intensity in active step TCX deserialization");

            // Rest step
            readNode.InnerXml = regularStepTestResult2;
            step.Deserialize(readNode.FirstChild);
            Assert.AreEqual("StepTest2", step.Name, "Invalid step name in TCX deserialization");
            Assert.AreEqual(RegularStep.StepIntensity.Rest, step.Intensity, "Invalid step intensity in rest step TCX deserialization");

            // No name node in XML should revert to empty name
            readNode.InnerXml = noNameStepTestResult;
            step.Deserialize(readNode.FirstChild);
            Assert.IsTrue(String.IsNullOrEmpty(step.Name), "Invalid empty step name in TCX deserialization");

            // Invalid steps            
            try
            {
                readNode.InnerXml = stepInvalidResult1;
                step.Deserialize(readNode.FirstChild);
                Assert.Fail("Missing step duration deserialization should trigger exception");
            }
            catch (GarminFitnessXmlDeserializationException)
            {
            }

            try
            {
                readNode.InnerXml = stepInvalidResult2;
                step.Deserialize(readNode.FirstChild);
                Assert.Fail("Missing step intensity TCX deserialization should trigger exception");
            }
            catch (GarminFitnessXmlDeserializationException)
            {
            }

            try
            {
                readNode.InnerXml = stepInvalidResult3;
                step.Deserialize(readNode.FirstChild);
                Assert.Fail("Missing step target TCX deserialization should trigger exception");
            }
            catch (GarminFitnessXmlDeserializationException)
            {
            }

            // Duration & target test
            readNode.InnerXml = regularStepComponentsTestResult;
            step.Deserialize(readNode.FirstChild);
            Assert.IsTrue(step.Duration is DistanceDuration, "Invalid step duration TCX deserialization");
            Assert.IsTrue(step.Target is BaseHeartRateTarget, "Invalid step target TCX deserialization");
        }

        [Test]
        public void TestRegularStepFITSerialization()
        {
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep placeholderStep = placeholderWorkout.Steps[0] as RegularStep;
            bool exportHRAsMax = Options.Instance.ExportSportTracksHeartRateAsPercentMax;
            bool exportPowerAsFTP = Options.Instance.ExportSportTracksPowerAsPercentFTP;
            FITMessage serializedMessage = new FITMessage(FITGlobalMessageIds.WorkoutStep);
            FITMessageField messageField;

            // Active step
            placeholderStep.Name = "TestStep1";
            placeholderStep.Intensity = RegularStep.StepIntensity.Active;
            placeholderStep.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.MessageIndex);
            Assert.IsNotNull(messageField, "Message index field not serialized for step");
            Assert.AreEqual(0, messageField.GetUInt16(), "Invalid message index FIT serialization for regular step");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.StepName);
            Assert.IsNotNull(messageField, "Invalid active step name FIT serialization");
            Assert.AreEqual("TestStep1", messageField.GetString(), "Invalid name in field for active step");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.Intensity);
            Assert.IsNotNull(messageField, "Invalid active step intensity FIT serialization");
            Assert.AreEqual(FITWorkoutStepIntensity.Active, (FITWorkoutStepIntensity)messageField.GetEnum(), "Invalid intensity in field for active step");
            serializedMessage.Clear();

            // Rest step
            placeholderStep.Name = "TestStep2";
            placeholderStep.Intensity = RegularStep.StepIntensity.Rest;
            placeholderStep.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.MessageIndex);
            Assert.IsNotNull(messageField, "Message index field not serialized for step");
            Assert.AreEqual(0, messageField.GetUInt16(), "Invalid message index FIT serialization for rest step");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.StepName);
            Assert.IsNotNull(messageField, "Invalid active step name FIT serialization");
            Assert.AreEqual("TestStep2", messageField.GetString(), "Invalid name in field for rest step");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.Intensity);
            Assert.IsNotNull(messageField, "Invalid active step intensity FIT serialization");
            Assert.AreEqual(FITWorkoutStepIntensity.Rest, (FITWorkoutStepIntensity)messageField.GetEnum(), "Invalid intensity in field for rest step");
            serializedMessage.Clear();

            // Warmup step
            placeholderStep = new RegularStep(placeholderWorkout);
            placeholderWorkout.Steps.AddStepToRoot(placeholderStep);
            placeholderStep.Name = "TestStep3";
            placeholderStep.Intensity = RegularStep.StepIntensity.Warmup;
            placeholderStep.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.MessageIndex);
            Assert.IsNotNull(messageField, "Message index field not serialized for step");
            Assert.AreEqual(1, messageField.GetUInt16(), "Invalid message index FIT serialization for warmup step");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.StepName);
            Assert.IsNotNull(messageField, "Invalid active step name FIT serialization");
            Assert.AreEqual("TestStep3", messageField.GetString(), "Invalid name in field for warmup step");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.Intensity);
            Assert.IsNotNull(messageField, "Invalid active step intensity FIT serialization");
            Assert.AreEqual(FITWorkoutStepIntensity.Warmup, (FITWorkoutStepIntensity)messageField.GetEnum(), "Invalid intensity in field for warmup step");
            serializedMessage.Clear();

            // Cooldown step
            placeholderStep.Name = "TestStep4";
            placeholderStep.Intensity = RegularStep.StepIntensity.Cooldown;
            placeholderStep.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.MessageIndex);
            Assert.IsNotNull(messageField, "Message index field not serialized for step");
            Assert.AreEqual(1, messageField.GetUInt16(), "Invalid message index FIT serialization for cooldwon step");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.StepName);
            Assert.IsNotNull(messageField, "Invalid active step name FIT serialization");
            Assert.AreEqual("TestStep4", messageField.GetString(), "Invalid name in field for cooldown step");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.Intensity);
            Assert.IsNotNull(messageField, "Invalid active step intensity FIT serialization");
            Assert.AreEqual(FITWorkoutStepIntensity.Cooldown, (FITWorkoutStepIntensity)messageField.GetEnum(), "Invalid intensity in field for cooldown step");
            serializedMessage.Clear();
        }

        [Test]
        public void TestRegularStepFITDeserialization()
        {
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep placeholderStep = new RegularStep(placeholderWorkout);
            FITMessage serializedMessage = new FITMessage(FITGlobalMessageIds.WorkoutStep);
            FITMessageField nameField = new FITMessageField((Byte)FITWorkoutStepFieldIds.StepName);
            FITMessageField intensityField = new FITMessageField((Byte)FITWorkoutStepFieldIds.Intensity);
            FITMessageField durationTypeField = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField durationValueField = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationValue);
            FITMessageField targetTypeField = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetType);
            FITMessageField targetValueField = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);
            FITMessageField targetLowRangeField = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueLow);
            FITMessageField targetHighRangeField = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueHigh);

            // Setup message
            serializedMessage.AddField(nameField);
            serializedMessage.AddField(intensityField);
            serializedMessage.AddField(durationTypeField);
            serializedMessage.AddField(durationValueField);
            serializedMessage.AddField(targetTypeField);
            serializedMessage.AddField(targetValueField);
            serializedMessage.AddField(targetLowRangeField);
            serializedMessage.AddField(targetHighRangeField);
            durationTypeField.SetEnum((Byte)FITWorkoutStepDurationTypes.Calories);
            durationValueField.SetUInt32(456);
            targetTypeField.SetEnum((Byte)FITWorkoutStepTargetTypes.Cadence);
            targetValueField.SetUInt32(0);
            targetLowRangeField.SetUInt32(80);
            targetHighRangeField.SetUInt32(90);

            // Active step
            nameField.SetString("TestStep1");
            intensityField.SetEnum((Byte)FITWorkoutStepIntensity.Active);
            placeholderStep.DeserializeFromFIT(serializedMessage);
            Assert.AreEqual("TestStep1", placeholderStep.Name, "Name not properly FIT deserialized for active step");
            Assert.AreEqual(RegularStep.StepIntensity.Active, placeholderStep.Intensity, "Intensity not properly FIT deserialized for active step");

            // Rest step
            nameField.SetString("TestStep2");
            intensityField.SetEnum((Byte)FITWorkoutStepIntensity.Rest);
            placeholderStep.DeserializeFromFIT(serializedMessage);
            Assert.AreEqual("TestStep2", placeholderStep.Name, "Name not properly FIT deserialized for rest step");
            Assert.AreEqual(RegularStep.StepIntensity.Rest, placeholderStep.Intensity, "Intensity not properly FIT deserialized for rest step");

            // Warmup step
            nameField.SetString("TestStep3");
            intensityField.SetEnum((Byte)FITWorkoutStepIntensity.Warmup);
            placeholderStep.DeserializeFromFIT(serializedMessage);
            Assert.AreEqual("TestStep3", placeholderStep.Name, "Name not properly FIT deserialized for warmup step");
            Assert.AreEqual(RegularStep.StepIntensity.Warmup, placeholderStep.Intensity, "Intensity not properly FIT deserialized for warmup step");

            // Cooldown step
            nameField.SetString("TestStep4");
            intensityField.SetEnum((Byte)FITWorkoutStepIntensity.Cooldown);
            placeholderStep.DeserializeFromFIT(serializedMessage);
            Assert.AreEqual("TestStep4", placeholderStep.Name, "Name not properly FIT deserialized for cooldown step");
            Assert.AreEqual(RegularStep.StepIntensity.Cooldown, placeholderStep.Intensity, "Intensity not properly FIT deserialized for cooldown step");

            // Duration & target test
            Assert.IsTrue(placeholderStep.Duration is CaloriesDuration, "Duration not properly FIT deserialized in regular step");
            Assert.IsTrue(placeholderStep.Target is BaseCadenceTarget, "Target not properly FIT deserialized in regular step");
        }
        
        [Test]
        public void TestRepeatStepTCXSerialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RepeatStep step = new RepeatStep(placeholderWorkout);
            int resultPosition;

            placeholderWorkout.Steps.AddStepToRoot(step);
            placeholderWorkout.Steps.RemoveStep(placeholderWorkout.Steps[0]);

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);

            // Single child
            step.Serialize(database, "RepeatStepTest1", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(repeatStepTestResult1);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for repeat step with single child");

            // Multiple children
            step.StepsToRepeat.Add(new RegularStep(placeholderWorkout));
            step.Serialize(database, "RepeatStepTest2", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(repeatStepTestResult2);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for repeat step with multiple children");

            // Nested repeat steps
            step.StepsToRepeat.Add(new RepeatStep(placeholderWorkout));
            step.Serialize(database, "RepeatStepTest3", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(repeatStepTestResult3);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for repeat step with nested repeat child");
        }

        [Test]
        public void TestRepeatStepTCXDeserialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode readNode;
            XmlNode database;
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RepeatStep repeatStep = new RepeatStep(placeholderWorkout);

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            XmlAttribute attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);
            readNode = testDocument.CreateElement("TestNode");
            database.AppendChild(readNode);

            // Single child
            readNode.InnerXml = repeatStepTestResult1;
            repeatStep.Deserialize(readNode.FirstChild);
            Assert.AreEqual(1, repeatStep.StepsToRepeat.Count, "Invalid step count deserialized for repeat step with single child");
            Assert.IsTrue(repeatStep.StepsToRepeat[0] is RegularStep, "Invalid child step deserialized for repeat step with single child");

            // Multiple children
            readNode.InnerXml = repeatStepTestResult2;
            repeatStep.Deserialize(readNode.FirstChild);
            Assert.AreEqual(2, repeatStep.StepsToRepeat.Count, "Invalid step count deserialized for repeat step with multiple children");
            Assert.IsTrue(repeatStep.StepsToRepeat[0] is RegularStep, "Invalid child step deserialized for repeat step with multiple children");
            Assert.IsTrue(repeatStep.StepsToRepeat[1] is RegularStep, "Invalid child step deserialized for repeat step with multiple children");

            // Nested repeat step
            readNode.InnerXml = repeatStepTestResult3;
            repeatStep.Deserialize(readNode.FirstChild);
            Assert.AreEqual(3, repeatStep.StepsToRepeat.Count, "Invalid step count deserialized for repeat step with nested repeat child");
            Assert.IsTrue(repeatStep.StepsToRepeat[0] is RegularStep, "Invalid child step deserialized for repeat step with nested repeat child");
            Assert.IsTrue(repeatStep.StepsToRepeat[1] is RegularStep, "Invalid child step deserialized for repeat step with nested repeat child");
            Assert.IsTrue(repeatStep.StepsToRepeat[2] is RepeatStep, "Invalid child step deserialized for repeat step with nested repeat child");
            RepeatStep tempRepeat = repeatStep.StepsToRepeat[2] as RepeatStep;
            Assert.AreEqual(1, tempRepeat.StepsToRepeat.Count, "Invalid child step deserialized for repeat step with nested repeat child");
            Assert.IsTrue(tempRepeat.StepsToRepeat[0] is RegularStep, "Invalid child step deserialized for repeat step with nested repeat child");

            // Regular step after repeat
            readNode.InnerXml = repeatStepTestResult4;
            repeatStep.Deserialize(readNode.FirstChild);
            Assert.AreEqual(4, repeatStep.StepsToRepeat.Count, "Invalid step count deserialized for repeat step with step after nested repeat child");
            Assert.IsTrue(repeatStep.StepsToRepeat[3] is RegularStep, "Invalid child step deserialized for repeat step with step after nested repeat child");
        }

        [Test]
        public void TestRepeatStepFITSerialization()
        {
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RepeatStep repeatStep = new RepeatStep(placeholderWorkout);
            FITMessage serializedMessage = new FITMessage(FITGlobalMessageIds.WorkoutStep);
            FITMessageField messageField;

            placeholderWorkout.Steps.AddStepToRoot(repeatStep);
            placeholderWorkout.Steps.RemoveStep(placeholderWorkout.Steps[0]);

            // Single child
            // - Root
            //  - Repeat step (id = 1)
            //   - Regular step (id = 0)
            repeatStep.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationValue);
            Assert.IsNotNull(messageField, "Duration value field not serialized for repeat step");
            Assert.AreEqual(0, messageField.GetUInt32(), "Invalid duration value FIT serialization for repeat step with single child");
            serializedMessage.Clear();

            // Multiple children
            // - Root
            //  - Repeat step (id = 2)
            //   - Regular step (id = 0)
            //   - Regular step (id = 1)
            repeatStep.StepsToRepeat.Add(new RegularStep(placeholderWorkout));
            repeatStep.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationValue);
            Assert.IsNotNull(messageField, "Duration value field not serialized for repeat step");
            Assert.AreEqual(0, messageField.GetUInt32(), "Invalid duration value FIT serialization for repeat step with single child");
            serializedMessage.Clear();

            // Nested repeat steps
            //  - Repeat step (id = 4)
            //   - Regular step (id = 0)
            //   - Regular step (id = 1)
            //   - Repeat step (id = 3)
            //    - Regular step (id = 2)
            repeatStep.StepsToRepeat.Add(new RepeatStep(placeholderWorkout));
            repeatStep.StepsToRepeat[2].FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationValue);
            Assert.IsNotNull(messageField, "Duration value field not serialized for repeat step");
            Assert.AreEqual(2, messageField.GetUInt32(), "Invalid duration value FIT serialization for repeat step with single child");
            serializedMessage.Clear();

            // Nested repeats with 2nd repeat as first nested step
            // - Root
            //  - Repeat step (id = 4)
            //   - Regular step (id = 0)
            //   - Regular step (id = 1)
            //   - Repeat step (id = 3)
            //    - Regular step (id = 2)
            //  - Repeat step (id = 8)
            //   - Repeat step (id = 6)
            //    - Regular step (id = 5)
            //   - Regular step (id = 7)
            repeatStep = new RepeatStep(placeholderWorkout);
            placeholderWorkout.Steps.AddStepToRoot(repeatStep);
            repeatStep.StepsToRepeat.Insert(0, new RepeatStep(placeholderWorkout));
            repeatStep.FillFITStepMessage(serializedMessage);
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationValue);
            Assert.IsNotNull(messageField, "Duration value field not serialized for repeat step");
            Assert.AreEqual(5, messageField.GetUInt32(), "Invalid duration value FIT serialization for repeat step with single child");
            serializedMessage.Clear();
        }

        [Test]
        public void TestRepeatStepFITDeserialization()
        {
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep regularStep;
            RepeatStep repeatStep = new RepeatStep(placeholderWorkout);
            FITMessage serializedMessage = new FITMessage(FITGlobalMessageIds.WorkoutStep);
            FITMessageField durationValueField = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationValue);
            FITMessageField durationTypeField = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField targetValueField = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);

            regularStep = placeholderWorkout.Steps[0] as RegularStep;

            // Setup message
            serializedMessage.AddField(durationValueField);
            serializedMessage.AddField(durationTypeField);
            serializedMessage.AddField(targetValueField);
            durationValueField.SetUInt32(0); // Step id to start repeat at
            durationTypeField.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatCount);
            targetValueField.SetUInt32(2);
            
            // This test is a little hard to follow because repeat steps in FIT format regroup previously deserialized steps.
            //  We will illustrate the expected output throughout the test to ease validation.

            // At this point we have the following
            // - Root
            //  - Regular step (id = 0)
            // By deserializing the message we should get the following
            // - Root
            //  - Repeat step (id = 1)
            //   - Regular step (id = 0)
            repeatStep.DeserializeFromFIT(serializedMessage);
            placeholderWorkout.Steps.AddStepToRoot(repeatStep);
            Assert.AreEqual(1, placeholderWorkout.Steps.Count, "Incorrect number of steps in workout after FIT deserialization of repeat step with single child");
            Assert.AreEqual(2, placeholderWorkout.StepCount, "Incorrect number of steps in workout after FIT deserialization of repeat step with single child");
            Assert.IsTrue(placeholderWorkout.Steps[0] is RepeatStep, "Invalid workout structure after FIT deserialization of repeat step with single child");
            Assert.AreEqual(repeatStep, placeholderWorkout.Steps[0], "Invalid workout structure after FIT deserialization of repeat step with single child");
            Assert.AreEqual(1, repeatStep.StepsToRepeat.Count, "Invalid number of children in repeat after FIT deserialization of repeat step with single child");
            Assert.AreEqual(regularStep, repeatStep.StepsToRepeat[0], "Invalid child in repeat after FIT deserialization of repeat step with single child");

            placeholderWorkout.Steps.AddStepToRoot(new RegularStep(placeholderWorkout));
            placeholderWorkout.Steps.AddStepToRoot(new RegularStep(placeholderWorkout));
            repeatStep = new RepeatStep(placeholderWorkout);
            durationValueField.SetUInt32(2); // Step id to start repeat at

            // At this point we have the following
            // - Root
            //  - Repeat step (id = 1)
            //   - Regular step (id = 0)
            //  - Regular step (id = 2)
            //  - Regular step (id = 3)
            // By deserializing the message we should get the following
            // - Root
            //  - Repeat step (id = 1)
            //   - Regular step (id = 0)
            //  - Repeat step (id = 4)
            //   - Regular step (id = 2)
            //   - Regular step (id = 3)
            repeatStep.DeserializeFromFIT(serializedMessage);
            placeholderWorkout.Steps.AddStepToRoot(repeatStep);
            Assert.AreEqual(2, placeholderWorkout.Steps.Count, "Incorrect number of steps in workout after FIT deserialization of repeat step with multiple children");
            Assert.AreEqual(5, placeholderWorkout.StepCount, "Incorrect number of steps in workout after FIT deserialization of repeat step with multiple children");
            Assert.IsTrue(placeholderWorkout.Steps[1] is RepeatStep, "Invalid workout structure after FIT deserialization of repeat step with multiple children");
            Assert.AreEqual(repeatStep, placeholderWorkout.Steps[1], "Invalid workout structure after FIT deserialization of repeat step with multiple children");
            Assert.AreEqual(2, repeatStep.StepsToRepeat.Count, "Invalid number of children in repeat after FIT deserialization of repeat step with multiple children");
            Assert.IsTrue(placeholderWorkout.Steps[0] is RepeatStep, "Invalid repeat step structure after FIT deserialization of repeat step with multiple children");
            Assert.IsTrue(placeholderWorkout.Steps[1] is RepeatStep, "Invalid repeat step structure after FIT deserialization of repeat step with multiple children");

            regularStep = new RegularStep(placeholderWorkout);
            placeholderWorkout.Steps.InsertStepBeforeStep(regularStep, placeholderWorkout.Steps[0]);
            repeatStep = new RepeatStep(placeholderWorkout);
            durationValueField.SetUInt32(1); // Step id to start repeat at

            // At this point we have the following
            // - Root
            //  - Regular step (id = 0)
            //  - Repeat step (id = 2)
            //   - Regular step (id = 1)
            //  - Repeat step (id = 5)
            //   - Regular step (id = 4)
            //   - Regular step (id = 5)
            //  - Repeat step (not deserialized yet)
            // By deserializing the message we should get the following
            // - Root
            //  - Regular step (id = 0)
            //  - Repeat step (id = 6)
            //   - Repeat step (id = 2)
            //    - Regular step (id = 1)
            //   - Repeat step (id = 5)
            //    - Regular step (id = 3)
            //    - Regular step (id = 4)
            repeatStep.DeserializeFromFIT(serializedMessage);
            placeholderWorkout.Steps.AddStepToRoot(repeatStep);
            Assert.AreEqual(2, placeholderWorkout.Steps.Count, "Incorrect number of steps in workout after FIT deserialization of repeat step with multiple nested children");
            Assert.AreEqual(7, placeholderWorkout.StepCount, "Incorrect number of steps in workout after FIT deserialization of repeat step with multiple nested children");
            Assert.IsTrue(placeholderWorkout.Steps[0] is RegularStep, "Invalid workout structure after FIT deserialization of repeat step with multiple nested children");
            Assert.IsTrue(placeholderWorkout.Steps[1] is RepeatStep, "Invalid workout structure after FIT deserialization of repeat step with multiple nested children");
            Assert.AreEqual(regularStep, placeholderWorkout.Steps[0], "Invalid workout structure after FIT deserialization of repeat step with multiple nested children");
            Assert.AreEqual(repeatStep, placeholderWorkout.Steps[1], "Invalid workout structure after FIT deserialization of repeat step with multiple nested children");
            Assert.AreEqual(2, repeatStep.StepsToRepeat.Count, "Invalid number of children in repeat after FIT deserialization of repeat step with multiple nested children");
            Assert.IsTrue(repeatStep.StepsToRepeat[0] is RepeatStep, "Invalid repeat step structure after FIT deserialization of repeat step with multiple nested children");
            Assert.IsTrue(repeatStep.StepsToRepeat[1] is RepeatStep, "Invalid repeat step structure after FIT deserialization of repeat step with multiple nested children");
        }

        [Test]
        public void TestLinkStepTCXSerialization()
        {
            // We are validating
            XmlDocument testDocument = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;
            IWorkout workout = GarminWorkoutManager.Instance.GetWorkout("LinkStepTest1");
            WorkoutLinkStep linkStep = workout.Steps[0] as WorkoutLinkStep;
            int resultPosition;

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);

            // Basic link step test
            linkStep.Serialize(database, "LinkStepTest1", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(linkStepTestResult1);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for basic link step");

            // Link step not the first in the workout
            workout = GarminWorkoutManager.Instance.GetWorkout("LinkStepTest2");
            linkStep = workout.Steps[3] as WorkoutLinkStep;
            linkStep.Serialize(database, "LinkStepTest2", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(linkStepTestResult2);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for link step not first in workout");

            // Link step nested inside a repeat
            workout = GarminWorkoutManager.Instance.GetWorkout("LinkStepTest3");
            linkStep = (workout.Steps[1] as RepeatStep).StepsToRepeat[0] as WorkoutLinkStep;
            linkStep.Serialize(database, "LinkStepTest3", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(linkStepTestResult3);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for link step nested in repeat");

            // Link step with a forced split
            workout = GarminWorkoutManager.Instance.GetWorkout("LinkStepTest4");
            workout.Serialize(database, "LinkStepTest4", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(linkStepTestResult4);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for link step with a forced split");

            // Link step on split boundary
            workout = GarminWorkoutManager.Instance.GetWorkout("LinkStepTest5");
            workout.Serialize(database, "LinkStepTest5", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(linkStepTestResult5);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for link step on split boundary");

            // Link step with a forced split nested in a repeat
            workout = GarminWorkoutManager.Instance.GetWorkout("LinkStepTest6");
            workout.Serialize(database, "LinkStepTest6", testDocument);
            resultPosition = testDocument.InnerXml.IndexOf(linkStepTestResult6);
            Assert.GreaterOrEqual(resultPosition, 0, "Invalid step TCX serialization for link step with a forced split nested in a repeat");
        }

        [Test]
        public void TestStepNotesTCXSerialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep regularStep = placeholderWorkout.Steps[0] as RegularStep;

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);

            // Regular step
            regularStep.Notes = "This is a note";
            regularStep.Serialize(database, "stepNotesTest", testDocument);
            Assert.GreaterOrEqual(placeholderWorkout.STExtensions.Count, 1, "Missing step extension node for regular step note");
            Assert.AreEqual(stepNotesExtensionResult1,
                            placeholderWorkout.STExtensions[placeholderWorkout.STExtensions.Count - 1].OuterXml,
                            "Invalid step notes serialization");

            regularStep.Notes = "This is a new note";
            regularStep.Serialize(database, "stepNotesTest", testDocument);
            Assert.GreaterOrEqual(placeholderWorkout.STExtensions.Count, 1, "Missing step extension node for regular step note");
            Assert.AreEqual(stepNotesExtensionResult2,
                            placeholderWorkout.STExtensions[placeholderWorkout.STExtensions.Count - 1].OuterXml,
                            "Invalid step notes serialization");

            // Repeat step
            RepeatStep repeatStep = new RepeatStep(placeholderWorkout);
            placeholderWorkout.Steps.AddStepToRoot(repeatStep);
            repeatStep.Notes = "This is a repeat note";
            repeatStep.Serialize(database, "stepNotesTest", testDocument);
            Assert.GreaterOrEqual(placeholderWorkout.STExtensions.Count, 1, "Missing step extension node for repeat step note");
            Assert.AreEqual(stepNotesExtensionResult3,
                            placeholderWorkout.STExtensions[placeholderWorkout.STExtensions.Count - 2].OuterXml,
                            "Invalid step notes serialization");
        }

        [Test]
        public void TestStepNotesTCXDeserialization()
        {
            Workout deserializedWorkout = new Workout("TestWorkout", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            XmlDocument testDocument = new XmlDocument();
            RegularStep regularStep;
            RepeatStep repeatStep;

            testDocument.LoadXml(workoutStepNotesExtensionsResult);
            Assert.AreEqual("TrainingCenterDatabase", testDocument.LastChild.Name, "Cannot find database node");
            Assert.AreEqual("Workouts", testDocument.LastChild.FirstChild.Name, "Cannot find workouts node");
            Assert.AreEqual("Workout", testDocument.LastChild.FirstChild.FirstChild.Name, "Cannot find workout node");
            deserializedWorkout.Deserialize(testDocument.LastChild.FirstChild.FirstChild);

            Assert.AreEqual("TestStepNoteExt", deserializedWorkout.Name, "Invalid workout name deserialized");
            Assert.AreEqual(4, deserializedWorkout.StepCount, "Invalid step count deserialized");

            // Regular step
            regularStep = deserializedWorkout.Steps[0] as RegularStep;
            Assert.AreEqual("Test Note1", regularStep.Notes, "Invalid deserialized step note for regular step");
            
            regularStep = deserializedWorkout.Steps[1] as RegularStep;
            Assert.AreEqual("Test Note2", regularStep.Notes, "Invalid deserialized step note for regular step");

            // Repeat step
            repeatStep = deserializedWorkout.Steps[2] as RepeatStep;
            Assert.AreEqual("Test Repeat Note", repeatStep.Notes, "Invalid deserialized step note for repeat step");

            regularStep = repeatStep.StepsToRepeat[0] as RegularStep;
            Assert.AreEqual("", regularStep.Notes, "Invalid deserialized step note for nested regular step");
        }

        const String regularStepTestResult1 = "<StepTest1 xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest1</Name><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></StepTest1>";
        const String regularStepTestResult2 = "<StepTest2 xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest2</Name><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Resting</Intensity><Target xsi:type=\"None_t\" /></StepTest2>";
        const String regularStepTestResult3 = "<StepTest3 xsi:type=\"Step_t\"><StepId>2</StepId><Name>StepTest3</Name><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></StepTest3>";
        const String regularStepTestResult4 = "<StepTest4 xsi:type=\"Step_t\"><StepId>2</StepId><Name>StepTest4</Name><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Resting</Intensity><Target xsi:type=\"None_t\" /></StepTest4>";
        const String regularStepTestResult5 = "<StepTest5 xsi:type=\"Step_t\"><StepId>2</StepId><Name>StepTest5</Name><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></StepTest5>";
        const String regularStepTestResult6 = "<StepTest6 xsi:type=\"Step_t\"><StepId>2</StepId><Name>StepTest6</Name><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Resting</Intensity><Target xsi:type=\"None_t\" /></StepTest6>";

        const String noNameStepTestResult = "<StepTest1 xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></StepTest1>";

        const String stepInvalidResult1 = "<StepTest xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest</Name><Intensity>Resting</Intensity><Target xsi:type=\"None_t\" /></StepTest>";
        const String stepInvalidResult2 = "<StepTest xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest</Name><Duration xsi:type=\"UserInitiated_t\" /><Target xsi:type=\"None_t\" /></StepTest>";
        const String stepInvalidResult3 = "<StepTest xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest</Name><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Resting</Intensity></StepTest>";

        const String regularStepComponentsTestResult = "<StepTest xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest</Name><Duration xsi:type=\"Distance_t\"><Meters>1000</Meters></Duration><Intensity>Resting</Intensity><Target xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"PredefinedHeartRateZone_t\"><Number>1</Number></HeartRateZone></Target></StepTest>";

        const String repeatStepTestResult1 = "<RepeatStepTest1 xsi:type=\"Repeat_t\"><StepId>2</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child></RepeatStepTest1>";
        const String repeatStepTestResult2 = "<RepeatStepTest2 xsi:type=\"Repeat_t\"><StepId>3</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child><Child xsi:type=\"Step_t\"><StepId>2</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child></RepeatStepTest2>";
        const String repeatStepTestResult3 = "<RepeatStepTest3 xsi:type=\"Repeat_t\"><StepId>5</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child><Child xsi:type=\"Step_t\"><StepId>2</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child><Child xsi:type=\"Repeat_t\"><StepId>4</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>3</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child></Child></RepeatStepTest3>";
        const String repeatStepTestResult4 = "<RepeatStepTest4 xsi:type=\"Repeat_t\"><StepId>5</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child><Child xsi:type=\"Step_t\"><StepId>2</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child><Child xsi:type=\"Repeat_t\"><StepId>4</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>3</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child></Child><Child xsi:type=\"Step_t\"><StepId>5</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child></RepeatStepTest4>";

        const String linkStepTestResult1 = "<LinkStepTest1 xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></LinkStepTest1><LinkStepTest1 xsi:type=\"Step_t\"><StepId>2</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></LinkStepTest1><LinkStepTest1 xsi:type=\"Repeat_t\"><StepId>4</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>3</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child></LinkStepTest1>";
        const String linkStepTestResult2 = "<LinkStepTest2 xsi:type=\"Step_t\"><StepId>5</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></LinkStepTest2><LinkStepTest2 xsi:type=\"Step_t\"><StepId>6</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></LinkStepTest2><LinkStepTest2 xsi:type=\"Repeat_t\"><StepId>8</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>7</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child></LinkStepTest2>";
        const String linkStepTestResult3 = "<LinkStepTest3 xsi:type=\"Step_t\"><StepId>2</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></LinkStepTest3><LinkStepTest3 xsi:type=\"Step_t\"><StepId>3</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></LinkStepTest3><LinkStepTest3 xsi:type=\"Repeat_t\"><StepId>5</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>4</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child></LinkStepTest3>";
        const String linkStepTestResult4 = "<LinkStepTest4 Sport=\"Other\"><Name>LinkStepTes-1/2</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><Step xsi:type=\"Step_t\"><StepId>2</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>fa756214-cf71-11db-9705-005056c00008</SportTracksCategory><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes><StepNotes><StepId>2</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></LinkStepTest4><LinkStepTest4 Sport=\"Other\"><Name>LinkStepTes-2/2</Name><Step xsi:type=\"Repeat_t\"><StepId>2</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child></Step><Notes>LinkStepTest4 part 2/2</Notes><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>fa756214-cf71-11db-9705-005056c00008</SportTracksCategory><StepNotes><StepId>2</StepId><Notes></Notes></StepNotes><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></LinkStepTest4>";
        const String linkStepTestResult5 = "<StepId>18</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><Step xsi:type=\"Step_t\"><StepId>19</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>fa756214-cf71-11db-9705-005056c00008</SportTracksCategory><StepNotes><StepId>17</StepId><Notes></Notes></StepNotes><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes><StepNotes><StepId>2</StepId><Notes></Notes></StepNotes><StepNotes><StepId>3</StepId><Notes></Notes></StepNotes><StepNotes><StepId>4</StepId><Notes></Notes></StepNotes><StepNotes><StepId>5</StepId><Notes></Notes></StepNotes><StepNotes><StepId>6</StepId><Notes></Notes></StepNotes><StepNotes><StepId>7</StepId><Notes></Notes></StepNotes><StepNotes><StepId>8</StepId><Notes></Notes></StepNotes><StepNotes><StepId>9</StepId><Notes></Notes></StepNotes><StepNotes><StepId>10</StepId><Notes></Notes></StepNotes><StepNotes><StepId>11</StepId><Notes></Notes></StepNotes><StepNotes><StepId>12</StepId><Notes></Notes></StepNotes><StepNotes><StepId>13</StepId><Notes></Notes></StepNotes><StepNotes><StepId>14</StepId><Notes></Notes></StepNotes><StepNotes><StepId>16</StepId><Notes></Notes></StepNotes><StepNotes><StepId>15</StepId><Notes></Notes></StepNotes><StepNotes><StepId>18</StepId><Notes></Notes></StepNotes><StepNotes><StepId>19</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></LinkStepTest5><LinkStepTest5 Sport=\"Other\"><Name>LinkStepTe1-2/2</Name><Step xsi:type=\"Repeat_t\"><StepId>2</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child></Step><Notes>LinkStepTest5 part 2/2</Notes><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>fa756214-cf71-11db-9705-005056c00008</SportTracksCategory><StepNotes><StepId>2</StepId><Notes></Notes></StepNotes><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes></SportTracksExtensions></Extensions></LinkStepTest5>";
        const String linkStepTestResult6 = "<LinkStepTest6 Sport=\"Other\"><Name>LinkStepTest6</Name><Step xsi:type=\"Repeat_t\"><StepId>6</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child><Child xsi:type=\"Step_t\"><StepId>2</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child><Child xsi:type=\"Step_t\"><StepId>3</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child><Child xsi:type=\"Repeat_t\"><StepId>5</StepId><Repetitions>2</Repetitions><Child xsi:type=\"Step_t\"><StepId>4</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Child></Child></Step>";

        const String stepNotesExtensionResult1 = "<StepNotes><StepId>1</StepId><Notes>This is a note</Notes></StepNotes>";
        const String stepNotesExtensionResult2 = "<StepNotes><StepId>1</StepId><Notes>This is a new note</Notes></StepNotes>";
        const String stepNotesExtensionResult3 = "<StepNotes><StepId>3</StepId><Notes>This is a repeat note</Notes></StepNotes>";

        const String workoutStepNotesExtensionsResult =
@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
<TrainingCenterDatabase xmlns=""http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2 http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd http://www.garmin.com/xmlschemas/WorkoutExtension/v1 http://www.garmin.com/xmlschemas/WorkoutExtensionv1.xsd"">
  <Workouts>
    <Workout Sport=""Other"">
      <Name>TestStepNoteExt</Name>
      <Step xsi:type=""Step_t"">
        <StepId>1</StepId>
        <Duration xsi:type=""UserInitiated_t"" />
        <Intensity>Active</Intensity>
        <Target xsi:type=""None_t"" />
      </Step>
      <Step xsi:type=""Step_t"">
        <StepId>2</StepId>
        <Duration xsi:type=""UserInitiated_t"" />
        <Intensity>Active</Intensity>
        <Target xsi:type=""None_t"" />
      </Step>
      <Step xsi:type=""Repeat_t"">
        <StepId>4</StepId>
        <Repetitions>2</Repetitions>
        <Child xsi:type=""Step_t"">
          <StepId>3</StepId>
          <Duration xsi:type=""UserInitiated_t"" />
          <Intensity>Active</Intensity>
          <Target xsi:type=""None_t"" />
        </Child>
      </Step>
      <Creator xsi:type=""Device_t"">
        <Name />
        <UnitId>1234567890</UnitId>
        <ProductID>0</ProductID>
        <Version>
          <VersionMajor>0</VersionMajor>
          <VersionMinor>0</VersionMinor>
          <BuildMajor>0</BuildMajor>
          <BuildMinor>0</BuildMinor>
        </Version>
      </Creator>
      <Extensions>
        <SportTracksExtensions xmlns=""http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness"">
          <SportTracksCategory>fa756214-cf71-11db-9705-005056c00008</SportTracksCategory>
          <StepNotes>
            <StepId>1</StepId>
            <Notes>Test Note1</Notes>
          </StepNotes>
          <StepNotes>
            <StepId>2</StepId>
            <Notes>
Test Note2
            </Notes>
          </StepNotes>
          <StepNotes>
            <StepId>3</StepId>
            <Notes>
            </Notes>
          </StepNotes>
          <StepNotes>
            <StepId>4</StepId>
            <Notes>Test Repeat Note</Notes>
          </StepNotes>
        </SportTracksExtensions>
      </Extensions>
    </Workout>
  </Workouts>
</TrainingCenterDatabase>";
    }
}
