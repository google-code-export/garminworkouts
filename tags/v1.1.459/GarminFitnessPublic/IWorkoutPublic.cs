using System;
using ZoneFiveSoftware.Common.Data.Fitness;

namespace GarminFitnessPublic
{
    public enum GarminSports
    {
        Running = 0,
        Cycling,
        Other
    }

    public interface IPublicWorkout
    {
        Guid Id { get; }
        String Name { get; }
        IActivityCategory Category { get; }
        String Notes { get; }
        GarminSports Sport { get; }
    }
}
