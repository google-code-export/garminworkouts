using System;
using System.Collections.Generic;
using System.IO;

namespace GarminFitnessPublic
{
    public interface IPublicProfileManager
    {
        void SerializeProfile(String directory);
    }
}
