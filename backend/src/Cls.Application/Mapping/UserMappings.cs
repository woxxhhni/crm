using AutoMapper;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Mapping;
namespace Cls.Application.Mapping;
public class UserMappings : ICustomMap
{
    public void CustomMappings(Profile profile)
    {
        profile.CreateMap<User, UserResponse>()
            .ForMember(d => d.Role, m => m.MapFrom(s => s.Role.ToString().ToLower()))
            .ForMember(d=>d.UserProfileFileId, m => m.MapFrom(c=>c.FileId));

        profile.CreateMap<User, MeResponse>()
            .ForMember(d => d.Role, m => m.MapFrom(s => s.Role.ToString().ToLower()))
            .ForMember(d => d.ProfileFileId, m => m.MapFrom(c => c.FileId));
    }
}
