using AutoMapper;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.General;
using Cls.Shared.Mapping;

namespace Cls.Application.Mapping;

public class CurrencyMappings : ICustomMap
{
    public void CustomMappings(Profile profile)
    {
        profile.CreateMap<Currency, CurrencyResponse>();
    }
}
