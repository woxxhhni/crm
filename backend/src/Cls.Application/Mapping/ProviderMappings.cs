using AutoMapper;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Providers;
using Cls.Shared.Mapping;
namespace Cls.Application.Mapping;
public class ProviderMappings : ICustomMap
{
    public void CustomMappings(Profile profile)
    {
        profile.CreateMap<Provider, ProviderResponse>();
    }
}
