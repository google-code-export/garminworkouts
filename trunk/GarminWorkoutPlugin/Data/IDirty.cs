using System;
using System.Collections.Generic;
using System.Text;

namespace GarminWorkoutPlugin.Data
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
