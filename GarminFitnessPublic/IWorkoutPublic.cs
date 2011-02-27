using System;
using ZoneFiveSoftware.Common.Data.Fitness;

namespace GarminFitnessPublic
{
    public interface IPublicWorkout
    {
        Guid Id { get; }
        String Name { get; }
        IActivityCategory Category { get; }
        String Notes { get; }
    }
}
