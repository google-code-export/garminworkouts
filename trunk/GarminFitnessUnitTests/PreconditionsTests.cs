using System;
using NUnit.Framework;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin;

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

            // Validate HR zones used in tests
            Assert.Greater(logbook.HeartRateZones.Count, 0, "Missing heart rate zones");
            Assert.AreEqual("HR Test Zones", logbook.HeartRateZones[0].Name, "Heart rate category 0 doesn't have proper name");
            Assert.AreEqual(5, logbook.HeartRateZones[0].Zones.Count, "Heart rate category 0 doesn't have proper zone count");
            Assert.AreEqual(0, logbook.HeartRateZones[0].Zones[0].Low, "Heart rate category 0 values mismatch");
            Assert.AreEqual(120, logbook.HeartRateZones[0].Zones[1].Low, "Heart rate category 0 values mismatch");
            Assert.AreEqual(140, logbook.HeartRateZones[0].Zones[2].Low, "Heart rate category 0 values mismatch");
            Assert.AreEqual(160, logbook.HeartRateZones[0].Zones[3].Low, "Heart rate category 0 values mismatch");
            Assert.AreEqual(180, logbook.HeartRateZones[0].Zones[4].Low, "Heart rate category 0 values mismatch");            
        }
    }
}
