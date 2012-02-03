using System;
using System.IO;
using System.Text;
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
            bool exportPowerAsFTP = Options.Instance.ExportSportTracksPowerAsPercentFTP;
            int targetPosition1;
            int targetPosition2;

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
            targetPosition1 = testDocument.InnerXml.IndexOf(noTargetResult);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid no target serialization");

            // Cadence targets
            BaseCadenceTarget cadenceTarget = new BaseCadenceTarget(placeholderStep);

            // Cadence range
            cadenceTarget.ConcreteTarget = new CadenceRangeTarget(80, 90, cadenceTarget);
            cadenceTarget.Serialize(database, "CadenceRangeTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(cadenceRangeTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid CadenceRange target serialization");

            cadenceTarget.ConcreteTarget = new CadenceRangeTarget(60, 120, cadenceTarget);
            cadenceTarget.Serialize(database, "CadenceRangeTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(cadenceRangeTargetResult2);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid CadenceRange target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "CadenceRange target serializations don't differ");

            // Cadence ST zone
            cadenceTarget.ConcreteTarget = new CadenceZoneSTTarget(logbook.CadenceZones[0].Zones[2], cadenceTarget);
            cadenceTarget.Serialize(database, "CadenceSTTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(cadenceSTTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid CadenceST target serialization");

            cadenceTarget.ConcreteTarget = new CadenceZoneSTTarget(logbook.CadenceZones[0].Zones[4], cadenceTarget);
            cadenceTarget.Serialize(database, "CadenceSTTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(cadenceSTTargetResult2);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid CadenceST target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "CadenceST target serializations don't differ");

            // Speed targets
            BaseSpeedTarget speedTarget = new BaseSpeedTarget(placeholderStep);

            // Speed range
            placeholderWorkout.Category = logbook.ActivityCategories[0].SubCategories[2];
            speedTarget.ConcreteTarget = new SpeedRangeTarget(20, 30, Length.Units.Kilometer, Speed.Units.Speed, speedTarget);
            speedTarget.Serialize(database, "SpeedRangeTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(speedRangeTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid SpeedRange target serialization");

            speedTarget.ConcreteTarget = new SpeedRangeTarget(20, 30, Length.Units.Mile, Speed.Units.Speed, speedTarget);
            speedTarget.Serialize(database, "SpeedRangeTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(speedRangeTargetResult2);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid SpeedRange target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "SpeedRange target serializations don't differ");

            placeholderWorkout.Category = logbook.ActivityCategories[0].SubCategories[3];
            speedTarget.ConcreteTarget = new SpeedRangeTarget(20, 30, Length.Units.Kilometer, Speed.Units.Speed, speedTarget);
            speedTarget.Serialize(database, "SpeedRangeTarget3", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(speedRangeTargetResult3);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid SpeedRange %Max target serialization");

            speedTarget.ConcreteTarget = new SpeedRangeTarget(7.5, 15, Length.Units.Mile, Speed.Units.Pace, speedTarget);
            speedTarget.Serialize(database, "SpeedRangeTarget4", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(speedRangeTargetResult4);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid SpeedRRange %Max target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "SpeedRange %Max target serializations don't differ");

            // Speed Garmin zone
            speedTarget.ConcreteTarget = new SpeedZoneGTCTarget(1, speedTarget);
            speedTarget.Serialize(database, "SpeedGTCTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(speedGTCTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid SpeedGTC target serialization");

            speedTarget.ConcreteTarget = new SpeedZoneGTCTarget(3, speedTarget);
            speedTarget.Serialize(database, "SpeedGTCTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(speedGTCTargetResult2);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid SpeedGTC target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "SpeedGTC target serializations don't differ");

            // Speed ST zone
            placeholderWorkout.Category = logbook.ActivityCategories[0].SubCategories[2];
            Options.Instance.ExportSportTracksHeartRateAsPercentMax = false;
            speedTarget.ConcreteTarget = new SpeedZoneSTTarget(logbook.SpeedZones[0].Zones[1], speedTarget);
            speedTarget.Serialize(database, "SpeedSTTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(speedSTTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid SpeedRST target serialization");

            speedTarget.ConcreteTarget = new SpeedZoneSTTarget(logbook.SpeedZones[0].Zones[2], speedTarget);
            speedTarget.Serialize(database, "SpeedSTTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(speedSTTargetResult2);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid SpeedRST target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "SpeedRST target serializations don't differ");

            placeholderWorkout.Category = logbook.ActivityCategories[0].SubCategories[3];
            speedTarget.ConcreteTarget = new SpeedZoneSTTarget(logbook.SpeedZones[0].Zones[1], speedTarget);
            speedTarget.Serialize(database, "SpeedSTTarget3", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(speedSTTargetResult3);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid SpeedRST %Max target serialization");

            speedTarget.ConcreteTarget = new SpeedZoneSTTarget(logbook.SpeedZones[0].Zones[2], speedTarget);
            speedTarget.Serialize(database, "SpeedSTTarget4", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(speedSTTargetResult4);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid SpeedRST %Max target serialization");
            Assert.GreaterOrEqual(targetPosition2, targetPosition1, "SpeedST %Max target serializations don't differ");

            // Heart rate targets
            BaseHeartRateTarget hrTarget = new BaseHeartRateTarget(placeholderStep);

            // HR range
            hrTarget.ConcreteTarget = new HeartRateRangeTarget(130, 170, false, hrTarget);
            hrTarget.Serialize(database, "HRRangeTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(hrRangeTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRange target serialization");

            hrTarget.ConcreteTarget = new HeartRateRangeTarget(100, 190, false, hrTarget);
            hrTarget.Serialize(database, "HRRangeTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(hrRangeTargetResult2);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid HRRange target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "HRRange target serializations don't differ");

            hrTarget.ConcreteTarget = new HeartRateRangeTarget(50, 70, true, hrTarget);
            hrTarget.Serialize(database, "HRRangeTarget3", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(hrRangeTargetResult3);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRange %Max target serialization");

            hrTarget.ConcreteTarget = new HeartRateRangeTarget(75, 95, true, hrTarget);
            hrTarget.Serialize(database, "HRRangeTarget4", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(hrRangeTargetResult4);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid HRRange %Max target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "HRRange %Max target serializations don't differ");

            // HR Garmin zone
            hrTarget.ConcreteTarget = new HeartRateZoneGTCTarget(1, hrTarget);
            hrTarget.Serialize(database, "HRGTCTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(hrGTCTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRGTC target serialization");

            hrTarget.ConcreteTarget = new HeartRateZoneGTCTarget(3, hrTarget);
            hrTarget.Serialize(database, "HRGTCTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(hrGTCTargetResult2);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid HRGTC target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "HRGTC target serializations don't differ");

            // HR ST zone
            Options.Instance.ExportSportTracksHeartRateAsPercentMax = false;
            hrTarget.ConcreteTarget = new HeartRateZoneSTTarget(logbook.HeartRateZones[0].Zones[2], hrTarget);
            hrTarget.Serialize(database, "HRSTTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(hrSTTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRST target serialization");

            hrTarget.ConcreteTarget = new HeartRateZoneSTTarget(logbook.HeartRateZones[0].Zones[4], hrTarget);
            hrTarget.Serialize(database, "HRSTTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(hrSTTargetResult2);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid HRRST target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "HRRST target serializations don't differ");

            placeholderWorkout.Category = logbook.ActivityCategories[0].SubCategories[1];
            Options.Instance.ExportSportTracksHeartRateAsPercentMax = true;
            hrTarget.ConcreteTarget = new HeartRateZoneSTTarget(logbook.HeartRateZones[1].Zones[2], hrTarget);
            hrTarget.Serialize(database, "HRSTTarget3", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(hrSTTargetResult3);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRST %Max target serialization");

            hrTarget.ConcreteTarget = new HeartRateZoneSTTarget(logbook.HeartRateZones[1].Zones[4], hrTarget);
            hrTarget.Serialize(database, "HRSTTarget4", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(hrSTTargetResult4);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid HRRST %Max target serialization");
            Assert.GreaterOrEqual(targetPosition2, targetPosition1, "HRST %Max target serializations don't differ");

            // Power targets
            BasePowerTarget powerTarget = new BasePowerTarget(placeholderStep);

            // Power range
            powerTarget.ConcreteTarget = new PowerRangeTarget(150, 200, false, powerTarget);
            powerTarget.Serialize(database, "PowerRangeTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(powerRangeTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid PowerRange target serialization");

            powerTarget.ConcreteTarget = new PowerRangeTarget(300, 400, false, powerTarget);
            powerTarget.Serialize(database, "PowerRangeTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(powerRangeTargetResult2);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid PowerRange target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "PowerRange target serializations don't differ");

            powerTarget.ConcreteTarget = new PowerRangeTarget(67, 80, true, powerTarget);
            powerTarget.Serialize(database, "PowerRangeTarget3", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(powerRangeTargetResult3);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid PowerRange %FTP target serialization");

            powerTarget.ConcreteTarget = new PowerRangeTarget(120, 160, true, powerTarget);
            powerTarget.Serialize(database, "PowerRangeTarget4", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(powerRangeTargetResult4);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid PowerRRange %FTP target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "PowerRange %FTP target serializations don't differ");

            // Power Garmin zone
            powerTarget.ConcreteTarget = new PowerZoneGTCTarget(1, powerTarget);
            powerTarget.Serialize(database, "PowerGTCTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(powerGTCTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid PowerGTC target serialization");

            powerTarget.ConcreteTarget = new PowerZoneGTCTarget(3, powerTarget);
            powerTarget.Serialize(database, "PowerGTCTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(powerGTCTargetResult2);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid PowerGTC target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "PowerGTC target serializations don't differ");

            // Power ST zone
            placeholderWorkout.Category = logbook.ActivityCategories[0].SubCategories[4];
            Options.Instance.ExportSportTracksPowerAsPercentFTP = false;
            powerTarget.ConcreteTarget = new PowerZoneSTTarget(logbook.PowerZones[0].Zones[1], powerTarget);
            powerTarget.Serialize(database, "PowerSTTarget1", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(powerSTTargetResult1);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid PowerST target serialization");

            powerTarget.ConcreteTarget = new PowerZoneSTTarget(logbook.PowerZones[0].Zones[3], powerTarget);
            powerTarget.Serialize(database, "PowerSTTarget2", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(powerSTTargetResult2);
            Assert.GreaterOrEqual(targetPosition2, 0, "Invalid PowerST target serialization");
            Assert.Greater(targetPosition2, targetPosition1, "PowerST target serializations don't differ");

            Options.Instance.ExportSportTracksPowerAsPercentFTP = true;
            powerTarget.ConcreteTarget = new PowerZoneSTTarget(logbook.PowerZones[0].Zones[1], powerTarget);
            powerTarget.Serialize(database, "PowerSTTarget3", testDocument);
            targetPosition1 = testDocument.InnerXml.IndexOf(powerSTTargetResult3);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid PowerST %FTP target serialization");

            powerTarget.ConcreteTarget = new PowerZoneSTTarget(logbook.PowerZones[0].Zones[3], powerTarget);
            powerTarget.Serialize(database, "PowerSTTarget4", testDocument);
            targetPosition2 = testDocument.InnerXml.IndexOf(powerSTTargetResult4);
            Assert.GreaterOrEqual(targetPosition1, 0, "Invalid PowerST %FTP target serialization");
            Assert.GreaterOrEqual(targetPosition2, targetPosition1, "PowerST %FTP target serializations don't differ");

            // Make sure to reset options to previous values
            Options.Instance.ExportSportTracksHeartRateAsPercentMax = exportHRAsMax;
            Options.Instance.ExportSportTracksPowerAsPercentFTP = exportPowerAsFTP;
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

            // No target
            readNode.InnerXml = noTargetResult;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "No target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is NullTarget, "No target wasn't deserialized as proper type");

            // Cadence targets
            BaseCadenceTarget cadenceTarget;

            // Cadence range
            CadenceRangeTarget cadenceRangeTarget;

            readNode.InnerXml = cadenceRangeTargetResult1;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "CadenceRange target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BaseCadenceTarget, "CadenceRange target wasn't deserialized as proper type");
            cadenceTarget = loadedTarget as BaseCadenceTarget;
            Assert.IsTrue(cadenceTarget.ConcreteTarget is CadenceRangeTarget, "CadenceRange target wasn't deserialized as proper concrete type");
            cadenceRangeTarget = cadenceTarget.ConcreteTarget as CadenceRangeTarget;
            Assert.AreEqual(80, cadenceRangeTarget.MinCadence, "CadenceRange min value wasn't properly deserialized");
            Assert.AreEqual(90, cadenceRangeTarget.MaxCadence, "CadenceRange max value wasn't properly deserialized");

            readNode.InnerXml = cadenceRangeTargetResult2;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "CadenceRange target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BaseCadenceTarget, "CadenceRange target wasn't deserialized as proper type");
            cadenceTarget = loadedTarget as BaseCadenceTarget;
            Assert.IsTrue(cadenceTarget.ConcreteTarget is CadenceRangeTarget, "CadenceRange target wasn't deserialized as proper concrete type");
            cadenceRangeTarget = cadenceTarget.ConcreteTarget as CadenceRangeTarget;
            Assert.AreEqual(60, cadenceRangeTarget.MinCadence, "CadenceRange min value wasn't properly deserialized");
            Assert.AreEqual(120, cadenceRangeTarget.MaxCadence, "CadenceRange max value wasn't properly deserialized");

            // Speed targets
            BaseSpeedTarget speedTarget;

            // Speed range
            SpeedRangeTarget speedRangeTarget;
            double speed;

            readNode.InnerXml = speedRangeTargetResult1;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "SpeedRange target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BaseSpeedTarget, "SpeedRange target wasn't deserialized as proper type");
            speedTarget = loadedTarget as BaseSpeedTarget;
            Assert.IsTrue(speedTarget.ConcreteTarget is SpeedRangeTarget, "SpeedRange target wasn't deserialized as proper concrete type");
            speedRangeTarget = speedTarget.ConcreteTarget as SpeedRangeTarget;
            speed = Length.Convert(speedRangeTarget.GetMinSpeedInBaseUnitsPerHour(), speedRangeTarget.BaseUnit, Length.Units.Kilometer);
            Assert.AreEqual(20, speed, STCommon.Data.Constants.Delta, "SpeedRange min value wasn't properly deserialized");
            speed = Length.Convert(speedRangeTarget.GetMaxSpeedInBaseUnitsPerHour(), speedRangeTarget.BaseUnit, Length.Units.Kilometer);
            Assert.AreEqual(30, speed, STCommon.Data.Constants.Delta, "SpeedRange max value wasn't properly deserialized");

            readNode.InnerXml = speedRangeTargetResult4;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "SpeedRange target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BaseSpeedTarget, "SpeedRange target wasn't deserialized as proper type");
            speedTarget = loadedTarget as BaseSpeedTarget;
            Assert.IsTrue(speedTarget.ConcreteTarget is SpeedRangeTarget, "SpeedRange target wasn't deserialized as proper concrete type");
            speedRangeTarget = speedTarget.ConcreteTarget as SpeedRangeTarget;
            speed = Length.Convert(speedRangeTarget.GetMinSpeedInBaseUnitsPerHour(), speedRangeTarget.BaseUnit, Length.Units.Mile);
            Assert.AreEqual(4, speed, STCommon.Data.Constants.Delta, "SpeedRange min value wasn't properly deserialized");
            speed = Length.Convert(speedRangeTarget.GetMaxSpeedInBaseUnitsPerHour(), speedRangeTarget.BaseUnit, Length.Units.Mile);
            Assert.AreEqual(8, speed, STCommon.Data.Constants.Delta, "SpeedRange max value wasn't properly deserialized");
            
            // Speed Garmin zone
            SpeedZoneGTCTarget speedGTCTarget;

            readNode.InnerXml = speedGTCTargetResult1;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "SpeedGTC target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BaseSpeedTarget, "SpeedGTC target wasn't deserialized as proper type");
            speedTarget = loadedTarget as BaseSpeedTarget;
            Assert.IsTrue(speedTarget.ConcreteTarget is SpeedZoneGTCTarget, "SpeedGTC target wasn't deserialized as proper concrete type");
            speedGTCTarget = speedTarget.ConcreteTarget as SpeedZoneGTCTarget;
            Assert.AreEqual(1, speedGTCTarget.Zone, "SpeedGTC zone value wasn't properly deserialized");

            readNode.InnerXml = speedGTCTargetResult2;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "SpeedGTC target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BaseSpeedTarget, "SpeedGTC target wasn't deserialized as proper type");
            speedTarget = loadedTarget as BaseSpeedTarget;
            Assert.IsTrue(speedTarget.ConcreteTarget is SpeedZoneGTCTarget, "SpeedGTC target wasn't deserialized as proper concrete type");
            speedGTCTarget = speedTarget.ConcreteTarget as SpeedZoneGTCTarget;
            Assert.AreEqual(3, speedGTCTarget.Zone, "SpeedGTC zone value wasn't properly deserialized");

            // Heart rate targets
            BaseHeartRateTarget hrTarget;

            // Heart rate range
            HeartRateRangeTarget hrRangeTarget;

            readNode.InnerXml = hrRangeTargetResult1;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "HRRange target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BaseHeartRateTarget, "HRRange target wasn't deserialized as proper type");
            hrTarget = loadedTarget as BaseHeartRateTarget;
            Assert.IsTrue(hrTarget.ConcreteTarget is HeartRateRangeTarget, "HRRange target wasn't deserialized as proper concrete type");
            hrRangeTarget = hrTarget.ConcreteTarget as HeartRateRangeTarget;
            Assert.AreEqual(false, hrRangeTarget.IsPercentMaxHeartRate, "HRRange %Max wasn't properly deserialized");
            Assert.AreEqual(130, hrRangeTarget.MinHeartRate, "HRRange min value wasn't properly deserialized");
            Assert.AreEqual(170, hrRangeTarget.MaxHeartRate, "HRRange max value wasn't properly deserialized");

            readNode.InnerXml = hrRangeTargetResult3;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "HRRange target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BaseHeartRateTarget, "HRRange target wasn't deserialized as proper type");
            hrTarget = loadedTarget as BaseHeartRateTarget;
            Assert.IsTrue(hrTarget.ConcreteTarget is HeartRateRangeTarget, "HRRange target wasn't deserialized as proper concrete type");
            hrRangeTarget = hrTarget.ConcreteTarget as HeartRateRangeTarget;
            Assert.AreEqual(true, hrRangeTarget.IsPercentMaxHeartRate, "HRRange %Max wasn't properly deserialized");
            Assert.AreEqual(50, hrRangeTarget.MinHeartRate, "HRRange min value wasn't properly deserialized");
            Assert.AreEqual(70, hrRangeTarget.MaxHeartRate, "HRRange max value wasn't properly deserialized");

            // Heart rate Garmin zone
            HeartRateZoneGTCTarget hrGTCTarget;

            readNode.InnerXml = hrGTCTargetResult1;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "HRGTC target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BaseHeartRateTarget, "HRGTC target wasn't deserialized as proper type");
            hrTarget = loadedTarget as BaseHeartRateTarget;
            Assert.IsTrue(hrTarget.ConcreteTarget is HeartRateZoneGTCTarget, "HRGTC target wasn't deserialized as proper concrete type");
            hrGTCTarget = hrTarget.ConcreteTarget as HeartRateZoneGTCTarget;
            Assert.AreEqual(1, hrGTCTarget.Zone, "HRGTC zone value wasn't properly deserialized");

            readNode.InnerXml = hrGTCTargetResult2;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "HRGTC target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BaseHeartRateTarget, "HRGTC target wasn't deserialized as proper type");
            hrTarget = loadedTarget as BaseHeartRateTarget;
            Assert.IsTrue(hrTarget.ConcreteTarget is HeartRateZoneGTCTarget, "HRGTC target wasn't deserialized as proper concrete type");
            hrGTCTarget = hrTarget.ConcreteTarget as HeartRateZoneGTCTarget;
            Assert.AreEqual(3, hrGTCTarget.Zone, "HRGTC zone value wasn't properly deserialized");

            // Power targets
            BasePowerTarget powerTarget;

            // Power range
            PowerRangeTarget powerRangeTarget;

            readNode.InnerXml = powerRangeTargetResult1;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "PowerRange target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BasePowerTarget, "PowerRange target wasn't deserialized as proper type");
            powerTarget = loadedTarget as BasePowerTarget;
            Assert.IsTrue(powerTarget.ConcreteTarget is PowerRangeTarget, "PowerRange target wasn't deserialized as proper concrete type");
            powerRangeTarget = powerTarget.ConcreteTarget as PowerRangeTarget;
            Assert.AreEqual(false, powerRangeTarget.IsPercentFTP, "PowerRange %FTP wasn't properly deserialized");
            Assert.AreEqual(150, powerRangeTarget.MinPower, "PowerRange min value wasn't properly deserialized");
            Assert.AreEqual(200, powerRangeTarget.MaxPower, "PowerRange max value wasn't properly deserialized");

            readNode.InnerXml = powerRangeTargetResult3;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "PowerRange target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BasePowerTarget, "PowerRange target wasn't deserialized as proper type");
            powerTarget = loadedTarget as BasePowerTarget;
            Assert.IsTrue(powerTarget.ConcreteTarget is PowerRangeTarget, "PowerRange target wasn't deserialized as proper concrete type");
            powerRangeTarget = powerTarget.ConcreteTarget as PowerRangeTarget;
            Assert.AreEqual(true, powerRangeTarget.IsPercentFTP, "PowerRange %FTP wasn't properly deserialized");
            Assert.AreEqual(67, powerRangeTarget.MinPower, "PowerRange min value wasn't properly deserialized");
            Assert.AreEqual(80, powerRangeTarget.MaxPower, "PowerRange max value wasn't properly deserialized");

            // Power Garmin zone
            PowerZoneGTCTarget powerGTCTarget;

            readNode.InnerXml = powerGTCTargetResult1;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "PowerGTC target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BasePowerTarget, "PowerGTC target wasn't deserialized as proper type");
            powerTarget = loadedTarget as BasePowerTarget;
            Assert.IsTrue(powerTarget.ConcreteTarget is PowerZoneGTCTarget, "PowerGTC target wasn't deserialized as proper concrete type");
            powerGTCTarget = powerTarget.ConcreteTarget as PowerZoneGTCTarget;
            Assert.AreEqual(1, powerGTCTarget.Zone, "PowerGTC zone value wasn't properly deserialized");

            readNode.InnerXml = powerGTCTargetResult2;
            loadedTarget = TargetFactory.Create(readNode.FirstChild, placeholderStep);
            Assert.IsNotNull(loadedTarget, "PowerGTC target wasn't properly deserialized");
            Assert.IsTrue(loadedTarget is BasePowerTarget, "PowerGTC target wasn't deserialized as proper type");
            powerTarget = loadedTarget as BasePowerTarget;
            Assert.IsTrue(powerTarget.ConcreteTarget is PowerZoneGTCTarget, "PowerGTC target wasn't deserialized as proper concrete type");
            powerGTCTarget = powerTarget.ConcreteTarget as PowerZoneGTCTarget;
            Assert.AreEqual(3, powerGTCTarget.Zone, "PowerGTC zone value wasn't properly deserialized");
        }

        [Test]
        public void TestSerializeSTTargetExtensions()
        {
            XmlDocument testDocument = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;
            Workout placeholderWorkout = new Workout("Test", PluginMain.GetApplication().Logbook.ActivityCategories[0]);
            RegularStep placeholderStep = new RegularStep(placeholderWorkout);
            ILogbook logbook = PluginMain.GetApplication().Logbook;

            placeholderWorkout.Steps.AddStepToRoot(placeholderStep);

            // Setup document
            testDocument.AppendChild(testDocument.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = testDocument.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            testDocument.AppendChild(database);
            attribute = testDocument.CreateAttribute("xmlns", "xsi", GarminFitnessPlugin.Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);

            // Cadence ST zone
            BaseCadenceTarget cadenceTarget = new BaseCadenceTarget(placeholderStep);
            cadenceTarget.ConcreteTarget = new CadenceZoneSTTarget(logbook.CadenceZones[0].Zones[2], cadenceTarget);
            cadenceTarget.Serialize(database, "CadenceSTTarget1", testDocument);

            Assert.GreaterOrEqual(placeholderWorkout.STExtensions.Count, 1, "Missing step extension node for CadenceST target");
            Assert.AreEqual(cadenceSTTargetExtensionResult1,
                            placeholderWorkout.STExtensions[placeholderWorkout.STExtensions.Count - 1].OuterXml,
                            "Invalid CadenceST target extension serialization");

            // Speed ST zone
            BaseSpeedTarget speedTarget = new BaseSpeedTarget(placeholderStep);
            speedTarget.ConcreteTarget = new SpeedZoneSTTarget(logbook.SpeedZones[0].Zones[1], speedTarget);
            speedTarget.Serialize(database, "SpeedSTTarget1", testDocument);

            Assert.GreaterOrEqual(placeholderWorkout.STExtensions.Count, 1, "Missing step extension node for SpeedST target");
            Assert.AreEqual(speedSTTargetExtensionResult1,
                            placeholderWorkout.STExtensions[placeholderWorkout.STExtensions.Count - 1].OuterXml,
                            "Invalid SpeedST target extension serialization");

            // HR ST zone
            BaseHeartRateTarget hrTarget = new BaseHeartRateTarget(placeholderStep);
            hrTarget.ConcreteTarget = new HeartRateZoneSTTarget(logbook.HeartRateZones[0].Zones[2], hrTarget);
            hrTarget.Serialize(database, "HRSTTarget1", testDocument);

            placeholderWorkout.Category = logbook.ActivityCategories[0].SubCategories[1];
            hrTarget.ConcreteTarget = new HeartRateZoneSTTarget(logbook.HeartRateZones[1].Zones[2], hrTarget);
            hrTarget.Serialize(database, "HRSTTarget3", testDocument);
            hrTarget.ConcreteTarget = new HeartRateZoneSTTarget(logbook.HeartRateZones[1].Zones[4], hrTarget);
            hrTarget.Serialize(database, "HRSTTarget4", testDocument);

            Assert.GreaterOrEqual(placeholderWorkout.STExtensions.Count, 3, "Missing step extension node for HRST target");
            Assert.AreEqual(hrSTTargetExtensionResult1,
                            placeholderWorkout.STExtensions[placeholderWorkout.STExtensions.Count - 3].OuterXml,
                            "Invalid HRST target extension serialization");
            Assert.AreEqual(hrSTTargetExtensionResult2,
                            placeholderWorkout.STExtensions[placeholderWorkout.STExtensions.Count - 1].OuterXml,
                            "Invalid HRST target extension serialization");
            Assert.AreEqual(placeholderWorkout.STExtensions[placeholderWorkout.STExtensions.Count - 1].LastChild.FirstChild.InnerText,
                            placeholderWorkout.STExtensions[placeholderWorkout.STExtensions.Count - 2].LastChild.FirstChild.InnerText,
                            "Matching HRST target extension are not identical");
            Assert.AreNotEqual(placeholderWorkout.STExtensions[placeholderWorkout.STExtensions.Count - 1].LastChild.FirstChild.InnerText,
                               placeholderWorkout.STExtensions[placeholderWorkout.STExtensions.Count - 3].LastChild.FirstChild.InnerText,
                               "Mismatching HRST target extension are identical");

            // Power ST zone
            BasePowerTarget powerTarget = new BasePowerTarget(placeholderStep);
            powerTarget.ConcreteTarget = new PowerZoneSTTarget(logbook.PowerZones[0].Zones[3], powerTarget);
            powerTarget.Serialize(database, "PowerSTTarget1", testDocument);

            Assert.GreaterOrEqual(placeholderWorkout.STExtensions.Count, 1, "Missing step extension node for PowerST target");
            Assert.AreEqual(powerSTTargetExtensionResult1,
                            placeholderWorkout.STExtensions[placeholderWorkout.STExtensions.Count - 1].OuterXml,
                            "Invalid PowerST target extension serialization");
        }

        [Test]
        public void TestDeserializeSTTargetExtensions()
        {
            ILogbook logbook = PluginMain.GetApplication().Logbook;
            Workout deserializedWorkout = new Workout("TestWorkout", logbook.ActivityCategories[0]);
            XmlDocument testDocument = new XmlDocument();
            RegularStep step;

            testDocument.LoadXml(workoutSTExtensionsResult);
            Assert.AreEqual("TrainingCenterDatabase", testDocument.LastChild.Name, "Cannot find database node");
            Assert.AreEqual("Workouts", testDocument.LastChild.FirstChild.Name, "Cannot find workouts node");
            Assert.AreEqual("Workout", testDocument.LastChild.FirstChild.FirstChild.Name, "Cannot find workout node");
            deserializedWorkout.Deserialize(testDocument.LastChild.FirstChild.FirstChild);

            Assert.AreEqual("TestSTExtension", deserializedWorkout.Name, "Invalid workout name deserialized");
            Assert.AreEqual(4, deserializedWorkout.StepCount, "Invalid step count deserialized");

            // Speed ST zone
            BaseSpeedTarget speedTarget;
            SpeedZoneSTTarget speedSTTarget;

            Assert.IsTrue(deserializedWorkout.Steps[0] is RegularStep, "First step is of invalid type");
            step = deserializedWorkout.Steps[0] as RegularStep;
            Assert.IsTrue(step.Target is BaseSpeedTarget, "First step target is of invalid type");
            speedTarget = step.Target as BaseSpeedTarget;
            Assert.IsTrue(speedTarget.ConcreteTarget is SpeedZoneSTTarget, "Speed concrete target is of invalid type");
            speedSTTarget = speedTarget.ConcreteTarget as SpeedZoneSTTarget;
            Assert.AreEqual(logbook.SpeedZones[0].Zones[3], speedSTTarget.Zone, "Invalid speed ST zone deserialized");

            // Cadence ST zone
            BaseCadenceTarget cadenceTarget;
            CadenceZoneSTTarget cadenceSTTarget;

            Assert.IsTrue(deserializedWorkout.Steps[1] is RegularStep, "Second step is of invalid type");
            step = deserializedWorkout.Steps[1] as RegularStep;
            Assert.IsTrue(step.Target is BaseCadenceTarget, "Second step target is of invalid type");
            cadenceTarget = step.Target as BaseCadenceTarget;
            Assert.IsTrue(cadenceTarget.ConcreteTarget is CadenceZoneSTTarget, "Cadence concrete target is of invalid type");
            cadenceSTTarget = cadenceTarget.ConcreteTarget as CadenceZoneSTTarget;
            Assert.AreEqual(logbook.CadenceZones[0].Zones[4], cadenceSTTarget.Zone, "Invalid cadence ST zone deserialized");

            // HR ST zone
            BaseHeartRateTarget hrTarget;
            HeartRateZoneSTTarget hrSTTarget;

            Assert.IsTrue(deserializedWorkout.Steps[2] is RegularStep, "Third step is of invalid type");
            step = deserializedWorkout.Steps[2] as RegularStep;
            Assert.IsTrue(step.Target is BaseHeartRateTarget, "Third step target is of invalid type");
            hrTarget = step.Target as BaseHeartRateTarget;
            Assert.IsTrue(hrTarget.ConcreteTarget is HeartRateZoneSTTarget, "HR concrete target is of invalid type");
            hrSTTarget = hrTarget.ConcreteTarget as HeartRateZoneSTTarget;
            Assert.AreEqual(logbook.HeartRateZones[0].Zones[3], hrSTTarget.Zone, "Invalid HR ST zone deserialized");

            // Power ST zone
            BasePowerTarget powerTarget;
            PowerZoneSTTarget powerSTTarget;

            Assert.IsTrue(deserializedWorkout.Steps[3] is RegularStep, "Fourth step is of invalid type");
            step = deserializedWorkout.Steps[3] as RegularStep;
            Assert.IsTrue(step.Target is BasePowerTarget, "Fourth step target is of invalid type");
            powerTarget = step.Target as BasePowerTarget;
            Assert.IsTrue(powerTarget.ConcreteTarget is PowerZoneSTTarget, "Power concrete target is of invalid type");
            powerSTTarget = powerTarget.ConcreteTarget as PowerZoneSTTarget;
            Assert.AreEqual(logbook.PowerZones[0].Zones[0], powerSTTarget.Zone, "Invalid power ST zone deserialized");
        }

        [Test]
        public void TestPowerTargetSerialization()
        {
            IWorkout workout = GarminWorkoutManager.Instance.GetWorkout("TestPowerExt");
            MemoryStream stream = new MemoryStream();

            WorkoutExporter.ExportWorkout(workout, stream);
            Assert.AreEqual(workoutPowerExtensionsResult, Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length), "Power target extension not properly serialized");
        }

        const String noTargetResult = "<NoTarget xsi:type=\"None_t\" />";
        const String cadenceRangeTargetResult1 = "<CadenceRangeTarget1 xsi:type=\"Cadence_t\"><Low>80.00000</Low><High>90.00000</High></CadenceRangeTarget1>";
        const String cadenceRangeTargetResult2 = "<CadenceRangeTarget2 xsi:type=\"Cadence_t\"><Low>60.00000</Low><High>120.00000</High></CadenceRangeTarget2>";
        const String cadenceSTTargetResult1 = "<CadenceSTTarget1 xsi:type=\"Cadence_t\"><Low>70.00000</Low><High>85.00000</High></CadenceSTTarget1>";
        const String cadenceSTTargetResult2 = "<CadenceSTTarget2 xsi:type=\"Cadence_t\"><Low>100.00000</Low><High>120.00000</High></CadenceSTTarget2>";
        const String cadenceSTTargetExtensionResult1 = "<TargetOverride><StepId>2</StepId><Category><Id>9b534961-59e8-41c0-8b1d-f47bcb6a5cfd</Id><Index>2</Index></Category></TargetOverride>";

        const String speedRangeTargetResult1 = "<SpeedRangeTarget1 xsi:type=\"Speed_t\"><SpeedZone xsi:type=\"CustomSpeedZone_t\"><ViewAs>Pace</ViewAs><LowInMetersPerSecond>5.55556</LowInMetersPerSecond><HighInMetersPerSecond>8.33333</HighInMetersPerSecond></SpeedZone></SpeedRangeTarget1>";
        const String speedRangeTargetResult2 = "<SpeedRangeTarget2 xsi:type=\"Speed_t\"><SpeedZone xsi:type=\"CustomSpeedZone_t\"><ViewAs>Pace</ViewAs><LowInMetersPerSecond>8.94080</LowInMetersPerSecond><HighInMetersPerSecond>13.41120</HighInMetersPerSecond></SpeedZone></SpeedRangeTarget2>";
        const String speedRangeTargetResult3 = "<SpeedRangeTarget3 xsi:type=\"Speed_t\"><SpeedZone xsi:type=\"CustomSpeedZone_t\"><ViewAs>Speed</ViewAs><LowInMetersPerSecond>5.55556</LowInMetersPerSecond><HighInMetersPerSecond>8.33333</HighInMetersPerSecond></SpeedZone></SpeedRangeTarget3>";
        const String speedRangeTargetResult4 = "<SpeedRangeTarget4 xsi:type=\"Speed_t\"><SpeedZone xsi:type=\"CustomSpeedZone_t\"><ViewAs>Speed</ViewAs><LowInMetersPerSecond>3.57632</LowInMetersPerSecond><HighInMetersPerSecond>1.78816</HighInMetersPerSecond></SpeedZone></SpeedRangeTarget4>";
        const String speedGTCTargetResult1 = "<SpeedGTCTarget1 xsi:type=\"Speed_t\"><SpeedZone xsi:type=\"PredefinedSpeedZone_t\"><Number>1</Number></SpeedZone></SpeedGTCTarget1>";
        const String speedGTCTargetResult2 = "<SpeedGTCTarget2 xsi:type=\"Speed_t\"><SpeedZone xsi:type=\"PredefinedSpeedZone_t\"><Number>3</Number></SpeedZone></SpeedGTCTarget2>";
        const String speedSTTargetResult1 = "<SpeedSTTarget1 xsi:type=\"Speed_t\"><SpeedZone xsi:type=\"CustomSpeedZone_t\"><ViewAs>Pace</ViewAs><LowInMetersPerSecond>2.77778</LowInMetersPerSecond><HighInMetersPerSecond>5.55556</HighInMetersPerSecond></SpeedZone></SpeedSTTarget1>";
        const String speedSTTargetResult2 = "<SpeedSTTarget2 xsi:type=\"Speed_t\"><SpeedZone xsi:type=\"CustomSpeedZone_t\"><ViewAs>Pace</ViewAs><LowInMetersPerSecond>5.55556</LowInMetersPerSecond><HighInMetersPerSecond>8.33333</HighInMetersPerSecond></SpeedZone></SpeedSTTarget2>";
        const String speedSTTargetResult3 = "<SpeedSTTarget3 xsi:type=\"Speed_t\"><SpeedZone xsi:type=\"CustomSpeedZone_t\"><ViewAs>Speed</ViewAs><LowInMetersPerSecond>2.77778</LowInMetersPerSecond><HighInMetersPerSecond>5.55556</HighInMetersPerSecond></SpeedZone></SpeedSTTarget3>";
        const String speedSTTargetResult4 = "<SpeedSTTarget4 xsi:type=\"Speed_t\"><SpeedZone xsi:type=\"CustomSpeedZone_t\"><ViewAs>Speed</ViewAs><LowInMetersPerSecond>5.55556</LowInMetersPerSecond><HighInMetersPerSecond>8.33333</HighInMetersPerSecond></SpeedZone></SpeedSTTarget4>";
        const String speedSTTargetExtensionResult1 = "<TargetOverride><StepId>2</StepId><Category><Id>6d87c9b6-628e-4ad4-9a0a-bc63f95e54ea</Id><Index>1</Index></Category></TargetOverride>";

        const String hrRangeTargetResult1 = "<HRRangeTarget1 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>130</Value></Low><High xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>170</Value></High></HeartRateZone></HRRangeTarget1>";
        const String hrRangeTargetResult2 = "<HRRangeTarget2 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>100</Value></Low><High xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>190</Value></High></HeartRateZone></HRRangeTarget2>";
        const String hrRangeTargetResult3 = "<HRRangeTarget3 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>50</Value></Low><High xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>70</Value></High></HeartRateZone></HRRangeTarget3>";
        const String hrRangeTargetResult4 = "<HRRangeTarget4 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>75</Value></Low><High xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>95</Value></High></HeartRateZone></HRRangeTarget4>";
        const String hrGTCTargetResult1 = "<HRGTCTarget1 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"PredefinedHeartRateZone_t\"><Number>1</Number></HeartRateZone></HRGTCTarget1>";
        const String hrGTCTargetResult2 = "<HRGTCTarget2 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"PredefinedHeartRateZone_t\"><Number>3</Number></HeartRateZone></HRGTCTarget2>";
        const String hrSTTargetResult1 = "<HRSTTarget1 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>140</Value></Low><High xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>160</Value></High></HeartRateZone></HRSTTarget1>";
        const String hrSTTargetResult2 = "<HRSTTarget2 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>180</Value></Low><High xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>240</Value></High></HeartRateZone></HRSTTarget2>";
        const String hrSTTargetResult3 = "<HRSTTarget3 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>68</Value></Low><High xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>82</Value></High></HeartRateZone></HRSTTarget3>";
        const String hrSTTargetResult4 = "<HRSTTarget4 xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>95</Value></Low><High xsi:type=\"HeartRateAsPercentOfMax_t\"><Value>100</Value></High></HeartRateZone></HRSTTarget4>";
        const String hrSTTargetExtensionResult1 = "<TargetOverride><StepId>2</StepId><Category><Id>a73e7a11-b520-40fc-831c-5c8f9a745b75</Id><Index>2</Index></Category></TargetOverride>";
        const String hrSTTargetExtensionResult2 = "<TargetOverride><StepId>2</StepId><Category><Id>13aaa38a-f8c0-4c25-99be-6b06377aa5d3</Id><Index>4</Index></Category></TargetOverride>";

        const String powerRangeTargetResult1 = "<PowerRangeTarget1 xsi:type=\"Power_t\"><PowerZone xsi:type=\"CustomPowerZone_t\"><Low xsi:type=\"PowerInWatts_t\"><Value>150</Value></Low><High xsi:type=\"PowerInWatts_t\"><Value>200</Value></High></PowerZone></PowerRangeTarget1>";
        const String powerRangeTargetResult2 = "<PowerRangeTarget2 xsi:type=\"Power_t\"><PowerZone xsi:type=\"CustomPowerZone_t\"><Low xsi:type=\"PowerInWatts_t\"><Value>300</Value></Low><High xsi:type=\"PowerInWatts_t\"><Value>400</Value></High></PowerZone></PowerRangeTarget2>";
        const String powerRangeTargetResult3 = "<PowerRangeTarget3 xsi:type=\"Power_t\"><PowerZone xsi:type=\"CustomPowerZone_t\"><Low xsi:type=\"PowerAsPercentOfFTP_t\"><Value>67</Value></Low><High xsi:type=\"PowerAsPercentOfFTP_t\"><Value>80</Value></High></PowerZone></PowerRangeTarget3>";
        const String powerRangeTargetResult4 = "<PowerRangeTarget4 xsi:type=\"Power_t\"><PowerZone xsi:type=\"CustomPowerZone_t\"><Low xsi:type=\"PowerAsPercentOfFTP_t\"><Value>120</Value></Low><High xsi:type=\"PowerAsPercentOfFTP_t\"><Value>160</Value></High></PowerZone></PowerRangeTarget4>";
        const String powerGTCTargetResult1 = "<PowerGTCTarget1 xsi:type=\"Power_t\"><PowerZone xsi:type=\"PredefinedPowerZone_t\"><Number>1</Number></PowerZone></PowerGTCTarget1>";
        const String powerGTCTargetResult2 = "<PowerGTCTarget2 xsi:type=\"Power_t\"><PowerZone xsi:type=\"PredefinedPowerZone_t\"><Number>3</Number></PowerZone></PowerGTCTarget2>";
        const String powerSTTargetResult1 = "<PowerSTTarget1 xsi:type=\"Power_t\"><PowerZone xsi:type=\"CustomPowerZone_t\"><Low xsi:type=\"PowerInWatts_t\"><Value>150</Value></Low><High xsi:type=\"PowerInWatts_t\"><Value>200</Value></High></PowerZone></PowerSTTarget1>";
        const String powerSTTargetResult2 = "<PowerSTTarget2 xsi:type=\"Power_t\"><PowerZone xsi:type=\"CustomPowerZone_t\"><Low xsi:type=\"PowerInWatts_t\"><Value>300</Value></Low><High xsi:type=\"PowerInWatts_t\"><Value>400</Value></High></PowerZone></PowerSTTarget2>";
        const String powerSTTargetResult3 = "<PowerSTTarget3 xsi:type=\"Power_t\"><PowerZone xsi:type=\"CustomPowerZone_t\"><Low xsi:type=\"PowerAsPercentOfFTP_t\"><Value>60</Value></Low><High xsi:type=\"PowerAsPercentOfFTP_t\"><Value>80</Value></High></PowerZone></PowerSTTarget3>";
        const String powerSTTargetResult4 = "<PowerSTTarget4 xsi:type=\"Power_t\"><PowerZone xsi:type=\"CustomPowerZone_t\"><Low xsi:type=\"PowerAsPercentOfFTP_t\"><Value>120</Value></Low><High xsi:type=\"PowerAsPercentOfFTP_t\"><Value>160</Value></High></PowerZone></PowerSTTarget4>";
        const String powerSTTargetExtensionResult1 = "<TargetOverride><StepId>2</StepId><Category><Id>922170b1-9b58-49e6-aa44-028936c2ad91</Id><Index>3</Index></Category></TargetOverride>";

        const String workoutPowerExtensionsResult =
@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
<TrainingCenterDatabase xmlns=""http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2 http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd http://www.garmin.com/xmlschemas/WorkoutExtension/v1 http://www.garmin.com/xmlschemas/WorkoutExtensionv1.xsd"">
  <Workouts>
    <Workout Sport=""Other"">
      <Name>TestPowerExt</Name>
      <Step xsi:type=""Step_t"">
        <StepId>1</StepId>
        <Duration xsi:type=""UserInitiated_t"" />
        <Intensity>Active</Intensity>
        <Target xsi:type=""None_t"" />
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
        <Steps xmlns=""http://www.garmin.com/xmlschemas/WorkoutExtension/v1"">
          <Step xsi:type=""Step_t"">
            <StepId>1</StepId>
            <Duration xsi:type=""UserInitiated_t"" />
            <Intensity>Active</Intensity>
            <Target xsi:type=""Power_t"">
              <PowerZone xsi:type=""CustomPowerZone_t"">
                <Low xsi:type=""PowerInWatts_t"">
                  <Value>20</Value>
                </Low>
                <High xsi:type=""PowerInWatts_t"">
                  <Value>150</Value>
                </High>
              </PowerZone>
            </Target>
          </Step>
        </Steps>
        <SportTracksExtensions xmlns=""http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness"">
          <SportTracksCategory>fa756214-cf71-11db-9705-005056c00008</SportTracksCategory>
          <StepNotes>
            <StepId>1</StepId>
            <Notes>
            </Notes>
          </StepNotes>
          <TargetOverride>
            <StepId>1</StepId>
            <Category>
              <Id>922170b1-9b58-49e6-aa44-028936c2ad91</Id>
              <Index>0</Index>
            </Category>
          </TargetOverride>
        </SportTracksExtensions>
      </Extensions>
    </Workout>
  </Workouts>
</TrainingCenterDatabase>";
        const String workoutSTExtensionsResult = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><TrainingCenterDatabase xmlns=\"http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2 http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd http://www.garmin.com/xmlschemas/WorkoutExtension/v1 http://www.garmin.com/xmlschemas/WorkoutExtensionv1.xsd\"><Workouts><Workout Sport=\"Other\"><Name>TestSTExtension</Name><Step xsi:type=\"Step_t\"><StepId>1</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"Speed_t\"><SpeedZone xsi:type=\"CustomSpeedZone_t\"><ViewAs>Pace</ViewAs><LowInMetersPerSecond>8.33333</LowInMetersPerSecond><HighInMetersPerSecond>26.82240</HighInMetersPerSecond></SpeedZone></Target></Step><Step xsi:type=\"Step_t\"><StepId>2</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"Cadence_t\"><Low>100.00000</Low><High>120.00000</High></Target></Step><Step xsi:type=\"Step_t\"><StepId>3</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"HeartRate_t\"><HeartRateZone xsi:type=\"CustomHeartRateZone_t\"><Low xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>160</Value></Low><High xsi:type=\"HeartRateInBeatsPerMinute_t\"><Value>180</Value></High></HeartRateZone></Target></Step><Step xsi:type=\"Step_t\"><StepId>4</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"None_t\" /></Step><Creator xsi:type=\"Device_t\"><Name /><UnitId>1234567890</UnitId><ProductID>0</ProductID><Version><VersionMajor>0</VersionMajor><VersionMinor>0</VersionMinor><BuildMajor>0</BuildMajor><BuildMinor>0</BuildMinor></Version></Creator><Extensions><Steps xmlns=\"http://www.garmin.com/xmlschemas/WorkoutExtension/v1\"><Step xsi:type=\"Step_t\"><StepId>4</StepId><Duration xsi:type=\"UserInitiated_t\" /><Intensity>Active</Intensity><Target xsi:type=\"Power_t\"><PowerZone xsi:type=\"CustomPowerZone_t\"><Low xsi:type=\"PowerInWatts_t\"><Value>20</Value></Low><High xsi:type=\"PowerInWatts_t\"><Value>150</Value></High></PowerZone></Target></Step></Steps><SportTracksExtensions xmlns=\"http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness\"><SportTracksCategory>fa756214-cf71-11db-9705-005056c00008</SportTracksCategory><StepNotes><StepId>1</StepId><Notes></Notes></StepNotes><TargetOverride><StepId>1</StepId><Category><Id>6d87c9b6-628e-4ad4-9a0a-bc63f95e54ea</Id><Index>3</Index></Category></TargetOverride><StepNotes><StepId>2</StepId><Notes></Notes></StepNotes><TargetOverride><StepId>2</StepId><Category><Id>9b534961-59e8-41c0-8b1d-f47bcb6a5cfd</Id><Index>4</Index></Category></TargetOverride><StepNotes><StepId>3</StepId><Notes></Notes></StepNotes><TargetOverride><StepId>3</StepId><Category><Id>a73e7a11-b520-40fc-831c-5c8f9a745b75</Id><Index>3</Index></Category></TargetOverride><StepNotes><StepId>4</StepId><Notes></Notes></StepNotes><TargetOverride><StepId>4</StepId><Category><Id>922170b1-9b58-49e6-aa44-028936c2ad91</Id><Index>0</Index></Category></TargetOverride></SportTracksExtensions></Extensions></Workout></Workouts></TrainingCenterDatabase>";
    }
}
