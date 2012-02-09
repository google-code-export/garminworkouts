using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    public class DurationFactory
    {
        static public IDuration Create(IDuration.DurationType type, RegularStep parent)
        {
            IDuration newDuration;

            switch (type)
            {
                case IDuration.DurationType.LapButton:
                    {
                        newDuration = new LapButtonDuration(parent);
                        break;
                    }
                case IDuration.DurationType.Time:
                    {
                        newDuration = new TimeDuration(parent);
                        break;
                    }
                case IDuration.DurationType.Distance:
                    {
                        newDuration = new DistanceDuration(parent);
                        break;
                    }
                case IDuration.DurationType.HeartRateAbove:
                    {
                        newDuration = new HeartRateAboveDuration(parent);
                        break;
                    }
                case IDuration.DurationType.HeartRateBelow:
                    {
                        newDuration = new HeartRateBelowDuration(parent);
                        break;
                    }
                case IDuration.DurationType.Calories:
                    {
                        newDuration = new CaloriesDuration(parent);
                        break;
                    }
                case IDuration.DurationType.PowerAbove:
                    {
                        newDuration = new PowerAboveDuration(parent);
                        break;
                    }
                case IDuration.DurationType.PowerBelow:
                    {
                        newDuration = new PowerBelowDuration(parent);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        newDuration = null;
                        break;
                    }
            }

            parent.Duration = newDuration;

            return newDuration;
        }

        static public IRepeatDuration Create(IRepeatDuration.RepeatDurationType type, RepeatStep parent)
        {
            IRepeatDuration newDuration;

            switch (type)
            {
                case IRepeatDuration.RepeatDurationType.RepeatCount:
                    {
                        newDuration = new RepeatCountDuration(parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilTime:
                    {
                        newDuration = new RepeatUntilTimeDuration(parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilDistance:
                    {
                        newDuration = new RepeatUntilDistanceDuration(parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilHeartRateAbove:
                    {
                        newDuration = new RepeatUntilHeartRateAboveDuration(parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilHeartRateBelow:
                    {
                        newDuration = new RepeatUntilHeartRateBelowDuration(parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilCalories:
                    {
                        newDuration = new RepeatUntilCaloriesDuration(parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilPowerAbove:
                    {
                        newDuration = new RepeatUntilPowerAboveDuration(parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilPowerBelow:
                    {
                        newDuration = new RepeatUntilPowerBelowDuration(parent);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        newDuration = null;
                        break;
                    }
            }

            parent.Duration = newDuration;

            return newDuration;
        }

        static public IDuration Create(IDuration.DurationType type, Stream stream, DataVersion version, RegularStep parent)
        {
            IDuration newDuration;

            switch(type)
            {
                case IDuration.DurationType.LapButton:
                    {
                        newDuration = new LapButtonDuration(stream, version, parent);
                        break;
                    }
                case IDuration.DurationType.Time:
                    {
                        newDuration = new TimeDuration(stream, version, parent);
                        break;
                    }
                case IDuration.DurationType.Distance:
                    {
                        newDuration = new DistanceDuration(stream, version, parent);
                        break;
                    }
                case IDuration.DurationType.HeartRateAbove:
                    {
                        newDuration = new HeartRateAboveDuration(stream, version, parent);
                        break;
                    }
                case IDuration.DurationType.HeartRateBelow:
                    {
                        newDuration = new HeartRateBelowDuration(stream, version, parent);
                        break;
                    }
                case IDuration.DurationType.Calories:
                    {
                        newDuration = new CaloriesDuration(stream, version, parent);
                        break;
                    }
                case IDuration.DurationType.PowerAbove:
                    {
                        newDuration = new PowerAboveDuration(stream, version, parent);
                        break;
                    }
                case IDuration.DurationType.PowerBelow:
                    {
                        newDuration = new PowerBelowDuration(stream, version, parent);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        newDuration = null;
                        break;
                    }
            }

            parent.Duration = newDuration;

            return newDuration;
        }

        static public IRepeatDuration Create(IRepeatDuration.RepeatDurationType type, Stream stream, DataVersion version, RepeatStep parent)
        {
            IRepeatDuration newDuration;

            switch (type)
            {
                case IRepeatDuration.RepeatDurationType.RepeatCount:
                    {
                        newDuration = new RepeatCountDuration(stream, version, parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilTime:
                    {
                        newDuration = new RepeatUntilTimeDuration(stream, version, parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilDistance:
                    {
                        newDuration = new RepeatUntilDistanceDuration(stream, version, parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilHeartRateAbove:
                    {
                        newDuration = new RepeatUntilHeartRateAboveDuration(stream, version, parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilHeartRateBelow:
                    {
                        newDuration = new RepeatUntilHeartRateBelowDuration(stream, version, parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilCalories:
                    {
                        newDuration = new RepeatUntilCaloriesDuration(stream, version, parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilPowerAbove:
                    {
                        newDuration = new RepeatUntilPowerAboveDuration(stream, version, parent);
                        break;
                    }
                case IRepeatDuration.RepeatDurationType.RepeatUntilPowerBelow:
                    {
                        newDuration = new RepeatUntilPowerBelowDuration(stream, version, parent);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        newDuration = null;
                        break;
                    }
            }

            parent.Duration = newDuration;

            return newDuration;
        }

        static public IDuration Create(XmlNode parentNode, RegularStep parent)
        {
            IDuration newDuration = null;

            if (parentNode.Attributes.Count == 1 && parentNode.Attributes[0].Name == Constants.XsiTypeTCXString)
            {
                string stepTypeString = parentNode.Attributes[0].Value;

                // Power above & below are FIT only so stop the loop there
                for (int i = 0; i < (int)IDuration.DurationType.PowerAbove; ++i)
                {
                    if (stepTypeString == Constants.DurationTypeTCXString[i])
                    {
                        newDuration = DurationFactory.Create((IDuration.DurationType)i, parent);
                        newDuration.Deserialize(parentNode);
                        parent.Duration = newDuration;

                        break;
                    }
                }
            }

            return newDuration;
        }

        static public IDuration Create(FITMessage stepMessage, RegularStep parent)
        {
            FITMessageField durationTypeField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField durationField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationValue);
            IDuration newDuration = new LapButtonDuration(parent);

            if (durationTypeField == null)
            {
                throw new FITParserException("Missing duration type field");
            }
            else if ((FITWorkoutStepDurationTypes)durationTypeField.GetEnum() != FITWorkoutStepDurationTypes.Open &&
                     durationField == null)
            {
                throw new FITParserException("Missing duration value field");
            }
            else
            {
                switch ((FITWorkoutStepDurationTypes)durationTypeField.GetEnum())
                {
                    case FITWorkoutStepDurationTypes.Time:
                        {
                            TimeDuration timeDuration = (TimeDuration)Create(IDuration.DurationType.Time, parent);
                            timeDuration.TimeInSeconds = (UInt16)(durationField.GetUInt32() / 1000.0f);
                            newDuration = timeDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.Distance:
                        {
                            DistanceDuration distanceDuration = (DistanceDuration)Create(IDuration.DurationType.Distance, parent);
                            distanceDuration.SetDistanceInBaseUnit(Length.Convert(durationField.GetUInt32(), Length.Units.Centimeter, distanceDuration.BaseUnit));
                            newDuration = distanceDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.HeartRateGreaterThan:
                        {
                            HeartRateAboveDuration hrDuration = (HeartRateAboveDuration)Create(IDuration.DurationType.HeartRateAbove, parent);
                            UInt32 hrValue = durationField.GetUInt32();

                            if (hrValue >= 100)
                            {
                                hrDuration.IsPercentageMaxHeartRate = false;
                                hrDuration.MaxHeartRate = (Byte)(hrValue - 100);
                            }
                            else
                            {
                                hrDuration.IsPercentageMaxHeartRate = true;
                                hrDuration.MaxHeartRate = (Byte)hrValue;
                            }
                            newDuration = hrDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.HeartRateLessThan:
                        {
                            HeartRateBelowDuration hrDuration = (HeartRateBelowDuration)Create(IDuration.DurationType.HeartRateBelow, parent);
                            UInt32 hrValue = durationField.GetUInt32();

                            if (hrValue >= 100)
                            {
                                hrDuration.IsPercentageMaxHeartRate = false;
                                hrDuration.MinHeartRate = (Byte)(hrValue - 100);
                            }
                            else
                            {
                                hrDuration.IsPercentageMaxHeartRate = true;
                                hrDuration.MinHeartRate = (Byte)hrValue;
                            }
                            newDuration = hrDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.Calories:
                        {
                            CaloriesDuration caloriesDuration = (CaloriesDuration)Create(IDuration.DurationType.Calories, parent);
                            caloriesDuration.CaloriesToSpend = (UInt16)durationField.GetUInt32();
                            newDuration = caloriesDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.PowerGreaterThan:
                        {
                            PowerAboveDuration powerDuration = (PowerAboveDuration)Create(IDuration.DurationType.PowerAbove, parent);
                            UInt32 powerValue = durationField.GetUInt32();

                            if (powerValue >= 1000)
                            {
                                powerDuration.IsPercentFTP = false;
                                powerDuration.MaxPower = (UInt16)(powerValue - 1000);
                            }
                            else
                            {
                                powerDuration.IsPercentFTP = true;
                                powerDuration.MaxPower = (UInt16)powerValue;
                            }
                            newDuration = powerDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.PowerLessThan:
                        {
                            PowerBelowDuration powerDuration = (PowerBelowDuration)Create(IDuration.DurationType.PowerBelow, parent);
                            UInt32 powerValue = durationField.GetUInt32();

                            if (powerValue >= 1000)
                            {
                                powerDuration.IsPercentFTP = false;
                                powerDuration.MinPower = (UInt16)(powerValue - 1000);
                            }
                            else
                            {
                                powerDuration.IsPercentFTP = true;
                                powerDuration.MinPower = (UInt16)powerValue;
                            }
                            newDuration = powerDuration;
                            break;
                        }
                }
            }

            parent.Duration = newDuration;

            return newDuration;
        }

        static public IRepeatDuration Create(FITMessage stepMessage, RepeatStep parent)
        {
            FITMessageField durationTypeField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField targetField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            IRepeatDuration newDuration = new RepeatCountDuration(parent);

            if (durationTypeField == null)
            {
                throw new FITParserException("Missing repeat duration type field");
            }
            else if (targetField == null)
            {
                throw new FITParserException("Missing repeat target value field");
            }
            else
            {
                switch ((FITWorkoutStepDurationTypes)durationTypeField.GetEnum())
                {
                    case FITWorkoutStepDurationTypes.RepeatCount:
                        {
                            RepeatCountDuration repeatDuration = (RepeatCountDuration)Create(IRepeatDuration.RepeatDurationType.RepeatCount, parent);
                            repeatDuration.RepetitionCount = (Byte)targetField.GetUInt32();
                            newDuration = repeatDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.RepeatUntilTime:
                        {
                            RepeatUntilTimeDuration timeDuration = Create(IRepeatDuration.RepeatDurationType.RepeatUntilTime, parent) as RepeatUntilTimeDuration;
                            timeDuration.TimeInSeconds = (UInt16)(targetField.GetUInt32() / 1000.0f);
                            newDuration = timeDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.RepeatUntilDistance:
                        {
                            RepeatUntilDistanceDuration distanceDuration = Create(IRepeatDuration.RepeatDurationType.RepeatUntilDistance, parent) as RepeatUntilDistanceDuration;
                            distanceDuration.SetDistanceInBaseUnit(Length.Convert(targetField.GetUInt32(), Length.Units.Centimeter, distanceDuration.BaseUnit));
                            newDuration = distanceDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.RepeatUntilHeartRateGreaterThan:
                        {
                            RepeatUntilHeartRateAboveDuration hrDuration = Create(IRepeatDuration.RepeatDurationType.RepeatUntilHeartRateAbove, parent) as RepeatUntilHeartRateAboveDuration;
                            UInt32 hrValue = targetField.GetUInt32();

                            if (hrValue >= 100)
                            {
                                hrDuration.IsPercentageMaxHeartRate = false;
                                hrDuration.MaxHeartRate = (Byte)(hrValue - 100);
                            }
                            else
                            {
                                hrDuration.IsPercentageMaxHeartRate = true;
                                hrDuration.MaxHeartRate = (Byte)hrValue;
                            }
                            newDuration = hrDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.RepeatUntilHeartRateLessThan:
                        {
                            RepeatUntilHeartRateBelowDuration hrDuration = Create(IRepeatDuration.RepeatDurationType.RepeatUntilHeartRateBelow, parent) as RepeatUntilHeartRateBelowDuration;
                            UInt32 hrValue = targetField.GetUInt32();

                            if (hrValue >= 100)
                            {
                                hrDuration.IsPercentageMaxHeartRate = false;
                                hrDuration.MinHeartRate = (Byte)(hrValue - 100);
                            }
                            else
                            {
                                hrDuration.IsPercentageMaxHeartRate = true;
                                hrDuration.MinHeartRate = (Byte)hrValue;
                            }
                            newDuration = hrDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.RepeatUntilCalories:
                        {
                            RepeatUntilCaloriesDuration caloriesDuration = Create(IRepeatDuration.RepeatDurationType.RepeatUntilCalories, parent) as RepeatUntilCaloriesDuration;
                            caloriesDuration.CaloriesToSpend = (UInt16)targetField.GetUInt32();
                            newDuration = caloriesDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.PowerGreaterThan:
                        {
                            RepeatUntilPowerAboveDuration powerDuration = Create(IRepeatDuration.RepeatDurationType.RepeatUntilPowerAbove, parent) as RepeatUntilPowerAboveDuration;
                            UInt32 powerValue = targetField.GetUInt32();

                            if (powerValue >= 1000)
                            {
                                powerDuration.IsPercentFTP = false;
                                powerDuration.MaxPower = (Byte)(powerValue - 1000);
                            }
                            else
                            {
                                powerDuration.IsPercentFTP = true;
                                powerDuration.MaxPower = (Byte)powerValue;
                            }
                            newDuration = powerDuration;
                            break;
                        }
                    case FITWorkoutStepDurationTypes.RepeatUntilPowerLessThan:
                        {
                            RepeatUntilPowerBelowDuration powerDuration = Create(IRepeatDuration.RepeatDurationType.RepeatUntilPowerBelow, parent) as RepeatUntilPowerBelowDuration;
                            UInt32 powerValue = targetField.GetUInt32();

                            if (powerValue >= 1000)
                            {
                                powerDuration.IsPercentFTP = false;
                                powerDuration.MinPower = (Byte)(powerValue - 1000);
                            }
                            else
                            {
                                powerDuration.IsPercentFTP = true;
                                powerDuration.MinPower = (Byte)powerValue;
                            }
                            newDuration = powerDuration;
                            break;
                        }
                }
            }

            parent.Duration = newDuration;

            return newDuration;
        }
    }
}
