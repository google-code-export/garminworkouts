using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class DurationFactory
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


        static public IDuration Create(IDuration.DurationType type, XmlNode parentNode, RegularStep parent)
        {
            IDuration newDuration = Create(type, parent);

            newDuration.Deserialize(parentNode);
                parent.Duration = newDuration;

            return newDuration;
        }

        static public IDuration Create(FITMessage stepMessage, RegularStep parent)
        {
            FITMessageField durationTypeField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField durationField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationValue);
            IDuration result = new LapButtonDuration(parent);

            if (durationTypeField != null)
            {
                switch ((FITWorkoutStepDurationTypes)durationTypeField.GetEnum())
                {
                    case FITWorkoutStepDurationTypes.Time:
                        {
                            if (durationField != null)
                            {
                                TimeDuration timeDuration = (TimeDuration)Create(IDuration.DurationType.Time, parent);
                                timeDuration.TimeInSeconds = (UInt16)(durationField.GetUInt32() / 1000.0f);
                                result = timeDuration;
                            }
                            else
                            {
                                throw new FITParserException("Missing duration value field");
                            }
                            break;
                        }
                    case FITWorkoutStepDurationTypes.Distance:
                        {
                            if (durationField != null)
                            {
                                DistanceDuration distanceDuration = (DistanceDuration)Create(IDuration.DurationType.Distance, parent);
                                distanceDuration.SetDistanceInBaseUnit(Length.Convert(durationField.GetUInt32(), Length.Units.Centimeter, distanceDuration.BaseUnit));
                                result = distanceDuration;
                            }
                            else
                            {
                                throw new FITParserException("Missing duration value field");
                            }
                            break;
                        }
                    case FITWorkoutStepDurationTypes.HeartRateGreaterThan:
                        {
                            if (durationField != null)
                            {
                                HeartRateAboveDuration hrDuration = (HeartRateAboveDuration)Create(IDuration.DurationType.HeartRateAbove, parent);
                                UInt32 hrValue = durationField.GetUInt32();

                                if (hrValue > 100)
                                {
                                    hrDuration.IsPercentageMaxHeartRate = false;
                                    hrDuration.MaxHeartRate = (Byte)(hrValue - 100);
                                }
                                else
                                {
                                    hrDuration.IsPercentageMaxHeartRate = true;
                                    hrDuration.MaxHeartRate = (Byte)hrValue;
                                }
                                result = hrDuration;
                            }
                            else
                            {
                                throw new FITParserException("Missing duration value field");
                            }
                            break;
                        }
                    case FITWorkoutStepDurationTypes.HeartRateLessThan:
                        {
                            if (durationField != null)
                            {
                                HeartRateBelowDuration hrDuration = (HeartRateBelowDuration)Create(IDuration.DurationType.HeartRateBelow, parent);
                                UInt32 hrValue = durationField.GetUInt32();

                                if (hrValue > 100)
                                {
                                    hrDuration.IsPercentageMaxHeartRate = false;
                                    hrDuration.MinHeartRate = (Byte)(hrValue - 100);
                                }
                                else
                                {
                                    hrDuration.IsPercentageMaxHeartRate = true;
                                    hrDuration.MinHeartRate = (Byte)hrValue;
                                }
                                result = hrDuration;
                            }
                            else
                            {
                                throw new FITParserException("Missing duration value field");
                            }
                            break;
                        }
                    case FITWorkoutStepDurationTypes.Calories:
                        {
                            if (durationField != null)
                            {
                                CaloriesDuration caloriesDuration = (CaloriesDuration)Create(IDuration.DurationType.Calories, parent);
                                caloriesDuration.CaloriesToSpend = (UInt16)durationField.GetUInt32();
                                result = caloriesDuration;
                            }
                            else
                            {
                                throw new FITParserException("Missing duration value field");
                            }
                            break;
                        }
                    case FITWorkoutStepDurationTypes.PowerGreaterThan:
                        {
                            break;
                        }
                    case FITWorkoutStepDurationTypes.PowerLessThan:
                        {
                            break;
                        }
                }
            }
            else
            {
                throw new FITParserException("Missing duration type field");
            }

            return result;
        }
    }
}
