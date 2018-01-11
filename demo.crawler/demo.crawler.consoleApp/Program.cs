using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SimpleBrowser;

namespace demo.crawler.consoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Test HttpClient - C# HTTP GET request synchronous example
            GetPlaceHolderOnHttpClient();

            // Test SimpleBrowser
            GetSeriesOnSimpleBrowser();
        }

        #region HttpClient Demo - API for tests

        static void GetPlaceHolderOnHttpClient()
        {
            using (var client = new HttpClient())
            {
                var url = "https://jsonplaceholder.typicode.com/posts/1";
                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    // by calling .Result you are performing a synchronous call
                    var responseContent = response.Content;

                    // by calling .Result you are synchronously reading the result
                    string responseString = responseContent.ReadAsStringAsync().Result;

                    Console.WriteLine(responseString);
                }
            }
        }

        #endregion

        #region Simple Browser Demo - Search for series on calendar 

        static void GetSeriesOnSimpleBrowser()
        {
            var browser = new Browser();
            try
            {
                // log the browser request/response data to files so we can interrogate them in case of an issue with our scraping
                browser.RequestLogged += OnBrowserRequestLogged;
                browser.MessageLogged += new Action<Browser, string>(OnBrowserMessageLogged);

                // we'll fake the user agent for websites that alter their content for unrecognised browsers
                browser.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.10 (KHTML, like Gecko) Chrome/8.0.552.224 Safari/534.10";

                // browse to GitHub
                browser.Navigate("http://www.pogdesign.co.uk/cat/");
                if (LastRequestFailed(browser)) return; // always check the last request in case the page failed to load

                browser.Log("First we need do find a Title with month and year description.");

                var titleLink = browser.Find("a", FindBy.Text, "August 2016 TV Episode Calendar");
                if (!titleLink.Exists)
                {
                    browser.Log("Can't find the link! Perhaps the site is down for maintenance?");
                    return;
                }

                browser.Log("Then we find the yesterday column.");

                // Obter dia
                var dayDiv = browser.Find("div", FindBy.Id, "d_1_8_2016");
                if (!dayDiv.Exists)
                {
                    browser.Log("Can't find the column! Perhaps dont have any series for the month.");
                    return;
                }

                var serieLink = browser.Find("a href", FindBy.PartialText, "-summary");
                if (serieLink.Exists)
                {
                    Console.WriteLine(serieLink.TotalElementsFound);

                    Console.WriteLine(serieLink.FirstOrDefault().Value);
                }

                //// click the login link and click it
                //browser.Log("First we need to log in, so browse to the login page, fill in the login details and submit the form.");
                //var loginLink = browser.Find("a", FindBy.Text, "Login");
                //if (!loginLink.Exists)
                //    browser.Log("Can't find the login link! Perhaps the site is down for maintenance?");
                //else
                //{
                //    loginLink.Click();
                //    if (LastRequestFailed(browser)) return;

                //    // fill in the form and click the login button - the fields are easy to locate because they have ID attributes
                //    browser.Find("login_field").Value = "youremail@domain.com";
                //    browser.Find("password").Value = "yourpassword";
                //    browser.Find(ElementType.Button, "name", "commit").Click();
                //    if (LastRequestFailed(browser)) return;

                //    // see if the login succeeded - ContainsText() is very forgiving, so don't worry about whitespace, casing, html tags separating the text, etc.
                //    if (browser.ContainsText("Incorrect login or password"))
                //    {
                //        browser.Log("Login failed!", LogMessageType.Error);
                //    }
                //    else
                //    {
                //        // After logging in, we should check that the page contains elements that we recognise
                //        if (!browser.ContainsText("Your Repositories"))
                //            browser.Log("There wasn't the usual login failure message, but the text we normally expect isn't present on the page");
                //        else
                //        {
                //            browser.Log("Your News Feed:");
                //            // we can use simple jquery selectors, though advanced selectors are yet to be implemented
                //            foreach (var item in browser.Select("div.news .title"))
                //                browser.Log("* " + item.Value);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                browser.Log(ex.Message, LogMessageType.Error);
                browser.Log(ex.StackTrace, LogMessageType.StackTrace);
            }
            finally
            {
                var path = WriteFile("log-" + DateTime.UtcNow.Ticks + ".html", browser.RenderHtmlLogFile("SimpleBrowser Sample - Request Log"));
                //Process.Start(path);
            }

            Console.ReadKey();
        }

        static bool LastRequestFailed(Browser browser)
        {
            if (browser.LastWebException != null)
            {
                browser.Log("There was an error loading the page: " + browser.LastWebException.Message);
                return true;
            }
            return false;
        }

        static void OnBrowserMessageLogged(Browser browser, string log)
        {
            Console.WriteLine(log);
        }

        static void OnBrowserRequestLogged(Browser req, HttpRequestLog log)
        {
            Console.WriteLine(" -> " + log.Method + " request to " + log.Url);
            Console.WriteLine(" <- Response status code: " + log.ResponseCode);
        }

        static string WriteFile(string filename, string text)
        {
            var dir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));
            if (!dir.Exists) dir.Create();
            var path = Path.Combine(dir.FullName, filename);
            File.WriteAllText(path, text);
            return path;
        }

        #endregion
    }
}