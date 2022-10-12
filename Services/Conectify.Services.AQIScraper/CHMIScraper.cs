using Conectify.Services.Library;
using Conectify.Shared.Library.Models.Values;
using Conectify.Shared.Library.Models.Websocket;
using HtmlAgilityPack;
using System.Globalization;
using System.Net.Http.Headers;

namespace Conectify.Services.AQIScraper;

public class CHMIScraper
{
    private readonly IServicesWebsocketClient websocketClient;
    private readonly Conectify.Services.AQIScraper.Configuration configuration;
    private readonly ILogger<CHMIScraper> logger;

    public CHMIScraper(IServicesWebsocketClient websocketClient, Configuration configuration, ILogger<CHMIScraper> logger)
    {
        this.websocketClient = websocketClient;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task LoadNewValues()
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

        string res = await client.GetStringAsync("https://www.chmi.cz/files/portal/docs/uoco/web_generator/aqindex_slide3h3/mp_TOPOA_CZ.html");
        HtmlDocument pageDocument = new();
        pageDocument.LoadHtml(res);
        float AQI;
        string status = string.Empty;
        try { 
            var x = pageDocument.DocumentNode.SelectSingleNode("//body");
            status = pageDocument.DocumentNode.SelectSingleNode("//body//div[@id='main']//div[@id='content']//table[2]//tr[last()]//td[2]//span").InnerText;
            string aqi = pageDocument.DocumentNode.SelectSingleNode("//body//div[@id='main']//div[@id='content']//table[2]//tr[last()]//td[5]").InnerText;
            
            AQI = float.Parse(aqi.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));

            var value = new WebsocketValue()
            {
                Name = "AQI",
                NumericValue = AQI,
                StringValue = status,
                SourceId = this.configuration.SensorId,
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Unit = "PM10",
                Type = "Value",
            };
            logger.LogInformation($"Got values! AQI is {AQI}");
            await websocketClient.SendMessageAsync(value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to scrape AQI");
        }

    }
}
