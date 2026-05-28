using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Exceptions
{
    public class EmailException : Exception
    {
        public EmailException(string message) : base(message) { }
    }

    public class PinException : Exception
    {
        public PinException(string message) : base(message) { }
    }

    public class UserException : Exception
    {
        public UserException(string message) : base(message) { }
    }

    public class FailedException : Exception
    {
        public FailedException(string message) : base(message) { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}