using System;
using GarminFitnessPlugin.Data;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin
{
    class Constants
    {
        public static readonly double Delta = 0.00005;

        public static readonly UInt16 SecondsPerMinute = 60;
        public static readonly UInt16 MinutesPerHour = 60;
        public static readonly UInt16 SecondsPerHour = (UInt16)(MinutesPerHour * SecondsPerMinute);

        public static readonly Byte MaxStepsPerWorkout = 20;

        // HR limits
        public static readonly Byte MinHRInBPM = 30;
        public static readonly Byte MaxHRInBPM = 240;
        public static readonly Byte MinHRInPercentMax = 0;
        public static readonly Byte MaxHRInPercentMax = 100;

        // Distance limits
        public static readonly double MinDistance = 0.01;
        public static readonly double MaxDistanceStatute = 40.0;
        public static readonly double MaxDistanceMetric = 65.0;
        public static readonly UInt16 MinDistanceMeters = 1;
        public static readonly UInt16 MaxDistanceMeters = 65000;

        // Time limits
        public static readonly UInt16 MinTime = UInt16.MinValue;
        public static readonly UInt16 MaxTime = 64799;

        // Calories limits
        public static readonly UInt16 MinCalories = UInt16.MinValue;
        public static readonly UInt16 MaxCalories = UInt16.MaxValue;

        // Cadence limits
        public static readonly Byte MinCadence = Byte.MinValue;
        public static readonly Byte MaxCadence = Byte.MaxValue;

        // Speed limits
        public static readonly double MinSpeedStatute = 1.0;
        public static readonly double MaxSpeedStatute = 60.0;
        public static readonly double MinSpeedMetric = Length.Convert(MinSpeedStatute, Length.Units.Mile, Length.Units.Kilometer);
        public static readonly double MaxSpeedMetric = Length.Convert(MaxSpeedStatute, Length.Units.Mile, Length.Units.Kilometer);
        public static readonly double MinSpeedMetersPerSecond = 0.44722;
        public static readonly double MaxSpeedMetersPerSecond = 26.8222;

        // Pace limits
        public static readonly double MinPaceStatute = MinutesPerHour / MaxSpeedStatute;
        public static readonly double MaxPaceStatute = MinutesPerHour / MinSpeedStatute;
        public static readonly double MinPaceMetric = Utils.TimeToFloat("0:37");
        public static readonly double MaxPaceMetric = Utils.TimeToFloat("37:16");

        // Power limits
        public static readonly UInt16 MinPower = 20;
        public static readonly UInt16 MaxPowerWorkout = 999;
        public static readonly UInt16 MaxPowerProfile = 3999;

        // Repeat limits
        public static readonly Byte MinRepeats = 2;
        public static readonly Byte MaxRepeats = 99;

        // Profile limits
        public static readonly double MinWeight = 0;
        public static readonly double MaxWeight = 65535;

        public static readonly Byte GarminHRZoneCount = 5;
        public static readonly Byte GarminSpeedZoneCount = 10;
        public static readonly Byte GarminPowerZoneCount = 7;
        public static readonly Byte GarminBikeProfileCount = 3;

        public static readonly double MinOdometer = 0;
        public static readonly double MaxOdometer = 65535;
        public static readonly double MaxOdometerMeters = MaxOdometer * 1000;

        public static readonly UInt16 MinWheelSize = UInt16.MinValue;
        public static readonly UInt16 MaxWheelSize = UInt16.MaxValue;

        public static readonly string xmlns = "http://www.w3.org/2000/xmlns/";
        public static readonly string xsins = "http://www.w3.org/2001/XMLSchema-instance";
        
        public static readonly string DeserializeMethodNamePrefix = "Deserialize_V";

        public static readonly string XsiTypeTCXString = "xsi:type";
        public static readonly string ExtensionsTCXString = "Extensions";
        public static readonly string ValueTCXString = "Value";
        public static readonly string LowInMeterPerSecTCXString = "LowInMetersPerSecond";
        public static readonly string HighInMeterPerSecTCXString = "HighInMetersPerSecond";
        public static readonly string[] StepTypeTCXString =
            {
                "Step_t",
                "Repeat_t"
            };
        public static readonly string[] SpeedOrPaceTCXString =
            {
                "Pace",
                "Speed"
            };
        public static readonly string[] DurationTypeTCXString =
            {
                "UserInitiated_t",
                "Distance_t",
                "Time_t",
                "HeartRateAbove_t",
                "HeartRateBelow_t",
                "CaloriesBurned_t"
            };
        public static readonly string[] TargetTypeTCXString =
            {
                "None_t",
                "Speed_t",
                "Cadence_t",
                "HeartRate_t",
                "Power_t"
            };
        public static readonly string[] HeartRateReferenceTCXString =
            {
                "HeartRateInBeatsPerMinute_t",
                "HeartRateAsPercentOfMax_t"
            };
        public static readonly string[] HeartRateRangeZoneTCXString =
            {
                "PredefinedHeartRateZone_t",
                "HeartRateAsPercentOfMax_t"
            };
        public static readonly string[] SpeedRangeZoneTCXString =
            {
                "CustomSpeedZone_t",
                "PredefinedSpeedZone_t"
            };
        public static readonly string[] StepIntensityZoneTCXString =
            {
                "Active",
                "Resting"
            };
        public static readonly string[] GarminCategoryTCXString =
            {
                "Running",
                "Biking",
                "Other"
            };

        public static readonly string ProfileTCXString = "Profile";
        public static readonly string TimeStampTCXString = "TimeStamp";

        public static readonly string BirthDateTCXString = "BirthDate";
        public static readonly string WeightTCXString = "WeightKilograms";
        public static readonly string GenderTCXString = "Gender";
        public static readonly string GenderMaleTCXString = "Male";
        public static readonly string GenderFemaleTCXString = "Female";
        public static readonly string MaxHRBPMTCXString = "MaximumHeartRateBpm";
        public static readonly string RestHRBPMTCXString = "RestingHeartRateBpm";

        public static readonly string ActivitiesTCXString = "Activities";
        public static readonly string GearWeightTCXString = "GearWeightKilograms";
        public static readonly string HeartRateZonesTCXString = "HeartRateZones";
        public static readonly string SpeedZonesTCXString = "SpeedZones";

        public static readonly string ViewAsTCXString = "ViewAs";
        public static readonly string LowTCXString = "Low";
        public static readonly string HighTCXString = "High";
        public static readonly string PercentMaxTCXString = "Percent Max";
        public static readonly string BPMTCXString = "Beats Per Minute";

        public static readonly string PowerZonesTCXString = "PowerZones";
        public static readonly string PowerZoneTCXString = "PowerZone";
        public static readonly string FTPTCXString = "FTP";

        public static readonly string BikeTCXString = "Bike";
        public static readonly string HasCadenceTCXString = "HasCadenceSensor";
        public static readonly string HasPowerTCXString = "HasPowerSensor";
        public static readonly string OdometerTCXString = "OdometerMeters";
        public static readonly string WheelSizeTCXString = "WheelSize";
        public static readonly string AutoWheelSizeTCXString = "AutoWheelSize";
        public static readonly string SizeMillimetersTCXString = "SizeMillimeters";

        public static readonly string WorkoutsClipboardID = "GFP_WorkoutsList";
        public static readonly string StepsClipboardID = "GFP_StepsList";

        public static readonly DataVersion CurrentVersion = new DataVersion(11);
        public static readonly String DataHeaderIdString = "Garmin Workouts Plugin made by S->G";
   }
}