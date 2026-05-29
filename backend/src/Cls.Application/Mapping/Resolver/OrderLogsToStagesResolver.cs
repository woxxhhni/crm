//using AutoMapper;
//using Cls.Domain.Entities;
//using Cls.Shared.Contracts.Orders;
//using System.Collections.Generic;
//using System.Linq;

//namespace Cls.Application.Mapping.Resolver;

//public sealed class OrderLogsToStagesResolver
//    : IValueResolver<Order, OrderResponse, List<OrderStageLogsResponse>>
//{
//    // IMPORTANT: public parameterless ctor
//    public OrderLogsToStagesResolver()
//    {
//    }

//    public List<OrderStageLogsResponse> Resolve(
//        Order source,
//        OrderResponse destination,
//        List<OrderStageLogsResponse> destMember,
//        ResolutionContext context)
//    {
//        if (source.Logs == null || source.Logs.Count == 0)
//            return new();

//        return source.Logs
//            .Where(l => l.Step != null && l.Step.Stage != null)
//            .GroupBy(l => l.Step!.Stage!) // Stage level
//            .Select(stageGroup => new OrderStageLogsResponse
//            {
//                StageId = stageGroup.Key.Id,
//                StageName = stageGroup.Key.Name,
//                Steps = stageGroup
//                    .GroupBy(l => l.Step!)
//                    .Select(stepGroup => new OrderStepLogsResponse
//                    {
//                        StepId = stepGroup.Key.Id,
//                        StepName = stepGroup.Key.Name,
//                        Logs = stepGroup
//                            .Select(l => context.Mapper.Map<OrderLogResponse>(l))
//                            .ToList()
//                    })
//                    .ToList()
//            })
//            .ToList();
//    }
//}
