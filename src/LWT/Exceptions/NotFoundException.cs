using System;

namespace Lwt.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string description) : base(description)
        {
        }
    }
}