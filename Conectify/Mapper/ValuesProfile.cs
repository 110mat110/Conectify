namespace Conectify.Server.Mapper;

using AutoMapper;
using Conectify.Database.Models.Values;

public class ValuesProfile : Profile
{
    public ValuesProfile()
    {
        CreateMap<Value, ApiValue>();
        CreateMap<ApiValue, Value>()
            .ForMember(x => x.Source, opt => opt.Ignore());

        CreateMap<Action, ApiAction>();
        CreateMap<ApiAction, Action>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Destination, opt => opt.Ignore());

        CreateMap<Command, ApiCommand>();
        CreateMap<ApiCommand, Command>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Destination, opt => opt.Ignore());

        CreateMap<ActionResponse, ApiActionResponse>();
        CreateMap<ApiActionResponse, ActionResponse>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Action, opt => opt.Ignore());

        CreateMap<CommandResponse, ApiCommandResponse>();
        CreateMap<ApiCommandResponse, CommandResponse>()
            .ForMember(x => x.Source, opt => opt.Ignore())
            .ForMember(x => x.Command, opt => opt.Ignore());
    }
}
