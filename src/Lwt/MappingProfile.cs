namespace Lwt
{
    using AutoMapper;

    using Lwt.Models;
    using Lwt.ViewModels;
    using Lwt.ViewModels.User;

    /// <summary>
    /// a.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingProfile"/> class.
        /// </summary>
        public MappingProfile()
        {
            this.CreateMap<SignUpViewModel, User>();
            this.CreateMap<TextCreateModel, Text>();
            this.CreateMap<Text, TextViewModel>();
        }
    }
}