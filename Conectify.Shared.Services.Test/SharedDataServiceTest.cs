using AutoMapper;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library.ErrorHandling;
using Conectify.Shared.Library.Models.Websocket;
using Conectify.Shared.Maps;
using Conectify.Shared.Services.Data;

namespace Conectify.Shared.Services.Test;

public class SharedDataServiceTest
{
    IMapper Mapper;

    public SharedDataServiceTest()
    {
        Mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EventProfile>();
        }).CreateMapper();
    }

    [Fact]
    public void ItShallThrowWhenInvalidJson()
    {
        try
        {
            SharedDataService.DeserializeJson("!@#$", Mapper);
        }
        catch (ConectifyException ex)
        {
            Assert.Matches(ex.Message, "Json to deserialize is not an event!!");
        }
    }

    [Fact]
    public void ItShallFailWhenValidJsonWithoutType()
    {
        try
        {
            SharedDataService.DeserializeJson("{\"test\":\"test\"", Mapper);
        }
        catch (ConectifyException ex)
        {
            Assert.Matches(ex.Message, "Json to deserialize is not an event!!");
        }
    }

    [Fact]
    public void ItShallFailWhenValidJsonHasInvalidType()
    {
        try
        {
            SharedDataService.DeserializeJson("{\"Type\":\"test\" }", Mapper);
        }
        catch (ConectifyException ex)
        {
            Assert.Matches(ex.Message, "Does not recognize type test");
        }
    }

    [Fact]
    public void ItShallFailOnMapper()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
        }).CreateMapper();

        var websocketValue = new WebsocketEvent()
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            NumericValue = 10,
            SourceId = Guid.NewGuid(),
            StringValue = "Test",
            TimeCreated = 10000,
            Type = "Value",
            Unit = "Test"
        }.ToJson();

        try
        {
            SharedDataService.DeserializeJson(websocketValue, mapper);
        }
        catch (AutoMapperMappingException)
        {
            Assert.True(true);
        }
    }

    [Theory]
    [InlineData(typeof(Event))]
    public void ItShallPass(Type valueType)
    {

        var websocketValue = new WebsocketEvent()
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            NumericValue = 10,
            SourceId = Guid.NewGuid(),
            StringValue = "Test",
            TimeCreated = 10000,
            Type = valueType.Name,
            Unit = "Test"
        }.ToJson();

        var result = SharedDataService.DeserializeJson(websocketValue, Mapper);

        Assert.Equal("Test", result.Name);
        Assert.Equal(10, result.NumericValue);
    }

}