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
            RegularStep placeholderStep = new RegularStep(placeholderWorkout);
            ILogbook logbook = PluginMain.GetApplication().Logbook;
            bool exportHRAsMax = Options.Instance.ExportSportTracksHeartRateAsPercentMax;
            bool exportPowerAsFTP = Options.Instance.ExportSportTracksPowerAsPercentFTP;
            FITMessage serializedMessage = new FITMessage(FITGlobalMessageIds.WorkoutStep);
            FITMessageField messageField;

            // This is required to determine the step's id in the workout during serialization
            placeholderWorkout.Steps.AddStepToRoot(placeholderStep);

            // Active step
            placeholderStep.Name = "TestStep1";
            placeholderStep.Intensity = RegularStep.StepIntensity.Active;
            placeholderStep.FillFITStepMessage(serializedMessage);
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
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.StepName);
            Assert.IsNotNull(messageField, "Invalid active step name FIT serialization");
            Assert.AreEqual("TestStep2", messageField.GetString(), "Invalid name in field for rest step");
            messageField = serializedMessage.GetField((Byte)FITWorkoutStepFieldIds.Intensity);
            Assert.IsNotNull(messageField, "Invalid active step intensity FIT serialization");
            Assert.AreEqual(FITWorkoutStepIntensity.Rest, (FITWorkoutStepIntensity)messageField.GetEnum(), "Invalid intensity in field for rest step");
            serializedMessage.Clear();

            // Warmup step
            placeholderStep.Name = "TestStep3";
            placeholderStep.Intensity = RegularStep.StepIntensity.Warmup;
            placeholderStep.FillFITStepMessage(serializedMessage);
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
            Assert.Fail("Not Implemented");
        }

        [Test]
        public void TestRepeatStepTCXDeserialization()
        {
            Assert.Fail("Not Implemented");
        }

        [Test]
        public void TestRepeatStepFITSerialization()
        {
            Assert.Fail("Not Implemented");
        }

        [Test]
        public void TestRepeatStepFITDeserialization()
        {
            Assert.Fail("Not Implemented");
        }

        [Test]
        public void TestStepNotesTCXSerialization()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep regularStep = placeholderWorkout.Steps[0] as RegularStep;
            ILogbook logbook = PluginMain.GetApplication().Logbook;

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
            ILogbook logbook = PluginMain.GetApplication().Logbook;
            Workout deserializedWorkout = new Workout("TestWorkout", logbook.ActivityCategories[0]);
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
        const String regularStepTestResult3 = "<StepTest3 xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest3</Name><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></StepTest3>";
        const String regularStepTestResult4 = "<StepTest4 xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest4</Name><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Resting</Intensity><Target xsi:type=\"None_t\" /></StepTest4>";
        const String regularStepTestResult5 = "<StepTest5 xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest5</Name><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></StepTest5>";
        const String regularStepTestResult6 = "<StepTest6 xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest6</Name><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Resting</Intensity><Target xsi:type=\"None_t\" /></StepTest6>";

        const String noNameStepTestResult = "<StepTest1 xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></StepTest1>";

        const String stepInvalidResult1 = "<StepTest xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest</Name><Intensity>Resting</Intensity><Target xsi:type=\"None_t\" /></StepTest>";
        const String stepInvalidResult2 = "<StepTest xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest</Name><Duration xsi:type=\"UserInitiated_t\" /><Target xsi:type=\"None_t\" /></StepTest>";
        const String stepInvalidResult3 = "<StepTest xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest</Name><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Resting</Intensity></StepTest>";

        const String regularStepComponentsTestResult = "<StepTest xsi:type=\"Step_t\"><StepId>1</StepId><Name>StepTest</Name><Duration xsi:type=\"Distance_t\"><Meters>1000</Meters></Duration><Intensity>Resting</Intensity><Target xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"PredefinedHeartRateZone_t\"><Number>1</Number></HeartRateZone></Target></StepTest>";

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
