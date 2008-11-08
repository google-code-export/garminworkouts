using System;
using GarminFitnessPlugin.Data;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin
{
    class Constants
    {
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
        public static readonly UInt16 MaxPower = 999;

        // Repeat limits
        public static readonly Byte MinRepeats = 2;
        public static readonly Byte MaxRepeats = 99;

        public static readonly Byte GarminHRZoneCount = 5;
        public static readonly Byte GarminSpeedZoneCount = 10;
        public static readonly Byte GarminPowerZoneCount = 7;

        public static readonly string xmlns = "http://www.w3.org/2000/xmlns/";
        public static readonly string xsins = "http://www.w3.org/2001/XMLSchema-instance";
        
        public static readonly string DeserializeMethodNamePrefix = "Deserialize_V";

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

        public static readonly DataVersion CurrentVersion = new DataVersion(8);
        public static readonly String DataHeaderIdString = "Garmin Workouts Plugin made by S->G";

        public static readonly string WorkoutsClipboardID = "GFP_WorkoutsList";
        public static readonly string StepsClipboardID = "GFP_StepsList";
   }
}