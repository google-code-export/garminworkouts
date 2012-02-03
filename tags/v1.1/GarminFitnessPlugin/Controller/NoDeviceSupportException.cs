using System;
using System.Collections.Generic;
using System.Text;
using GarminFitnessPlugin.View;

namespace GarminFitnessPlugin.Controller
{
    class NoDeviceSupportException : Exception
    {
        public NoDeviceSupportException(IGarminDevice device, String operation) :
            base(String.Format(GarminFitnessView.GetLocalizedString("NoDeviceSupportText"),
                               device.DisplayName, operation))
        {
        }
    }
}