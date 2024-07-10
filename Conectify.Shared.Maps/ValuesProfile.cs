namespace Conectify.Shared.Maps;

using AutoMapper;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Values;
using Conectify.Shared.Library.Models.Websocket;

public class ValuesProfile : Profile
{
    public ValuesProfile()
    {
        CreateMap<Value, ApiValue>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Value)));
        CreateMap<Value, WebsocketBaseModel>()
             .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Value)))
             .ForMember(x => x.DestinationId, opt => opt.Ignore())
             .ForMember(x => x.ResponseSourceId, opt => opt.Ignore());
        CreateMap<WebsocketBaseModel, Value>()
            .ForMember(x => x.Source, opt => opt.Ignore());
        CreateMap<ApiValue, Value>()
            .ForMember(x => x.Source, opt => opt.Ignore());

        CreateMap<Action, ApiAction>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Action)));
        CreateMap<Action, WebsocketBaseModel>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Action)))
            .ForMember(x => x.ResponseSourceId, opt => opt.Ignore());
        CreateMap<WebsocketBaseModel, Action>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Destination, opt => opt.Ignore());
        CreateMap<ApiAction, Action>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Destination, opt => opt.Ignore());

        CreateMap<Command, ApiCommand>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Command)));
        CreateMap<Command, WebsocketBaseModel>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Command)))
            .ForMember(x => x.ResponseSourceId, opt => opt.Ignore());
        CreateMap<WebsocketBaseModel, Command>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Destination, opt => opt.Ignore());
        CreateMap<ApiCommand, Command>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Destination, opt => opt.Ignore());

        CreateMap<ActionResponse, ApiActionResponse>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(ActionResponse)));
        CreateMap<ActionResponse, WebsocketBaseModel>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(ActionResponse)))
            .ForMember(x => x.DestinationId, opt => opt.Ignore())
            .ForMember(x => x.ResponseSourceId, opt => opt.MapFrom(x => x.ActionId));
        CreateMap<WebsocketBaseModel, ActionResponse>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Action, opt => opt.Ignore())
            .ForMember(x => x.ActionId, opt => opt.MapFrom(x => x.ResponseSourceId));
        CreateMap<ApiActionResponse, ActionResponse>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Action, opt => opt.Ignore());

        CreateMap<CommandResponse, ApiCommandResponse>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(CommandResponse)));
        CreateMap<CommandResponse, WebsocketBaseModel>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(CommandResponse)))
            .ForMember(x => x.DestinationId, opt => opt.Ignore())
            .ForMember(x => x.ResponseSourceId, opt => opt.MapFrom(x => x.CommandId));
        CreateMap<WebsocketBaseModel, CommandResponse>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Command, opt => opt.Ignore())
            .ForMember(x => x.CommandId, opt => opt.MapFrom(x => x.ResponseSourceId));
        CreateMap<ApiCommandResponse, CommandResponse>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Command, opt => opt.Ignore());

        CreateMap<Command, ApiDataModel>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Command)))
            .ForMember(x => x.ResponseSourceId, opt => opt.Ignore());
        CreateMap<Action, ApiDataModel>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Action)))
            .ForMember(x => x.ResponseSourceId, opt => opt.Ignore());


    }
}
