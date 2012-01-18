using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class TargetFactory
    {
        static public ITarget Create(ITarget.TargetType type, RegularStep parent)
        {
            ITarget newTarget;

            switch (type)
            {
                case ITarget.TargetType.Null:
                    {
                        newTarget = new NullTarget(parent);
                        break;
                    }
                case ITarget.TargetType.Speed:
                    {
                        newTarget = new BaseSpeedTarget(parent);
                        break;
                    }
                case ITarget.TargetType.HeartRate:
                    {
                        newTarget = new BaseHeartRateTarget(parent);
                        break;
                    }
                case ITarget.TargetType.Cadence:
                    {
                        newTarget = new BaseCadenceTarget(parent);
                        break;
                    }
                case ITarget.TargetType.Power:
                    {
                        newTarget = new BasePowerTarget(parent);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        newTarget = null;
                        break;
                    }
            }

            parent.Target = newTarget;

            return newTarget;
        }

        static public ITarget Create(ITarget.TargetType type, Stream stream, DataVersion version, RegularStep parent)
        {
            ITarget newTarget;

            switch (type)
            {
                case ITarget.TargetType.Null:
                    {
                        newTarget = new NullTarget(stream, version, parent);
                        break;
                    }
                case ITarget.TargetType.Speed:
                    {
                        newTarget = new BaseSpeedTarget(stream, version, parent);
                        break;
                    }
                case ITarget.TargetType.HeartRate:
                    {
                        newTarget = new BaseHeartRateTarget(stream, version, parent);
                        break;
                    }
                case ITarget.TargetType.Cadence:
                    {
                        newTarget = new BaseCadenceTarget(stream, version, parent);
                        break;
                    }
                case ITarget.TargetType.Power:
                    {
                        newTarget = new BasePowerTarget(stream, version, parent);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        newTarget = null;
                        break;
                    }
            }

            parent.Target = newTarget;

            return newTarget;
        }

        static public ITarget Create(ITarget.TargetType type, XmlNode parentNode, RegularStep parent)
        {
            ITarget newTarget = Create(type, parent);

            newTarget.Deserialize(parentNode);
            parent.Target = newTarget;
            
            return newTarget;
        }

        static public ITarget Create(FITMessage stepMessage, RegularStep parent)
        {
            FITMessageField targetTypeField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetType);
            FITMessageField targetField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetValue);
            FITMessageField targetCustomLowField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetCustomValueLow);
            FITMessageField targetCustomHighField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.TargetCustomValueHigh);
            ITarget result = new NullTarget(parent);

            if(targetTypeField != null)
            {
                switch((FITWorkoutStepTargetTypes)targetTypeField.GetEnum())
                {
                    case FITWorkoutStepTargetTypes.HeartRate:
                        {
                            if(targetField != null)
                            {
                                BaseHeartRateTarget hrTarget = (BaseHeartRateTarget)Create(ITarget.TargetType.HeartRate, parent);
                                UInt32 hrZone = targetField.GetUInt32();

                                if (hrZone == 0)
                                {
                                    if (targetCustomLowField != null &&
                                        targetCustomHighField != null)
                                    {
                                        HeartRateRangeTarget concreteTarget = new HeartRateRangeTarget(hrTarget);

                                        if (targetCustomLowField.GetUInt32() > 100)
                                        {
                                            concreteTarget.SetValues((Byte)(targetCustomLowField.GetUInt32() - 100),
                                                                     (Byte)(targetCustomHighField.GetUInt32() - 100),
                                                                     false);
                                        }
                                        else
                                        {
                                            concreteTarget.SetValues((Byte)targetCustomLowField.GetUInt32(),
                                                                     (Byte)targetCustomHighField.GetUInt32(),
                                                                     true);
                                        }

                                        hrTarget.ConcreteTarget = concreteTarget;
                                    }
                                    else
                                    {
                                        throw new FITParserException("Missing target custom value field");
                                    }
                                }
                                else
                                {
                                    HeartRateZoneGTCTarget concreteTarget = new HeartRateZoneGTCTarget(hrTarget);

                                    concreteTarget.Zone = (Byte)hrZone;
                                    hrTarget.ConcreteTarget = concreteTarget;
                                }

                                result = hrTarget;
                            }
                            else
                            {
                                throw new FITParserException("Missing target value field");
                            }
                            break;
                        }
                    case FITWorkoutStepTargetTypes.Cadence:
                        {
                            if (targetField != null &&
                                targetCustomLowField != null &&
                                targetCustomHighField != null)
                            {
                                BaseCadenceTarget cadenceTarget = (BaseCadenceTarget)Create(ITarget.TargetType.Cadence, parent);
                                CadenceRangeTarget concreteTarget = new CadenceRangeTarget(cadenceTarget);
                                UInt32 cadenceZone = targetField.GetUInt32();

                                Debug.Assert(cadenceZone == 0);

                                concreteTarget.SetValues((Byte)targetCustomLowField.GetUInt32(),
                                                         (Byte)targetCustomHighField.GetUInt32());
                                cadenceTarget.ConcreteTarget = concreteTarget;

                                result = cadenceTarget;
                            }
                            else
                            {
                                throw new FITParserException("Missing target or custom values field");
                            }

                            break;
                        }
                    case FITWorkoutStepTargetTypes.Speed:
                        {
                            if(targetField != null)
                            {
                                BaseSpeedTarget speedTarget = (BaseSpeedTarget)Create(ITarget.TargetType.Speed, parent);
                                UInt32 speedZone = targetField.GetUInt32();

                                if (speedZone == 0)
                                {
                                    if (targetCustomLowField != null &&
                                        targetCustomHighField != null)
                                    {
                                        SpeedRangeTarget concreteTarget = new SpeedRangeTarget(speedTarget);

                                        concreteTarget.SetRangeInBaseUnitsPerHour(Length.Convert(targetCustomLowField.GetUInt32() * 3.6, Length.Units.Meter, concreteTarget.BaseUnit),
                                                                                  Length.Convert(targetCustomHighField.GetUInt32() * 3.6, Length.Units.Meter, concreteTarget.BaseUnit));
                                        speedTarget.ConcreteTarget = concreteTarget;
                                    }
                                    else
                                    {
                                        throw new FITParserException("Missing target custom value field");
                                    }
                                }
                                else
                                {
                                    SpeedZoneGTCTarget concreteTarget = new SpeedZoneGTCTarget(speedTarget);

                                    concreteTarget.Zone = (Byte)speedZone;
                                    speedTarget.ConcreteTarget = concreteTarget;
                                }

                                result = speedTarget;
                            }
                            else
                            {
                                throw new FITParserException("Missing target value field");
                            }
                            break;
                        }
                    case FITWorkoutStepTargetTypes.Power:
                        {
                            if(targetField != null)
                            {
                                BasePowerTarget powerTarget = (BasePowerTarget)Create(ITarget.TargetType.Power, parent);
                                UInt32 powerZone = targetField.GetUInt32();

                                if (powerZone == 0)
                                {
                                    if (targetCustomLowField != null &&
                                        targetCustomHighField != null)
                                    {
                                        PowerRangeTarget concreteTarget = new PowerRangeTarget(powerTarget);

                                        if (targetCustomLowField.GetUInt32() > 1000)
                                        {
                                            concreteTarget.SetValues((UInt16)(targetCustomLowField.GetUInt32() - 1000),
                                                                     (UInt16)(targetCustomHighField.GetUInt32() - 1000),
                                                                     false);
                                        }
                                        else
                                        {
                                            concreteTarget.SetValues((UInt16)(targetCustomLowField.GetUInt32()),
                                                                     (UInt16)(targetCustomHighField.GetUInt32()),
                                                                     true);
                                        }
                                        powerTarget.ConcreteTarget = concreteTarget;
                                    }
                                    else
                                    {
                                        throw new FITParserException("Missing target custom value field");
                                    }
                                }
                                else
                                {
                                    PowerZoneGTCTarget concreteTarget = new PowerZoneGTCTarget(powerTarget);

                                    concreteTarget.Zone = (Byte)powerZone;
                                    powerTarget.ConcreteTarget = concreteTarget;
                                }

                                result = powerTarget;
                            }
                            else
                            {
                                throw new FITParserException("Missing target value field");
                            }

                            break;
                        }
                }
            }
            else
            {
                throw new FITParserException("Missing target type field");
            }

            return result;
        }
    }
}
