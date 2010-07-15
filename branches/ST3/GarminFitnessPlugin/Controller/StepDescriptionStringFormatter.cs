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
                            result = regularStep.Name;
                        }


                        if (regularStep.IsRestingStep)
                        {
                            result += " " + "(" + GarminFitnessView.GetLocalizedString("RestText") + ")";
                        }
                        break;
                    }
                case IStep.StepType.Repeat:
                    {
                        RepeatStep concreteStep = (RepeatStep)step;
                        string baseString = GarminFitnessView.GetLocalizedString("RepeatStepDescriptionText");
                        result = String.Format(baseString, concreteStep.RepetitionCount);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
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
            IDuration.DurationType type = duration.Type;
            FieldInfo fieldInfo = type.GetType().GetField(Enum.GetName(type.GetType(), type));
            StepDescriptionStringProviderAttribute providerAttribute = (StepDescriptionStringProviderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StepDescriptionStringProviderAttribute));

            baseString = GarminFitnessView.GetLocalizedString(providerAttribute.StringName);

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
                        result = String.Format(baseString, concreteDuration.GetDistanceInBaseUnit(), Length.LabelAbbr(concreteDuration.BaseUnit));
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
                        Debug.Assert(false);
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
                baseString = GarminFitnessView.GetLocalizedString(providerAttribute.StringName);
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
                        result = FormatSpeedTarget(baseTarget.ConcreteTarget);
                        break;
                    }
                case ITarget.TargetType.Cadence:
                    {
                        BaseCadenceTarget baseTarget = (BaseCadenceTarget)target;
                        result = FormatCadenceTarget(baseTarget.ConcreteTarget);
                        break;
                    }
                case ITarget.TargetType.HeartRate:
                    {
                        BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)target;
                        result = FormatHeartRateTarget(baseTarget.ConcreteTarget);
                        break;
                    }
                case ITarget.TargetType.Power:
                    {
                        BasePowerTarget baseTarget = (BasePowerTarget)target;
                        result = FormatPowerTarget(baseTarget.ConcreteTarget);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        result = String.Empty;
                        break;
                    }
            }

            return result;
        }

        static private string FormatCadenceTarget(BaseCadenceTarget.IConcreteCadenceTarget target)
        {
            string result;
            string baseString;
            BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType type = target.Type;
            FieldInfo fieldInfo = type.GetType().GetField(Enum.GetName(type.GetType(), type));
            StepDescriptionStringProviderAttribute providerAttribute = (StepDescriptionStringProviderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StepDescriptionStringProviderAttribute));

            baseString = GarminFitnessView.GetLocalizedString(providerAttribute.StringName);

            switch (type)
            {
                case BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.Range:
                    {
                        CadenceRangeTarget concreteTarget = (CadenceRangeTarget)target;

                        result = String.Format(baseString, concreteTarget.MinCadence, concreteTarget.MaxCadence, CommonResources.Text.LabelRPM);
                        break;
                    }
                case BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.ZoneST:
                    {
                        CadenceZoneSTTarget concreteTarget = (CadenceZoneSTTarget)target;

                        result = String.Format(baseString, concreteTarget.Zone.Name);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        result = String.Empty;
                        break;
                    }
            }

            return result;
        }

        static private string FormatHeartRateTarget(BaseHeartRateTarget.IConcreteHeartRateTarget target)
        {
            string result;
            string baseString;
            BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType type = target.Type;
            FieldInfo fieldInfo = type.GetType().GetField(Enum.GetName(type.GetType(), type));
            StepDescriptionStringProviderAttribute providerAttribute = (StepDescriptionStringProviderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StepDescriptionStringProviderAttribute));

            baseString = GarminFitnessView.GetLocalizedString(providerAttribute.StringName);

            switch (type)
            {
                case BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.Range:
                    {
                        HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)target;
                        string unitsString;

                        if (concreteTarget.IsPercentMaxHeartRate)
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
                case BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.ZoneGTC:
                    {
                        HeartRateZoneGTCTarget concreteTarget = (HeartRateZoneGTCTarget)target;
                        result = String.Format(baseString, concreteTarget.Zone);
                        break;
                    }
                case BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.ZoneST:
                    {
                        HeartRateZoneSTTarget concreteTarget = (HeartRateZoneSTTarget)target;
                        result = String.Format(baseString, concreteTarget.Zone.Name);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        result = String.Empty;
                        break;
                    }
            }

            return result;
        }


        static private string FormatSpeedTarget(BaseSpeedTarget.IConcreteSpeedTarget target)
        {
            string result;
            string baseString;
            BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType type = target.Type;
            FieldInfo fieldInfo = type.GetType().GetField(Enum.GetName(type.GetType(), type));
            StepDescriptionStringProviderAttribute providerAttribute = (StepDescriptionStringProviderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StepDescriptionStringProviderAttribute));

            baseString = GarminFitnessView.GetLocalizedString(providerAttribute.StringName);

            switch (type)
            {
                case BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.Range:
                    {
                        SpeedRangeTarget concreteTarget = (SpeedRangeTarget)target;

                        if (concreteTarget.BaseTarget.ParentStep.ParentConcreteWorkout.Category.SpeedUnits == Speed.Units.Pace)
                        {
                            baseString = GarminFitnessView.GetLocalizedString("PaceRangeTargetDescriptionText");

                            double min = concreteTarget.GetMinSpeedInMinutesPerBaseUnit();
                            double max = concreteTarget.GetMaxSpeedInMinutesPerBaseUnit();
                            UInt16 minMinutes, minSeconds;
                            UInt16 maxMinutes, maxSeconds;

                            Utils.DoubleToTime(min, out minMinutes, out minSeconds);
                            Utils.DoubleToTime(max, out maxMinutes, out maxSeconds);
                            result = String.Format(baseString,
                                                   maxMinutes, maxSeconds, 
                                                   minMinutes, minSeconds,
                                                   Length.LabelAbbr(concreteTarget.BaseUnit));
                        }
                        else
                        {
                            result = String.Format(baseString, concreteTarget.GetMinSpeedInBaseUnitsPerHour(), concreteTarget.GetMaxSpeedInBaseUnitsPerHour(), Length.LabelAbbr(concreteTarget.BaseUnit));
                        }
                        break;
                    }
                case BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.ZoneGTC:
                    {
                        SpeedZoneGTCTarget concreteTarget = (SpeedZoneGTCTarget)target;
                        GarminCategories garminCategory = Options.Instance.GetGarminCategory(target.BaseTarget.ParentStep.ParentWorkout.Category);
                        GarminActivityProfile currentProfile = GarminProfileManager.Instance.GetProfileForActivity(garminCategory);
                        result = String.Format(baseString, currentProfile.GetSpeedZoneName(concreteTarget.Zone));
                        break;
                    }
                case BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.ZoneST:
                    {
                        SpeedZoneSTTarget concreteTarget = (SpeedZoneSTTarget)target;
                        result = String.Format(baseString, concreteTarget.Zone.Name);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        result = String.Empty;
                        break;
                    }
            }

            return result;
        }

        static private string FormatPowerTarget(BasePowerTarget.IConcretePowerTarget target)
        {
            string result;
            string baseString;
            BasePowerTarget.IConcretePowerTarget.PowerTargetType type = target.Type;
            FieldInfo fieldInfo = type.GetType().GetField(Enum.GetName(type.GetType(), type));
            StepDescriptionStringProviderAttribute providerAttribute = (StepDescriptionStringProviderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StepDescriptionStringProviderAttribute));

            baseString = GarminFitnessView.GetLocalizedString(providerAttribute.StringName);

            switch (type)
            {
                case BasePowerTarget.IConcretePowerTarget.PowerTargetType.Range:
                    {
                        PowerRangeTarget concreteTarget = (PowerRangeTarget)target;
                        string unitsString = CommonResources.Text.LabelWatts;

                        result = String.Format(baseString, concreteTarget.MinPower, concreteTarget.MaxPower, unitsString);
                        break;
                    }
                case BasePowerTarget.IConcretePowerTarget.PowerTargetType.ZoneGTC:
                    {
                        PowerZoneGTCTarget concreteTarget = (PowerZoneGTCTarget)target;
                        result = String.Format(baseString, concreteTarget.Zone);
                        break;
                    }
                case BasePowerTarget.IConcretePowerTarget.PowerTargetType.ZoneST:
                    {
                        PowerZoneSTTarget concreteTarget = (PowerZoneSTTarget)target;
                        result = String.Format(baseString, concreteTarget.Zone.Name);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        result = String.Empty;
                        break;
                    }
            }

            return result;
        }
    }
}
