using System;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.SportTracks.Device.GarminGPS;
using GarminFitnessPlugin.View;

namespace GarminFitnessPlugin.Controller
{
    class NoDeviceSupportException : Exception
    {
        public NoDeviceSupportException(Device device, String operation) :
            base(String.Format(GarminFitnessView.GetLocalizedString("NoDeviceSupportText"),
                               device.Description, operation))
        {
        }
    }
}