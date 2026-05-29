using AutoMapper;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Mapping;
namespace Cls.Application.Mapping;
public class OrderEmployeeMappings : ICustomMap
{
    public void CustomMappings(Profile profile)
    {
        profile.CreateMap<OrderEmployee, OrderEmployeeResponse>()
            .ForMember(d => d.Name, m => m.MapFrom(s => s.User.Name))
            .ForMember(d => d.Email, m => m.MapFrom(s => s.User.Email))
            .ForMember(d => d.ProfileFileId, m => m.MapFrom(s => s.User.FileId))
            .ForMember(d => d.EmployeeId, m => m.MapFrom(s => s.UserId));
    }
}
