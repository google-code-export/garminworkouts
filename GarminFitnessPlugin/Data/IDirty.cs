using System;
using System.Collections.Generic;
using System.Text;

namespace GarminFitnessPlugin.Data
{
    public interface IDirty
    {
        bool IsDirty
        {
            get;
            set;
        }
    }
}
