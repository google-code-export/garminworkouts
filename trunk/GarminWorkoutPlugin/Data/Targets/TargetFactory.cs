using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace GarminWorkoutPlugin.Data
{
    class TargetFactory
    {
        static public ITarget Create(ITarget.TargetType type, IStep parent)
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
                        Trace.Assert(false);
                        newTarget = null;
                        break;
                    }
            }

            return newTarget;
        }

        static public ITarget Create(ITarget.TargetType type, Stream stream, DataVersion version, IStep parent)
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
                        Trace.Assert(false);
                        newTarget = null;
                        break;
                    }
            }

            return newTarget;
        }

        static public ITarget Create(ITarget.TargetType type, XmlNode parentNode, IStep parent)
        {
            ITarget newTarget = null;

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
            }

            if(newTarget != null && newTarget.Deserialize(parentNode))
            {
                return newTarget;
            }
            
            return null;
        }
    }
}
