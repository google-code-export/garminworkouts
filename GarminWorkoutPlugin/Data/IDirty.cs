using System;
using System.Collections.Generic;
using System.Text;

namespace GarminFitnessPlugin.Data
{
    interface IDirty
    {
        bool IsDirty
        {
            get;
            set;
        }
    }
}
