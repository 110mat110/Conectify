using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace Conectify.Services.OccupancyCheck;

public class OccupancyService
{
    private readonly IServicesWebsocketClient websocketClient;
    private readonly Configuration configuration;

    public OccupancyService(IServicesWebsocketClient websocketClient, Configuration configuration)
    {
        this.websocketClient = websocketClient;
        this.configuration = configuration;
    }

    public async Task CheckForLiveDevices()
    {
        do
        {
            ChromeOptions options = new();
            //options.AddRemoteDebuggerEndpoint("http://localhost:4444");
            options.AddArgument("--ignore-ssl-errors=yes");
            options.AddArgument("--ignore-certificate-errors");
            //options.AddArgument("--headless");


            IWebDriver driver = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), options);
            //var driver = new EdgeDriver();
            try
            {
                driver.Navigate().GoToUrl(configuration.IpToSearch);
                var login = new WebDriverWait(driver, new TimeSpan(0, 0, 30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("password-text")));
                login.SendKeys(configuration.Password);

                //var loginButton = driver.FindElement(By.ClassName("button-button"));
                //loginButton.Click();
                var result = false;
                do
                {
                    var clients = new WebDriverWait(driver, new TimeSpan(0, 0, 30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("map-clients")));
                    clients.Click();

                    new WebDriverWait(driver, new TimeSpan(0, 0, 30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.ClassName("mac")));
                    var macs = driver.FindElements(By.XPath("//tr")).Select(x => x.GetAttribute("data-key")).Where(x => !string.IsNullOrEmpty(x)).ToList();

                    var searchedMacs = configuration.MacAdresses.Select(x => x.Replace("-", "").Replace(":", "").ToLower());

                    result = macs.Any(x => searchedMacs.Contains(x.ToLower()));

                    Console.WriteLine(result);

                    if (!result)
                    {
                        var value = new WebsocketBaseModel()
                        {
                            Name = "Occupancy",
                            NumericValue = 0,
                            StringValue = "no one home",
                            SourceId = this.configuration.SensorId,
                            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            Unit = "",
                            Type = Constants.Types.Value,
                        };
                        await websocketClient.SendMessageAsync(value);
                    }

                    await Task.Delay(new TimeSpan(0, 0, 59));

                    driver.Navigate().Refresh();
                } while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                driver.Quit();
            }
        } while (true);
    }
      
}