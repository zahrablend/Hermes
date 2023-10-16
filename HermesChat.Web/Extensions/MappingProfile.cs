using HermesChat.Data.Models;
using HermesChat.Web.Models;

namespace HermesChat.Web.Extensions
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<UserRegistrationModel, User>()
                .ForMember(u => u.UserName, opt => opt.MapFrom(x => x.Name));

            CreateMap<Profile, UserProfile>().ReverseMap();

            CreateMap<User, UserProfile>()
            .ForMember(p => p.Name, opt => opt.MapFrom(u => u.Name))
            .ForMember(p => p.Email, opt => opt.MapFrom(u => u.Email));
        }
    }
}
