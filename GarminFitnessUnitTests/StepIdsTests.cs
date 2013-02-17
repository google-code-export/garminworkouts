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
    class StepIdsTests
    {
        [Test]
        public void TestStepIds()
        {
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep regularStep = placeholderWorkout.Steps[0] as RegularStep;
            RepeatStep repeatStep;
            WorkoutLinkStep linkStep;

            // - Root
            //  - Regular step (id = 1)
            //  - Repeat step (id = 6)
            //   - Regular step (id = 2)
            //   - Repeat step (id = 4)
            //    - Regular step (id = 3)
            //   - Regular step (id = 5)
            //  - Regular step (id = 7)
            //  - Link step (LinkStep1)
            //   - RegularStep (id = 8)
            //   - RegularStep (id = 9)
            //   - Repeat step (id = 11)
            //    - Regular step (id = 10)
            //  - Link step (LinkStep2)
            //   - RegularStep (id = 12)
            //   - RegularStep (id = 13)
            //
            //   - Repeat step (id = 2) -> Force split
            //    - Regular step (id = 1)
            //  - Repeat step (id = 7)
            //   - Link step (LinkStep2)
            //    - RegularStep (id = 3)
            //    - RegularStep (id = 4)
            //    - Repeat step (id = 6) -> Force split ignored inside a repeat
            //     - Regular step (id = 5)
            repeatStep = new RepeatStep(placeholderWorkout);
            placeholderWorkout.Steps.AddStepToRoot(repeatStep);
            repeatStep.StepsToRepeat.Add(new RepeatStep(placeholderWorkout));
            repeatStep.StepsToRepeat.Add(new RegularStep(placeholderWorkout));
            placeholderWorkout.Steps.AddStepToRoot(new RegularStep(placeholderWorkout));
            linkStep = new WorkoutLinkStep(placeholderWorkout, GarminWorkoutManager.Instance.GetWorkout("LinkStep1"));
            placeholderWorkout.Steps.AddStepToRoot(linkStep);
            linkStep = new WorkoutLinkStep(placeholderWorkout, GarminWorkoutManager.Instance.GetWorkout("LinkStep2"));
            placeholderWorkout.Steps.AddStepToRoot(linkStep);
            repeatStep = new RepeatStep(placeholderWorkout);
            placeholderWorkout.Steps.AddStepToRoot(repeatStep);
            repeatStep.StepsToRepeat.Add(new WorkoutLinkStep(placeholderWorkout, GarminWorkoutManager.Instance.GetWorkout("LinkStep2")));
            repeatStep.StepsToRepeat.Remove(repeatStep.StepsToRepeat[0]);

            Assert.AreEqual(1, placeholderWorkout.GetStepExportId(placeholderWorkout.Steps[0]), "Invalid step id for regular step");
            repeatStep = placeholderWorkout.Steps[1] as RepeatStep;
            Assert.AreEqual(6, placeholderWorkout.GetStepExportId(repeatStep), "Invalid step id for repeat step");
            Assert.AreEqual(2, placeholderWorkout.GetStepExportId(repeatStep.StepsToRepeat[0]), "Invalid step id for nested regular step");
            Assert.AreEqual(4, placeholderWorkout.GetStepExportId(repeatStep.StepsToRepeat[1]), "Invalid step id for nested repeat step");
            Assert.AreEqual(5, placeholderWorkout.GetStepExportId(repeatStep.StepsToRepeat[2]), "Invalid step id for nested multiple regular steps");
            repeatStep = repeatStep.StepsToRepeat[1] as RepeatStep;
            Assert.AreEqual(3, placeholderWorkout.GetStepExportId(repeatStep.StepsToRepeat[0]), "Invalid step id for multiple nested regular steps");
            Assert.AreEqual(7, placeholderWorkout.GetStepExportId(placeholderWorkout.Steps[2]), "Invalid step id for multiple regular steps");
            linkStep = placeholderWorkout.Steps[3] as WorkoutLinkStep;
            Assert.AreEqual(placeholderWorkout.GetStepExportId(linkStep), placeholderWorkout.GetStepExportId(linkStep.LinkedWorkoutSteps[0]), "Invalid step id for link step");
            Assert.AreEqual(8, placeholderWorkout.GetStepExportId(linkStep.LinkedWorkoutSteps[0]), "Invalid step id for link nested regular step");
            Assert.AreEqual(9, placeholderWorkout.GetStepExportId(linkStep.LinkedWorkoutSteps[1]), "Invalid step id for link nested regular step");
            repeatStep = linkStep.LinkedWorkoutSteps[2] as RepeatStep;
            Assert.AreEqual(11, placeholderWorkout.GetStepExportId(repeatStep), "Invalid step id for link nested repeat step");
            Assert.AreEqual(10, placeholderWorkout.GetStepExportId(repeatStep.StepsToRepeat[0]), "Invalid step id for link nested repeat nested regular step");
            linkStep = placeholderWorkout.Steps[4] as WorkoutLinkStep;
            Assert.AreEqual(12, placeholderWorkout.GetStepExportId(linkStep.LinkedWorkoutSteps[0]), "Invalid step id for link nested regular step");
            Assert.AreEqual(13, placeholderWorkout.GetStepExportId(linkStep.LinkedWorkoutSteps[1]), "Invalid step id for link nested regular step");
            repeatStep = linkStep.LinkedWorkoutSteps[2] as RepeatStep;
            Assert.AreEqual(2, placeholderWorkout.GetStepExportId(repeatStep), "Invalid step id for link nested repeat step with forced split");
            Assert.AreEqual(1, placeholderWorkout.GetStepExportId(repeatStep.StepsToRepeat[0]), "Invalid step id for link nested repeat nested regular step with forced split");
            repeatStep = placeholderWorkout.Steps[5] as RepeatStep;
            Assert.AreEqual(7, placeholderWorkout.GetStepExportId(repeatStep), "Invalid step id for repeat step after forced split");
            linkStep = repeatStep.StepsToRepeat[0] as WorkoutLinkStep;
            Assert.AreEqual(3, placeholderWorkout.GetStepExportId(linkStep.LinkedWorkoutSteps[0]), "Invalid step id for repeat nested link step with regular step");
            Assert.AreEqual(4, placeholderWorkout.GetStepExportId(linkStep.LinkedWorkoutSteps[1]), "Invalid step id for repeat nested link step with regular step");
            repeatStep = linkStep.LinkedWorkoutSteps[2] as RepeatStep;
            Assert.AreEqual(6, placeholderWorkout.GetStepExportId(repeatStep), "Invalid step id for repeat nested link step with repeat step with forced split");
            Assert.AreEqual(5, placeholderWorkout.GetStepExportId(repeatStep.StepsToRepeat[0]), "Invalid step id for repeat nested link step with regular step with forced split");

            Workout workout = GarminWorkoutManager.Instance.GetWorkout("LinkStepTest5");
            linkStep = workout.Steps[1] as WorkoutLinkStep;
            repeatStep = linkStep.LinkedWorkoutSteps[2] as RepeatStep;
            Assert.AreEqual(2, workout.GetStepExportId(repeatStep), "Invalid step id for link nested repeat step with step count split");
            Assert.AreEqual(1, workout.GetStepExportId(repeatStep.StepsToRepeat[0]), "Invalid step id for link nested repeat nested regular step with step count split");

            try
            {
                Assert.AreEqual(2, placeholderWorkout.GetStepExportId(repeatStep), "Invalid step id for link nested repeat step with step count split");
                Assert.Fail("Expected exception when trying to get step id from wrong workout");
            }
            catch (System.Exception)
            {
            }
        }
    }
}
