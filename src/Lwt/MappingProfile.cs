using AutoMapper;
using Lwt.Models;
using Lwt.ViewModels.User;

namespace Lwt
{
    public class MappingProfile  : Profile
    {
        public MappingProfile()
        {
            CreateMap<SignUpViewModel, User>();
        }
    }
}
