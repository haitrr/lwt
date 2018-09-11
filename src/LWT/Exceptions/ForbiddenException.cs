using System;

namespace Lwt.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string description) : base(description)
        {
        }
    }
}