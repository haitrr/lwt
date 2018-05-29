using AutoMapper;
using Lwt.Models;
using Lwt.ViewModels;
using Lwt.ViewModels.User;
using LWT.Models;

namespace Lwt
{
    public class MappingProfile  : Profile
    {
        public MappingProfile()
        {
            CreateMap<SignUpViewModel, User>();
            CreateMap<TextCreateModel,Text>();
            CreateMap<Text,TextViewModel>();
        }
    }
}
