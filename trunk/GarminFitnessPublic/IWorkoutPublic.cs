using System;
using ZoneFiveSoftware.Common.Data.Fitness;

namespace GarminFitnessPublic
{
    public interface IPublicWorkout
    {
        Guid PublicId { get; }
        String PublicName { get; }
        IActivityCategory PublicCategory { get; }
        String PublicNotes { get; }
    }
}
