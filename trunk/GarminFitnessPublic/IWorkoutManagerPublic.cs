using System;
using System.Collections.Generic;
using System.IO;

namespace GarminFitnessPublic
{
    public interface IPublicWorkoutManager
    {
        IList<IPublicWorkout> PublicWorkouts { get; }

        void ScheduleWorkout(IPublicWorkout workout, DateTime date);

        void SerializeWorkouts(IList<IPublicWorkout> workouts, String directory);
        void DeserializeWorkout(Stream dataStream);

        void OpenWorkoutInView(IPublicWorkout workout);
    }
}
