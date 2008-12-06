using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
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

            if(newTarget.Deserialize(parentNode))
            {
                parent.Target = newTarget;
                return newTarget;
            }
            
            return newTarget;
        }
    }
}
