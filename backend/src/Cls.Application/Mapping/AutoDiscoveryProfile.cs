using System.Reflection;
using AutoMapper;

namespace Cls.Application.Mapping;

public sealed class AutoDiscoveryProfile : Profile
{
    public AutoDiscoveryProfile(Assembly assembly)
    {
        AutoMapperProfile.LoadMaps(this, assembly);
    }
}