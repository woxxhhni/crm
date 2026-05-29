using AutoMapper;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Mapping;
namespace Cls.Application.Mapping;
public class OrderUniqueNumberMappings : ICustomMap
{
    public void CustomMappings(Profile profile)
    {
        profile.CreateMap<OrderUniqueNumber, OrderUniqueNumberResponse>();
    }
}
