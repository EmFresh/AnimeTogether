using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace VideoFinder
{
    public class Utility
    {
        public static void StartDriver()
        {

            if (driver == null)
            {
                ChromeOptions options = new ChromeOptions();

                options.AddExtension(Application.dataPath + bin + @"/uBlock0.crx");
                //options.AddArgument("--headless");

                var service = ChromeDriverService.CreateDefaultService(Application.dataPath + bin);
                /*  
                    An address of a Chrome debugger server to connect to, in the form of<hostname / ip : port>, e.g.
                    '127.0.0.1:38947' 
                */

                //FirefoxOptions options = new FirefoxOptions();
                //options.BrowserExecutableLocation = @"C:\Program Files\Mozilla Firefox\firefox.exe";
                //options.Profile = new FirefoxProfile(Application.dataPath + bin + @"/My Profile", false);
                ////options.Profile.AlwaysLoadNoFocusLibrary = true;
                ////options.Profile.DeleteAfterUse = false;
                ////options.Profile.AcceptUntrustedCertificates = true;
                ////options.Profile.AddExtension(Application.dataPath + bin + @"/My Profile/extensions/uBlock0_1.27.10.firefox.xpi");
                ////options.AddArgument("-headles");

                //  FirefoxDriverService service = FirefoxDriverService.CreateDefaultService(Application.dataPath + bin);

                driver = new ChromeDriver(service, options);
                //driver = new FirefoxDriver(service, options);
                driver.Navigate().GoToUrl(@"https://9anime.ru/filter?type%5B%5D=series&type%5B%5D=ova&type%5B%5D=ona&type%5B%5D=special&language%5B%5D=dubbed&sort=episode_last_added_at%3Adesc");
            }

        }
    }
}