using System;
using NUnit.Framework;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.Data;

namespace GarminFitnessUnitTests
{
    [TestFixture]
    class PreconditionsTests
    {
        [Test]
        public void Preconditions()
        {
            ILogbook logbook = PluginMain.GetApplication().Logbook;
            
            // Validate logbook name 
            Assert.IsTrue(logbook.FileLocation.EndsWith("GarminFitnessTests.logbook3"));

            // Validate categories
            Assert.GreaterOrEqual(logbook.ActivityCategories[0].SubCategories.Count, 5, "Expecting more activity categories");
            Assert.AreEqual("HRTest", logbook.ActivityCategories[0].SubCategories[0].Name, "Invalid name for HR test category");
            Assert.AreEqual("HRTest2", logbook.ActivityCategories[0].SubCategories[1].Name, "Invalid name for HR test category");
            Assert.AreEqual("SpeedTest", logbook.ActivityCategories[0].SubCategories[2].Name, "Invalid name for Speed test category");
            Assert.AreEqual("SpeedTest2", logbook.ActivityCategories[0].SubCategories[3].Name, "Invalid name for Speed test category");
            Assert.AreEqual(Speed.Units.Pace, logbook.ActivityCategories[0].SubCategories[2].SpeedUnits, "Invalid speed unit for Speed test category");
            Assert.AreEqual(Speed.Units.Speed, logbook.ActivityCategories[0].SubCategories[3].SpeedUnits, "Invalid speed unit for Speed test category");
            Assert.AreEqual("PowerTest", logbook.ActivityCategories[0].SubCategories[4].Name, "Invalid name for Power test category");
            Assert.AreEqual("CyclingCategory", logbook.ActivityCategories[0].SubCategories[5].Name, "Invalid name for Cycling test category");
            Assert.AreEqual(GarminCategories.Biking, Options.Instance.GetGarminCategory(logbook.ActivityCategories[0].SubCategories[5]), "Invalid Garmin category name for Cycling test category");
            Assert.AreEqual("RunningCategory", logbook.ActivityCategories[0].SubCategories[6].Name, "Invalid name for Running test category");
            Assert.AreEqual(GarminCategories.Running, Options.Instance.GetGarminCategory(logbook.ActivityCategories[0].SubCategories[6]), "Invalid Garmin category name for Running test category");

            // Validate profile max HR for categories
            Assert.AreEqual(190,
                            GarminProfileManager.Instance.UserProfile.GetProfileForActivity(GarminCategories.Running).MaximumHeartRate,
                            "Invalid 'running' max HR");
            Assert.AreEqual(190,
                            GarminProfileManager.Instance.UserProfile.GetProfileForActivity(GarminCategories.Biking).MaximumHeartRate,
                            "Invalid 'cycling' max HR");
            Assert.AreEqual(190,
                            GarminProfileManager.Instance.UserProfile.GetProfileForActivity(GarminCategories.Other).MaximumHeartRate,
                            "Invalid 'other' max HR");

            // Validate profile FTP
            Assert.AreEqual(250,
                            ((GarminBikingActivityProfile)GarminProfileManager.Instance.UserProfile.GetProfileForActivity(GarminCategories.Biking)).FTP,
                            "Invalid 'cycling' FTP power");

            // Validate cadence zones used in tests
            Assert.GreaterOrEqual(logbook.CadenceZones.Count, 1, "Missing cadence zones");
            Assert.AreEqual("Cadence Test Zones", logbook.CadenceZones[0].Name, "Cadence category 0 doesn't have proper name");
            Assert.AreEqual(6, logbook.CadenceZones[0].Zones.Count, "Cadence category 0 doesn't have proper zone count");
            Assert.AreEqual(0, logbook.CadenceZones[0].Zones[0].Low, "Cadence category 0 values mismatch");
            Assert.AreEqual(55, logbook.CadenceZones[0].Zones[1].Low, "Cadence category 0 values mismatch");
            Assert.AreEqual(70, logbook.CadenceZones[0].Zones[2].Low, "Cadence category 0 values mismatch");
            Assert.AreEqual(85, logbook.CadenceZones[0].Zones[3].Low, "Cadence category 0 values mismatch");
            Assert.AreEqual(100, logbook.CadenceZones[0].Zones[4].Low, "Cadence category 0 values mismatch");
            Assert.AreEqual(120, logbook.CadenceZones[0].Zones[5].Low, "Cadence category 0 values mismatch");

            // Validate speed zones used in tests
            Assert.GreaterOrEqual(logbook.SpeedZones.Count, 1, "Missing speed zones");
            Assert.AreEqual("Speed Test Zones", logbook.SpeedZones[0].Name, "Speed category 0 doesn't have proper name");
            Assert.AreEqual(4, logbook.SpeedZones[0].Zones.Count, "Speed category 0 doesn't have proper zone count");
            Assert.AreEqual(0, logbook.SpeedZones[0].Zones[0].Low, "Speed category 0 values mismatch");
            Assert.AreEqual(2.77777767, logbook.SpeedZones[0].Zones[1].Low, STCommon.Data.Constants.Delta, "Speed category 0 values mismatch");
            Assert.AreEqual(5.55555534, logbook.SpeedZones[0].Zones[2].Low, STCommon.Data.Constants.Delta, "Speed category 0 values mismatch");
            Assert.AreEqual(8.333333, logbook.SpeedZones[0].Zones[3].Low, STCommon.Data.Constants.Delta, "Speed category 0 values mismatch");

            // Validate HR zones used in tests
            Assert.GreaterOrEqual(logbook.HeartRateZones.Count, 2, "Missing heart rate zones");
            Assert.AreEqual("HR Test Zones", logbook.HeartRateZones[0].Name, "Heart rate category 0 doesn't have proper name");
            Assert.AreEqual(5, logbook.HeartRateZones[0].Zones.Count, "Heart rate category 0 doesn't have proper zone count");
            Assert.AreEqual(0, logbook.HeartRateZones[0].Zones[0].Low, "Heart rate category 0 values mismatch");
            Assert.AreEqual(120, logbook.HeartRateZones[0].Zones[1].Low, "Heart rate category 0 values mismatch");
            Assert.AreEqual(140, logbook.HeartRateZones[0].Zones[2].Low, "Heart rate category 0 values mismatch");
            Assert.AreEqual(160, logbook.HeartRateZones[0].Zones[3].Low, "Heart rate category 0 values mismatch");
            Assert.AreEqual(180, logbook.HeartRateZones[0].Zones[4].Low, "Heart rate category 0 values mismatch");

            Assert.AreEqual("HR Test Zones 2", logbook.HeartRateZones[1].Name, "Heart rate category 1 doesn't have proper name");
            Assert.AreEqual(5, logbook.HeartRateZones[1].Zones.Count, "Heart rate category 1 doesn't have proper zone count");
            Assert.AreEqual(0, logbook.HeartRateZones[1].Zones[0].Low, "Heart rate category 1 values mismatch");
            Assert.AreEqual(105, logbook.HeartRateZones[1].Zones[1].Low, "Heart rate category 1 values mismatch");
            Assert.AreEqual(130, logbook.HeartRateZones[1].Zones[2].Low, "Heart rate category 1 values mismatch");
            Assert.AreEqual(155, logbook.HeartRateZones[1].Zones[3].Low, "Heart rate category 1 values mismatch");
            Assert.AreEqual(180, logbook.HeartRateZones[1].Zones[4].Low, "Heart rate category 1 values mismatch");

            // Validate Power zones used in tests
            Assert.GreaterOrEqual(logbook.PowerZones.Count, 1, "Missing power zones");
            Assert.AreEqual("Power Test Zones", logbook.PowerZones[0].Name, "Power category 0 doesn't have proper name");
            Assert.AreEqual(5, logbook.PowerZones[0].Zones.Count, "Power category 0 doesn't have proper zone count");
            Assert.AreEqual(0, logbook.PowerZones[0].Zones[0].Low, "Power category 0 values mismatch");
            Assert.AreEqual(150, logbook.PowerZones[0].Zones[1].Low, "Power category 0 values mismatch");
            Assert.AreEqual(200, logbook.PowerZones[0].Zones[2].Low, "Power category 0 values mismatch");
            Assert.AreEqual(300, logbook.PowerZones[0].Zones[3].Low, "Power category 0 values mismatch");
            Assert.AreEqual(400, logbook.PowerZones[0].Zones[4].Low, "Power category 0 values mismatch");

            Assert.IsNotNull(GarminWorkoutManager.Instance.GetWorkout("TestPowerExt"), "Cannot find required workout named TestPowerExt");
            Assert.IsNotNull(GarminWorkoutManager.Instance.GetWorkout("LinkStep1"), "Cannot find required workout named LinkStep1");
            Assert.IsNotNull(GarminWorkoutManager.Instance.GetWorkout("LinkStep2"), "Cannot find required workout named LinkStep2");
            Assert.IsTrue(GarminWorkoutManager.Instance.GetWorkout("LinkStep2").Steps[2].ForceSplitOnStep, "Workout named LinkStep2 contains invalid step");
            Assert.IsNotNull(GarminWorkoutManager.Instance.GetWorkout("LinkStepTest1"), "Cannot find required workout named LinkStepTest1");
            Assert.IsTrue(GarminWorkoutManager.Instance.GetWorkout("LinkStepTest1").Steps[0] is WorkoutLinkStep, "Workout named LinkStepTest1 contains invalid step");
            Assert.IsNotNull(GarminWorkoutManager.Instance.GetWorkout("LinkStepTest2"), "Cannot find required workout named LinkStepTest2");
            Assert.IsTrue(GarminWorkoutManager.Instance.GetWorkout("LinkStepTest2").Steps[3] is WorkoutLinkStep, "Workout named LinkStepTest2 contains invalid step");
            Assert.IsNotNull(GarminWorkoutManager.Instance.GetWorkout("LinkStepTest3"), "Cannot find required workout named LinkStepTest3");
            Assert.IsTrue(GarminWorkoutManager.Instance.GetWorkout("LinkStepTest3").Steps[1] is RepeatStep, "Workout named LinkStepTest3 contains invalid step");
            Assert.IsTrue((GarminWorkoutManager.Instance.GetWorkout("LinkStepTest3").Steps[1] as RepeatStep).StepsToRepeat[0] is WorkoutLinkStep, "Workout named LinkStepTest3 contains invalid step");
            Assert.IsNotNull(GarminWorkoutManager.Instance.GetWorkout("LinkStepTest4"), "Cannot find required workout named LinkStepTest4");
            Assert.IsTrue(GarminWorkoutManager.Instance.GetWorkout("LinkStepTest4").Steps[0] is WorkoutLinkStep, "Workout named LinkStepTest4 contains invalid step");
            Assert.IsNotNull(GarminWorkoutManager.Instance.GetWorkout("LinkStepTest5"), "Cannot find required workout named LinkStepTest5");
            Assert.IsTrue(GarminWorkoutManager.Instance.GetWorkout("LinkStepTest5").Steps[1] is WorkoutLinkStep, "Workout named LinkStepTest5 contains invalid step");
            Assert.IsNotNull(GarminWorkoutManager.Instance.GetWorkout("LinkStepTest6"), "Cannot find required workout named LinkStepTest6");
            Assert.IsTrue(GarminWorkoutManager.Instance.GetWorkout("LinkStepTest6").Steps[0] is RepeatStep, "Workout named LinkStepTest6 contains invalid step");
            Assert.IsTrue((GarminWorkoutManager.Instance.GetWorkout("LinkStepTest6").Steps[0] as RepeatStep).StepsToRepeat[1] is WorkoutLinkStep, "Workout named LinkStepTest6 contains invalid step");
        }
    }
}
