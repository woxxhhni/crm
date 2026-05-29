using AutoMapper;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Clients.FileGroups;
using Cls.Shared.Contracts.Providers.FileGroups;
using Cls.Shared.Mapping;

namespace Cls.Application.Mapping;

public class FileGroupMappings : ICustomMap
{
    public void CustomMappings(Profile profile)
    {
        profile.CreateMap<ClientFileGroup, ClientFileGroupResponse>();
        //.ForMember(d => d.FileIds, m => m.MapFrom(s => s.Items.Select(i => i.FileId).ToList()));

        profile.CreateMap<ProviderFileGroup, ProviderFileGroupResponse>();
        //.ForMember(d => d.FileIds, m => m.MapFrom(s => s.Items.Select(i => i.FileId).ToList()));
    }
}
