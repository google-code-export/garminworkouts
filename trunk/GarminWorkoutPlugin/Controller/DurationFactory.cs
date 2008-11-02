using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
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
                default:
                    {
                        Trace.Assert(false);
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
                        Trace.Assert(false);
                        newDuration = null;
                        break;
                    }
            }

            parent.Duration = newDuration;

            return newDuration;
        }


        static public IDuration Create(IDuration.DurationType type, XmlNode parentNode, RegularStep parent)
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
                default:
                    {
                        Trace.Assert(false);
                        newDuration = null;
                        break;
                    }
            }

            if (newDuration.Deserialize(parentNode))
            {
                parent.Duration = newDuration;
            }

            return newDuration;
        }
    }
}
