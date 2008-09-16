using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace GarminWorkoutPlugin.Data
{
    class DurationFactory
    {
        static public IDuration Create(IDuration.DurationType type, IStep parent)
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

            return newDuration;
        }

        static public IDuration Create(IDuration.DurationType type, Stream stream, DataVersion version, IStep parent)
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

            return newDuration;
        }


        static public IDuration Create(IDuration.DurationType type, XmlNode parentNode, IStep parent)
        {
            IDuration newDuration = null;

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
            }

            if (newDuration != null && newDuration.Deserialize(parentNode))
            {
                return newDuration;
            }

            return null;
        }
    }
}
