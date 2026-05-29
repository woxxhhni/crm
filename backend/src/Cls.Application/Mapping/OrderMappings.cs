using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Cls.Application.Orders.Commands;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Mapping;

namespace Cls.Application.Mapping;

public class OrderMappings : ICustomMap
{
    public void CustomMappings(Profile profile)
    {
        profile.CreateMap<OrderLogFile, FileResponse>()
            .ForMember(d => d.Id, m => m.MapFrom(s => s.FileId))
            .ForMember(d => d.Name, m => m.MapFrom(s => s.File.OriginalFilename));

        profile.CreateMap<OrderLog, OrderLogResponse>()
            .ForMember(d => d.Title, m => m.MapFrom(c => c.LogType == OrderLogType.NoteAdded ? c.Note.Title : c.Title))
            .ForMember(d => d.Description, m => m.MapFrom(c => c.LogType == OrderLogType.NoteAdded ? c.Note.Description : c.Description))
            .ForMember(d => d.LogDate, m => m.MapFrom(c => c.LogType == OrderLogType.NoteAdded ? c.Note.ActionDate : c.LogDate))
            .ForMember(d => d.Files, m => m.MapFrom(c =>
                                     c.LogType == OrderLogType.NoteAdded ?
                                     c.Note.Files.Select(p => new FileResponse() { Id = p.FileId, Name = p.File.OriginalFilename }) :
                                     c.Files.Select(p => new FileResponse() { Id = p.FileId, Name = p.File.OriginalFilename })))
            .ForMember(d => d.ActorFullName, m => m.MapFrom(c => c.ActorUser.Name))
            .ForMember(d => d.ActorProfileFileId, m => m.MapFrom(c => c.ActorUser.FileId))
            .ForMember(d => d.LogType, m => m.MapFrom(s => s.LogType.ToString().ToLower()))
            .ForMember(d => d.StepName, m => m.MapFrom(s => s.Step.Name))
            .ForMember(d => d.StageId, m => m.MapFrom(s => s.Step.StageId))
            .ForMember(d => d.StageName, m => m.MapFrom(s => s.Step.Stage.Name))
            .ForMember(d => d.FromStepName, m => m.MapFrom(s => s.FromStep.Name))
            .ForMember(d => d.ToStepName, m => m.MapFrom(s => s.ToStep.Name))
            .ForMember(d => d.NoteTitle, m => m.MapFrom(s => s.Note.Title));

        profile.CreateMap<Order, OrderResponse>()
            .ForMember(d => d.Status, m => m.MapFrom(s => s.Status.ToString().ToLower()))
            .ForMember(d => d.ClientName, m => m.MapFrom(s => s.Client.Name))
            .ForMember(d => d.ClientEmail, m => m.MapFrom(s => s.Client.Email))
            .ForMember(d => d.ProviderName, m => m.MapFrom(s => s.Provider.Name))
            .ForMember(d => d.ProviderEmail, m => m.MapFrom(s => s.Provider.Email))
            .ForMember(d => d.CurrentStepId, m => m.MapFrom(s => s.CurrentStepId))
            .ForMember(d => d.CurrentStepName, m => m.MapFrom(s => s.CurrentStep.Name))
            .ForMember(d => d.CurrentStageId, m => m.MapFrom(s => s.CurrentStep.StageId))
            .ForMember(d => d.CurrentStageName, m => m.MapFrom(s => s.CurrentStep.Stage.Name))
            .ForMember(d => d.StageAssignments, m => m.Ignore());

        profile.CreateMap<CreateOrderCommand, Order>()
            .ForMember(d => d.OrderDate, m => m.MapFrom(s => s.OrderDate.ToUniversalTime()));

        profile.CreateMap<UpdateOrderCommand, Order>()
            .ForMember(d => d.OrderDate, m => m.MapFrom(s => s.OrderDate.ToUniversalTime()));

        profile.CreateMap<ExtraProvider, ExtraProviderResponse>()
            .ForMember(d => d.ProviderName, m => m.MapFrom(s => s.Provider.Name))
            .ForMember(d => d.PaidAmount, m => m.MapFrom(s => s.Payments.Sum(c => c.Amount)));
    }
}
