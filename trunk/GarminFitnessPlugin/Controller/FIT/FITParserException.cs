using System;
using System.Collections.Generic;
using System.Text;

namespace GarminFitnessPlugin.Controller
{
    class FITParserException : Exception
    {
        public FITParserException(string message) :
            base(message)
        {
        }
    }
}
