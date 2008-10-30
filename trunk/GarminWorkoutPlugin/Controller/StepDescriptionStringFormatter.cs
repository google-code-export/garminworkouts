using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using ZoneFiveSoftware.Common.Data.Measurement;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.View;

namespace GarminFitnessPlugin.Controller
{
    class StepDescriptionStringFormatter
    {
        static public string FormatStepDescription(IStep step)
        {
            string result = String.Empty;

            switch (step.Type)
            {
                case IStep.StepType.Regular:
                    {
                        RegularStep regularStep = (RegularStep)step;
                        if (regularStep.Name == null || regularStep.Name == String.Empty)
                        {
                            result = FormatDurationDescription(regularStep.Duration) +
                                     "  " +
                                     FormatTargetDescription(regularStep.Target);
                        }
                        else
                        {
                            return regularStep.Name;
                        }
                        break;
                    }
                case IStep.StepType.Repeat:
                    {
                        RepeatStep concreteStep = (RepeatStep)step;
                        string baseString = GarminFitnessView.ResourceManager.GetString("RepeatStepDescriptionText", GarminFitnessView.UICulture);
                        result = String.Format(baseString, concreteStep.RepetitionCount);
                        break;
                    }
                default:
                    {
                        Trace.Assert(false);
                        result = String.Empty;
                        break;
                    }
            }

            return result;
        }

        static private string FormatDurationDescription(IDuration duration)
        {
            string result;
            string baseString;
            Length.Units systemUnit = duration.ParentStep.ParentWorkout.Category.DistanceUnits;
            IDuration.DurationType type = duration.Type;
            FieldInfo fieldInfo = type.GetType().GetField(Enum.GetName(type.GetType(), type));
            StepDescriptionStringProviderAttribute providerAttribute = (StepDescriptionStringProviderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StepDescriptionStringProviderAttribute));

            baseString = GarminFitnessView.ResourceManager.GetString(providerAttribute.StringName, GarminFitnessView.UICulture);

            switch(type)
            {
                case IDuration.DurationType.LapButton:
                    {
                        result = baseString;
                        break;
                    }
                case IDuration.DurationType.Distance:
                    {
                        DistanceDuration concreteDuration = (DistanceDuration)duration;
                        double distance = Length.Convert(concreteDuration.DistanceInMeters, Length.Units.Meter, systemUnit);
                        result = String.Format(baseString, distance, Length.LabelAbbr(systemUnit));
                        break;
                    }
                case IDuration.DurationType.Time:
                    {
                        TimeDuration concreteDuration = (TimeDuration)duration;
                        result = String.Format(baseString, concreteDuration.Hours, concreteDuration.Minutes, concreteDuration.Seconds);
                        break;
                    }
                case IDuration.DurationType.HeartRateAbove:
                    {
                        HeartRateAboveDuration concreteDuration = (HeartRateAboveDuration)duration;
                        string unitsString;

                        if(concreteDuration.IsPercentageMaxHeartRate)
                        {
                            unitsString = CommonResources.Text.LabelPercentOfMax;
                        }
                        else
                        {
                            unitsString = CommonResources.Text.LabelBPM;
                        }

                        result = String.Format(baseString, concreteDuration.MaxHeartRate, unitsString);
                        break;
                    }
                case IDuration.DurationType.HeartRateBelow:
                    {
                        HeartRateBelowDuration concreteDuration = (HeartRateBelowDuration)duration;
                        string unitsString;

                        if(concreteDuration.IsPercentageMaxHeartRate)
                        {
                            unitsString = CommonResources.Text.LabelPercentOfMax;
                        }
                        else
                        {
                            unitsString = CommonResources.Text.LabelBPM;
                        }

                        result = String.Format(baseString, concreteDuration.MinHeartRate, unitsString);
                        break;
                    }
                case IDuration.DurationType.Calories:
                    {
                        CaloriesDuration concreteDuration = (CaloriesDuration)duration;
                        result = String.Format(baseString, concreteDuration.CaloriesToSpend);
                        break;
                    }
                default:
                    {
                        Trace.Assert(false);
                        result = String.Empty;
                        break;
                    }
            }

            return result;
        }

        static private string FormatTargetDescription(ITarget target)
        {
            string result;
            string baseString = string.Empty;
            ITarget.TargetType type = target.Type;
            FieldInfo fieldInfo = type.GetType().GetField(Enum.GetName(type.GetType(), type));
            StepDescriptionStringProviderAttribute providerAttribute = (StepDescriptionStringProviderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StepDescriptionStringProviderAttribute));

            if (providerAttribute != null)
            {
                baseString = GarminFitnessView.ResourceManager.GetString(providerAttribute.StringName, GarminFitnessView.UICulture);
            }

            switch(type)
            {
                case ITarget.TargetType.Null:
                    {
                        result = "";
                        break;
                    }
                case ITarget.TargetType.Speed:
                    {
                        BaseSpeedTarget baseTarget = (BaseSpeedTarget)target;
                        result = FormatSpeedTarget(baseTarget.ConcreteTarget, GarminFitnessView.ResourceManager, GarminFitnessView.UICulture);
                        break;
                    }
                case ITarget.TargetType.Cadence:
                    {
                        BaseCadenceTarget baseTarget = (BaseCadenceTarget)target;
                        result = FormatCadenceTarget(baseTarget.ConcreteTarget, GarminFitnessView.ResourceManager, GarminFitnessView.UICulture);
                        break;
                    }
                case ITarget.TargetType.HeartRate:
                    {
                        BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)target;
                        result = FormatHeartRateTarget(baseTarget.ConcreteTarget, GarminFitnessView.ResourceManager, GarminFitnessView.UICulture);
                        break;
                    }
                case ITarget.TargetType.Power:
                    {
                        BasePowerTarget baseTarget = (BasePowerTarget)target;
                        result = FormatPowerTarget(baseTarget.ConcreteTarget, GarminFitnessView.ResourceManager, GarminFitnessView.UICulture);
                        break;
                    }
                default:
                    {
                        Trace.Assert(false);
                        result = String.Empty;
                        break;
                    }
            }

            return result;
        }

        static private string FormatCadenceTarget(IConcreteCadenceTarget target, ResourceManager resManager, CultureInfo culture)
        {
            string result;
            string baseString;
            IConcreteCadenceTarget.CadenceTargetType type = target.Type;
            FieldInfo fieldInfo = type.GetType().GetField(Enum.GetName(type.GetType(), type));
            StepDescriptionStringProviderAttribute providerAttribute = (StepDescriptionStringProviderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StepDescriptionStringProviderAttribute));

            baseString = resManager.GetString(providerAttribute.StringName, culture);

            switch (type)
            {
                case IConcreteCadenceTarget.CadenceTargetType.Range:
                    {
                        CadenceRangeTarget concreteTarget = (CadenceRangeTarget)target;

                        result = String.Format(baseString, concreteTarget.MinCadence, concreteTarget.MaxCadence, CommonResources.Text.LabelRPM);
                        break;
                    }
                case IConcreteCadenceTarget.CadenceTargetType.ZoneST:
                    {
                        CadenceZoneSTTarget concreteTarget = (CadenceZoneSTTarget)target;

                        result = String.Format(baseString, concreteTarget.Zone.Name);
                        break;
                    }
                default:
                    {
                        Trace.Assert(false);
                        result = String.Empty;
                        break;
                    }
            }

            return result;
        }

        static private string FormatHeartRateTarget(IConcreteHeartRateTarget target, ResourceManager resManager, CultureInfo culture)
        {
            string result;
            string baseString;
            IConcreteHeartRateTarget.HeartRateTargetType type = target.Type;
            FieldInfo fieldInfo = type.GetType().GetField(Enum.GetName(type.GetType(), type));
            StepDescriptionStringProviderAttribute providerAttribute = (StepDescriptionStringProviderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StepDescriptionStringProviderAttribute));

            baseString = resManager.GetString(providerAttribute.StringName, culture);

            switch (type)
            {
                case IConcreteHeartRateTarget.HeartRateTargetType.Range:
                    {
                        HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)target;
                        string unitsString;

                        if (concreteTarget.IsPercentageMaxHeartRate)
                        {
                            unitsString = CommonResources.Text.LabelPercentOfMax;
                        }
                        else
                        {
                            unitsString = CommonResources.Text.LabelBPM;
                        }

                        result = String.Format(baseString, concreteTarget.MinHeartRate, concreteTarget.MaxHeartRate, unitsString);
                        break;
                    }
                case IConcreteHeartRateTarget.HeartRateTargetType.ZoneGTC:
                    {
                        HeartRateZoneGTCTarget concreteTarget = (HeartRateZoneGTCTarget)target;
                        result = String.Format(baseString, concreteTarget.Zone);
                        break;
                    }
                case IConcreteHeartRateTarget.HeartRateTargetType.ZoneST:
                    {
                        HeartRateZoneSTTarget concreteTarget = (HeartRateZoneSTTarget)target;
                        result = String.Format(baseString, concreteTarget.Zone.Name);
                        break;
                    }
                default:
                    {
                        Trace.Assert(false);
                        result = String.Empty;
                        break;
                    }
            }

            return result;
        }


        static private string FormatSpeedTarget(IConcreteSpeedTarget target, ResourceManager resManager, CultureInfo culture)
        {
            string result;
            string baseString;
            IConcreteSpeedTarget.SpeedTargetType type = target.Type;
            FieldInfo fieldInfo = type.GetType().GetField(Enum.GetName(type.GetType(), type));
            StepDescriptionStringProviderAttribute providerAttribute = (StepDescriptionStringProviderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StepDescriptionStringProviderAttribute));

            baseString = resManager.GetString(providerAttribute.StringName, culture);

            switch (type)
            {
                case IConcreteSpeedTarget.SpeedTargetType.Range:
                    {
                        SpeedRangeTarget concreteTarget = (SpeedRangeTarget)target;
                        Length.Units systemUnit = target.BaseTarget.ParentStep.ParentWorkout.Category.DistanceUnits;

                        if (concreteTarget.BaseTarget.ParentStep.ParentWorkout.Category.SpeedUnits == Speed.Units.Pace)
                        {
                            baseString = resManager.GetString("PaceRangeTargetDescriptionText");

                            double min = concreteTarget.GetMinSpeedInMinutesPerUnit(systemUnit);
                            double max = concreteTarget.GetMaxSpeedInMinutesPerUnit(systemUnit);
                            UInt16 minMinutes, minSeconds;
                            UInt16 maxMinutes, maxSeconds;

                            Utils.FloatToTime(min, out minMinutes, out minSeconds);
                            Utils.FloatToTime(max, out maxMinutes, out maxSeconds);
                            result = String.Format(baseString,
                                                   maxMinutes, maxSeconds, 
                                                   minMinutes, minSeconds,
                                                   Length.LabelAbbr(systemUnit));
                        }
                        else
                        {
                            result = String.Format(baseString, concreteTarget.GetMinSpeedInUnitsPerHour(systemUnit), concreteTarget.GetMaxSpeedInUnitsPerHour(systemUnit), Length.LabelAbbr(systemUnit));
                        }
                        break;
                    }
                case IConcreteSpeedTarget.SpeedTargetType.ZoneGTC:
                    {
                        SpeedZoneGTCTarget concreteTarget = (SpeedZoneGTCTarget)target;
                        result = String.Format(baseString, resManager.GetString("GTCSpeedZone" + concreteTarget.Zone.ToString() + "Text", culture));
                        break;
                    }
                case IConcreteSpeedTarget.SpeedTargetType.ZoneST:
                    {
                        SpeedZoneSTTarget concreteTarget = (SpeedZoneSTTarget)target;
                        result = String.Format(baseString, concreteTarget.Zone.Name);
                        break;
                    }
                default:
                    {
                        Trace.Assert(false);
                        result = String.Empty;
                        break;
                    }
            }

            return result;
        }

        static private string FormatPowerTarget(IConcretePowerTarget target, ResourceManager resManager, CultureInfo culture)
        {
            string result;
            string baseString;
            IConcretePowerTarget.PowerTargetType type = target.Type;
            FieldInfo fieldInfo = type.GetType().GetField(Enum.GetName(type.GetType(), type));
            StepDescriptionStringProviderAttribute providerAttribute = (StepDescriptionStringProviderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StepDescriptionStringProviderAttribute));

            baseString = resManager.GetString(providerAttribute.StringName, culture);

            switch (type)
            {
                case IConcretePowerTarget.PowerTargetType.Range:
                    {
                        PowerRangeTarget concreteTarget = (PowerRangeTarget)target;
                        string unitsString = CommonResources.Text.LabelWatts;

                        result = String.Format(baseString, concreteTarget.MinPower, concreteTarget.MaxPower, unitsString);
                        break;
                    }
                case IConcretePowerTarget.PowerTargetType.ZoneGTC:
                    {
                        PowerZoneGTCTarget concreteTarget = (PowerZoneGTCTarget)target;
                        result = String.Format(baseString, concreteTarget.Zone);
                        break;
                    }
                case IConcretePowerTarget.PowerTargetType.ZoneST:
                    {
                        PowerZoneSTTarget concreteTarget = (PowerZoneSTTarget)target;
                        result = String.Format(baseString, concreteTarget.Zone.Name);
                        break;
                    }
                default:
                    {
                        Trace.Assert(false);
                        result = String.Empty;
                        break;
                    }
            }

            return result;
        }
    }
}
