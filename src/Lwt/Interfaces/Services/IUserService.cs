using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lwt.Models;

namespace Lwt.Interfaces.Services
{
    public interface IUserService
    {
        bool SignUp(User newUser);
    }
}
