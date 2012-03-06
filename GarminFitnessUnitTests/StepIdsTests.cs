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
            RepeatStep repeatStep = new RepeatStep(placeholderWorkout);

            // - Root
            //  - Regular step (id = 1)
            //  - Repeat step (id = 6)
            //   - Regular step (id = 2)
            //   - Repeat step (id = 4)
            //    - Regular step (id = 3)
            //   - Regular step (id = 5)
            //  - Regular step (id = 7)
            placeholderWorkout.Steps.AddStepToRoot(repeatStep);
            repeatStep.StepsToRepeat.Add(new RepeatStep(placeholderWorkout));
            repeatStep.StepsToRepeat.Add(new RegularStep(placeholderWorkout));
            placeholderWorkout.Steps.AddStepToRoot(new RegularStep(placeholderWorkout));

            Assert.AreEqual(1, placeholderWorkout.GetStepExportId(placeholderWorkout.Steps[0]), "Invalid step id for regular step");
            Assert.AreEqual(6, placeholderWorkout.GetStepExportId(placeholderWorkout.Steps[1]), "Invalid step id for repeat step");
            Assert.AreEqual(2, placeholderWorkout.GetStepExportId(repeatStep.StepsToRepeat[0]), "Invalid step id for nested regular step");
            Assert.AreEqual(4, placeholderWorkout.GetStepExportId(repeatStep.StepsToRepeat[1]), "Invalid step id for nested repeat step");
            Assert.AreEqual(5, placeholderWorkout.GetStepExportId(repeatStep.StepsToRepeat[2]), "Invalid step id for nested multiple regular steps");
            repeatStep = repeatStep.StepsToRepeat[1] as RepeatStep;
            Assert.AreEqual(3, placeholderWorkout.GetStepExportId(repeatStep.StepsToRepeat[0]), "Invalid step id for multiple nested regular steps");
            Assert.AreEqual(7, placeholderWorkout.GetStepExportId(placeholderWorkout.Steps[2]), "Invalid  for multiple regular steps");

            Assert.Fail("Link step ids not tested");
        }
    }
}
