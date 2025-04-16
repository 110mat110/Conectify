﻿using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace Conectify.Services.OccupancyCheck;

public class OccupancyService(IServicesWebsocketClient websocketClient, Configuration configuration)
{
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

                    var value = new WebsocketEvent()
                    {
                        Name = "Occupancy",
                        NumericValue = result ? 1 : 0,
                        StringValue = !result ? "no one home" : "",
                        SourceId = configuration.SensorId,
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        Unit = "",
                        Type = Constants.Events.Value,
                    };
                    await websocketClient.SendMessageAsync(value);

                    await Task.Delay(new TimeSpan(0, 0, 59));

                    driver.Navigate().Refresh();
                    await Task.Delay(new TimeSpan(0, 0, 1));
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