using System;
using GarminWorkoutPlugin.Data;

namespace GarminWorkoutPlugin
{
    class Constants
    {
        public static readonly UInt16 SecondsPerMinute = 60;
        public static readonly UInt16 MinutesPerHour = 60;
        public static readonly UInt16 SecondsPerHour = (UInt16)(MinutesPerHour * SecondsPerMinute);

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
                "Speed",
                "Pace"
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

        public static DataVersion CurrentVersion = new DataVersion(4);
    }
}