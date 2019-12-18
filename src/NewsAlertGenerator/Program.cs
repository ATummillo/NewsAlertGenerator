using NewsAlertGenerator.Enums;
using NewsAlertGenerator.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAlertGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            //MUST CHANGE DEPENDING ON MACHINE
            var inputPath = @"";
            var outputPath = @"";
            var login = "";
            var password = "";
            //var logPath = @"C:\Flow\NewsAlertGenerator\Logs\";
            //##################################

            List<Coin> coinList = PopulateCoinList(inputPath);

            using (IWebDriver driver = new ChromeDriver(@"..\..\..\..\"))
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;

                //Login to Google Account and get redirected to Alerts page
                driver.Navigate().GoToUrl("https://accounts.google.com/ServiceLogin?hl=en&passive=true&continue=https://www.google.com/alerts&service=alerts");
                IWebElement identifier = driver.FindElement(By.Id("identifierId"));
                identifier.SendKeys(login);
                SafeButtonClick(driver, wait, ByType.Id, "identifierNext");
                IWebElement passwordInput = driver.FindElement(By.XPath("//*[@id='password']/div[1]/div/div[1]/input"));
                passwordInput.SendKeys(password);
                SafeButtonClick(driver, wait, ByType.Id, "passwordNext");
    
                //Create and configure an alert for each coin + grab the FeedURL
                foreach (var coin in coinList)
                {
                    //Build new action objects each time since they don't seem to flush the builder properly
                    Actions action1 = new Actions(driver);

                    //Input Keyword
                    IWebElement alertInput = driver.FindElement(By.XPath("//*[@id='query_div']/input"));
                    alertInput.SendKeys(coin.Keyword);

                    //Configure the alert
                    SafeButtonClick(driver, wait, ByType.ClassName, "show_options");
                    MenuItemSelect(driver, wait, "frequency_select", ":0");
                    MenuItemSelect(driver, wait, "source_select", ":8");
                    action1.SendKeys(Keys.Escape).Build().Perform();
                    MenuItemSelect(driver, wait, "delivery_select", ":8l");
                    
                    //Create the alert
                    SafeButtonClick(driver, wait, ByType.Id, "create_alert");

                    //Grab the feed url
                    coin.Feed = GetFeedURL(driver, "//*[@id='manage-alerts-div']/ul/li[1]/div[1]/div/div[2]/a");
                    IWebElement feedLink = driver.FindElement(By.XPath("//*[@id='manage-alerts-div']/ul/li[1]/div[1]/div/div[2]/a"));
                    coin.Feed = feedLink.GetAttribute("href");
                }
                //Output CoinList with FeedURLs added
                OutputCoinsToCSV(coinList, outputPath);
            }
             Environment.Exit(0);
        }

        private static string GetFeedURL(IWebDriver driver, string v)
        {
            IWebElement feedLink = null;
            bool success = false;
            while (!success)
            {
                try
                {
                    feedLink = driver.FindElement(By.XPath(v));
                    success = true;
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("No feed item found! : " + v);
                }
            }
            return feedLink.GetAttribute("href");
        }

        private static void MenuItemSelect(IWebDriver driver, WebDriverWait wait, string menu, string item)
        {
            try
            {
                while (true)
                {
                    SafeMenuClick(driver, wait, ByType.ClassName, menu);
                }
            }
            catch (ElementClickInterceptedException)
            {
                //Do nothing. This only exists to exit the while loop
            }
            SafeMenuClick(driver, wait, ByType.Id, item);
        }

        private static void OutputCoinsToCSV(List<Coin> coinList, string path)
        {
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine("CoinID,CoinName,CoinSymbol,Keyword,FeedURL");
                foreach(var coin in coinList)
                {
                    writer.WriteLine(coin.ID + "," + coin.Name + "," + coin.Symbol + "," + coin.Keyword + "," + coin.Feed);
                }
            }
        }

        private static List<Coin> PopulateCoinList(string v)
        {
            List<Coin> cl = new List<Coin>();
            using (StreamReader reader = new StreamReader(v))
            {
                int counter = 0;
                while (!reader.EndOfStream)
                {
                    cl.Add(new Coin(reader.ReadLine()));
                    counter++;
                }
            }
            return cl;
        }

        private static void SafeButtonClick(IWebDriver driver, WebDriverWait wait, ByType bt, string searchStr)
        {
            bool success = false;
            IWebElement elem;
            while (!success)
            {
                elem = FindByType(driver, bt, searchStr);
                try
                {
                    wait.Until(ExpectedConditions.ElementToBeClickable(elem));                    
                    elem.Click();
                    success = true;
                }
                catch (StaleElementReferenceException)
                {
                    Console.WriteLine("Stale Button: " + searchStr);
                }
                catch (ElementClickInterceptedException)
                {
                    Console.WriteLine("Intercepted Button: " + searchStr);
                }
            }
        }

        private static void SafeMenuClick(IWebDriver driver, WebDriverWait wait, ByType bt, string searchStr)
        {
            bool success = false;
            IWebElement elem;
            while (!success)
            {
                elem = FindByType(driver, bt, searchStr);
                try
                {
                    wait.Until(ExpectedConditions.ElementToBeClickable(elem));
                    elem.Click();
                    success = true;
                }
                catch (StaleElementReferenceException)
                {
                    Console.WriteLine("Stale Menu: " + searchStr);
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("WebDriverTimeoutException: " + searchStr);
                }
            }
        }
        private static IWebElement FindByType(IWebDriver driver, ByType bt, string searchStr)
        {
            switch (bt)
            {
                case ByType.Id:
                    return driver.FindElement(By.Id(searchStr));

                case ByType.XPath:
                    return driver.FindElement(By.XPath(searchStr));

                case ByType.ClassName:
                    return driver.FindElement(By.ClassName(searchStr));
            }
            return null;
        }
    }
}