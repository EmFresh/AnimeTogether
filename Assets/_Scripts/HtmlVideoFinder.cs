using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using TMPro;
using Unity.Jobs;
using UnityEngine;

public class HtmlVideoFinder : MonoBehaviour
{
    public TMP_InputField vidURL;
    static IWebDriver driver;
    static ActiveSearchJob searchJob;
    static JobHandle hndSearch;
    static string url;
    static bool setField = false;
    
    struct ActiveSearchJob : IJob
    {

        public void Execute()
        {

            try
            {
                ReadOnlyCollection<IWebElement> ele = driver.FindElements(By.TagName("iframe"));
                // driver.Capabilities.BrowserName;

                string str;

                // OpenQA.Selenium.
                bool noURLFound = true;
                foreach (var element in ele)
                {
                    str = element.GetAttribute("src");

                    //for mp4upload
                    if (str.Contains("mp4upload"))
                    {
                        driver.SwitchTo().Frame(element);

                        try
                        {
                            if (InitSettings.videoURL == driver.FindElement(By.TagName("source")).GetAttribute("src"))
                            {
                                driver.SwitchTo().ParentFrame();
                                continue;
                            }

                            InitSettings.videoURL = driver.FindElement(By.TagName("source")).GetAttribute("src");

                            CreatePopups.SendPopup("Found video URL!!!", true);
                            setField = true;
                            noURLFound = false;
                         }
                        catch {}

                        //break;
                        driver.SwitchTo().ParentFrame();
                    }

                    //for streamtape
                    if (str.Contains("streamtape"))
                    {
                        driver.SwitchTo().Frame(element);

                        try
                        {
                            if (InitSettings.videoURL == driver.FindElement(By.TagName("video")).GetAttribute("src"))
                            {
                                driver.SwitchTo().ParentFrame();
                                continue;
                            }

                            InitSettings.videoURL = driver.FindElement(By.TagName("video")).GetAttribute("src");

                            CreatePopups.SendPopup("Found video URL!!!", true);
                            setField = true;
                            noURLFound = false;
                        
                        }
                        catch {}

                        //break;
                        driver.SwitchTo().ParentFrame();

                    }

                }
            }
            catch {}

        }
    }

#if UNITY_EDITOR
    const string bin = "";
#else
    const string bin = "/..";
#endif

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
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
#if UNITY_EDITOR
            service.HideCommandPromptWindow = true;
#endif

            driver = new ChromeDriver(service, options);
            //driver = new FirefoxDriver(service, options);
            driver.Navigate().GoToUrl(@"https://9anime.ru/filter?type%5B%5D=series&type%5B%5D=ova&type%5B%5D=ona&type%5B%5D=special&language%5B%5D=dubbed&sort=episode_last_added_at%3Adesc");
        }

        searchJob = new ActiveSearchJob();
        hndSearch = searchJob.Schedule();

    }

    void Update()
    {
        if (setField)
        {
            vidURL.text = InitSettings.videoURL;
            setField = false;
        }

        if (hndSearch.IsCompleted)
            hndSearch = searchJob.Schedule();

    }

    public void URLStringFinder(string url)
    {

        //if (driver != null)
        //{
        //    if (hndFind.IsCompleted)
        //    {
        //        HtmlVideoFinder.url = url;
        //        CreatePopups.SendPopup("Finding video URL...\nCan take longer depending on internet connectivity");
        //        jobFind = new FindURLJob();
        //        hndFind = jobFind.Schedule();
        //    }
        //    else
        //        CreatePopups.SendPopup("Still going...\nHave some dam patience :<");
        //
        //}
        //else
        //    CreatePopups.SendPopup("Driver Not Initialized");

    }

    public static void VideoFinderQuit()
    {
        if (driver != null)
            driver.Quit();
    }

    /// <summary>
    /// Callback sent to all game objects before the application is quit.
    /// </summary>
    void OnApplicationQuit()
    {
        VideoFinderQuit();
        //        driver.Close();
    }

}