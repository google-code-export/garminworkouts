using System;
using System.Collections.Generic;
using System.Text;

namespace GarminFitnessPlugin.Controller
{
    public class FITConstants
    {
        public static readonly Byte FITProtocolMajorVersion = 1;
        public static readonly Byte FITProtocolMinorVersion = 0;

        public static readonly Byte FITProfileMajorVersion = 1;
        public static readonly Byte FITProfileMinorVersion = 2;
        public static readonly Byte FITProfileMajorVersionMultiplier = 100;

        public static readonly String FITFileDescriptor = ".FIT";
    }

    public enum FITEndianness
    {
        LittleEndian = 0,
        BigEndian,
    }

    public enum FITBoolean
    {
        False = 0,
        True,
    }

    public enum FITFileTypes
    {
        Invalid = -1,
        Settings = 2,
        Sport = 3,
        Workout = 5,
        Schedules = 7,
    }

    public enum FITGlobalMessageIds
    {
        FileId = 0,
        UserProfile = 3,
        BikeProfile = 6,
        ZonesTarget,
        HRZones,
        PowerZones,
        Sport = 12,
        Workout = 26,
        WorkoutStep,
        WorkoutSchedules,
        FileCreator = 49,
        SpeedZones = 53,
    }

    public enum FITSports
    {
        Other = 0,
        Running,
        Cycling,
    }

    public enum FITGenders
    {
        Female = 0,
        Male,
    }

    public enum FITHRCalcTypes
    {
        Custom = 0,
        PercentMax,
        PercentReserve,
    }

    public enum FITPowerCalcTypes
    {
        Custom = 0,
        PercentFTP,
    }

    public enum FITFileIdFieldsIds
    {
        FileType = 0,
        ManufacturerId,
        ProductId,
        SerialNumber,
        ExportDate,
        Number,
    }

    public enum FITFileCreatorFieldsIds
    {
        SoftwareVersion = 0,
        HardwareVersion,
    }

    public enum FITUserProfileFieldIds
    {
        MessageIndex = 254,
        FriendlyName = 0,
        Gender,
        Weight = 4,
        RestingHR = 8,
    }

    public enum FITBikeProfileFieldIds
    {
        MessageIndex = 254,
        Name = 0,
        Odometer = 3,
        CustomWheelSize = 8,
        AutoWheelSize,
        Weight,
        AutoWheelSetting = 12,
        SpeedCadenceSensorEnabled = 17,
        PowerSensorEnabled,
    }

    public enum FITScheduleFieldIds
    {
        WorkoutManufacturer = 0,
        WorkoutProduct,
        WorkoutSN,
        WorkoutId,
        WorkoutCompleted,
        ScheduleType,
        ScheduledDate,
    }

    public enum FITSportFieldIds
    {
        Sport = 0,
        SubSport,
        Name,
    }

    public enum FITZonesTargetFieldIds
    {
        MaxHR = 1,
        ThresholdHR,
        FTP,
        HRCalcType = 5,
        PowerCalcType = 7,
    }

    public enum FITHRZonesFieldIds
    {
        MessageIndex = 254,
        HighBPM = 1,
        Name,
    }

    public enum FITSpeedZonesFieldIds
    {
        MessageIndex = 254,
        HighSpeed = 0,
        Name = 1,
    }

    public enum FITPowerZonesFieldIds
    {
        MessageIndex = 254,
        HighWatts = 1,
        Name
    }

    public enum FITWorkoutFieldIds
    {
        SportType = 4,
        Capabilities,
        NumSteps,
        Unknown,
        WorkoutName = 8,
    }

    public enum FITWorkoutStepFieldIds
    {
        MessageIndex = 254,
        StepName = 0,
        DurationType,
        DurationValue,
        TargetType,
        TargetValue,
        TargetCustomValueLow,
        TargetCustomValueHigh,
        Intensity
    }

    public enum FITWorkoutStepIntensity
    {
        Active = 0,
        Rest,
        Warmup,
        Cooldown
    }

    public enum FITWorkoutStepDurationTypes
    {
        Time = 0,
        Distance,
        HeartRateLessThan,
        HeartRateGreaterThan,
        Calories,
        Open,
        RepeatCount,
        RepeatUntilTime,
        RepeatUntilDistance,
        RepeatUntilCalories,
        RepeatUntilHeartRateLessThan,
        RepeatUntilHeartRateGreaterThan,
        RepeatUntilPowerLessThan,
        RepeatUntilPowerGreaterThan,
        PowerLessThan,
        PowerGreaterThan,
    }

    public enum FITWorkoutStepTargetTypes
    {
        Speed,
        HeartRate,
        NoTarget,
        Cadence,
        Power,
        Grade,
        Resistance,
    }

    public enum FITScheduleType
    {
        Workout,
        Course,
    }

}
