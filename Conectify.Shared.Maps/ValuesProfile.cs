namespace Conectify.Shared.Maps;

using AutoMapper;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library.Models.Values;
using Conectify.Shared.Library.Models.Websocket;

public class ValuesProfile : Profile
{
    public ValuesProfile()
    {
        CreateMap<Value, ApiValue>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Value)));
        CreateMap<Value, WebsocketValue>()
             .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Value)));
        CreateMap<WebsocketValue, Value>()
            .ForMember(x => x.Source, opt => opt.Ignore());
        CreateMap<ApiValue, Value>()
            .ForMember(x => x.Source, opt => opt.Ignore());

        CreateMap<Action, ApiAction>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Action)));
        CreateMap<Action, WebsocketAction>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Action)));
        CreateMap<WebsocketAction, Action>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Destination, opt => opt.Ignore());
        CreateMap<ApiAction, Action>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Destination, opt => opt.Ignore());

        CreateMap<Command, ApiCommand>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Command)));
        CreateMap<Command, WebsocketCommand>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(Command)));
        CreateMap<WebsocketCommand, Command>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Destination, opt => opt.Ignore());
        CreateMap<ApiCommand, Command>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Destination, opt => opt.Ignore());

        CreateMap<ActionResponse, ApiActionResponse>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(ActionResponse)));
        CreateMap<ActionResponse, WebsocketActionResponse>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(ActionResponse)));
        CreateMap<WebsocketActionResponse, ActionResponse>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Action, opt => opt.Ignore());
        CreateMap<ApiActionResponse, ActionResponse>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Action, opt => opt.Ignore());

        CreateMap<CommandResponse, ApiCommandResponse>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(CommandResponse)));
        CreateMap<CommandResponse, WebsocketCommandResponse>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => nameof(CommandResponse)));
        CreateMap<WebsocketCommandResponse, CommandResponse>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Command, opt => opt.Ignore());
        CreateMap<ApiCommandResponse, CommandResponse>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Command, opt => opt.Ignore());
    }
}
