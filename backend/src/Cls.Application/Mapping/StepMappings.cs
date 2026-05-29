using AutoMapper;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Steps;
using Cls.Shared.Mapping;

namespace Cls.Application.Mapping;

public class StepMappings : ICustomMap
{
    public void CustomMappings(Profile profile)
    {
        profile.CreateMap<Step, StepResponse>()
               .ForMember(d => d.StageName, m => m.MapFrom(s => s.Stage.Name));
    }
}
