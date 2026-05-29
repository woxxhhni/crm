using AutoMapper;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Clients;
using Cls.Shared.Mapping;
namespace Cls.Application.Mapping;
public class ClientMappings : ICustomMap
{
    public void CustomMappings(Profile profile)
    {
        profile.CreateMap<Client, ClientResponse>();
    }
}
