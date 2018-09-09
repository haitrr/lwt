using System;

namespace Lwt.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException()
        {
        }

        public BadRequestException(string description) : base(description)
        {
        }
    }
}