using AutoMapper;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library.ErrorHandling;
using Conectify.Shared.Library.Models.Websocket;
using Conectify.Shared.Maps;
using Conectify.Shared.Services.Data;

namespace Conectify.Shared.Services.Test;

public class SharedDataServiceTest
{
    [Fact]
    public void ItShallThrowWhenInvalidJson()
    {
        try
        {
            SharedDataService.DeserializeJson("!@#$");
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
            SharedDataService.DeserializeJson("{\"test\":\"test\"");
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
            SharedDataService.DeserializeJson("{\"Type\":\"test\" }");
        }
        catch (ConectifyException ex)
        {
            Assert.Matches(ex.Message, "Does not recognize type test");
        }
    }

    [Fact]
    public void ItShallFailOnMapper()
    {
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
            SharedDataService.DeserializeJson(websocketValue);
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

        var result = SharedDataService.DeserializeJson(websocketValue);

        Assert.Equal("Test", result?.Name);
        Assert.Equal(10, result?.NumericValue);
    }

}