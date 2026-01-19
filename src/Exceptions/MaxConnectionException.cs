using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Exceptions
{
    public class MaxConnectionException : Exception
    {
        public MaxConnectionException(string message) : base(message) { }
    }
}