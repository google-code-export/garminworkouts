using System;
using System.Collections.Generic;
using System.Text;

namespace GarminFitnessPlugin.Controller
{
    public class FITParserException : Exception
    {
        public FITParserException(string message) :
            base(message)
        {
        }
    }
}
