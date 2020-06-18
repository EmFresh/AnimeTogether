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
    static FindURLJob jobFind;
    static JobHandle hndFind;
    static string url;
    static bool setField = false;

    struct FindURLJob : IJob
    {
        public void Execute()
        {
            try
            {
                driver.Navigate().GoToUrl(url);
            }
            catch
            {
                CreatePopups.SendPopup("Video URL Not Found");
                return;
            }
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
                        InitSettings.videoURL = driver.FindElement(By.TagName("source")).GetAttribute("src");

                        CreatePopups.SendPopup("Found video URL!!!");
                        setField = true;
                        noURLFound = false;
                    }
                    catch {}

                    //break;
                    driver.SwitchTo().ParentFrame();
                }
            }

            if (noURLFound)
                CreatePopups.SendPopup("No video URL Found");

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
            //options.BinaryLocation = Application.dataPath + bin+"/chromedriver.exe";
            options.AddArgument("--headless");
            //options.AddArgument("--debug-devtools");

            //  options.BrowserExecutableLocation = Application.dataPath + bin;

            var service = ChromeDriverService.CreateDefaultService(Application.dataPath + bin);
            /*  
                An address of a Chrome debugger server to connect to, in the form of<hostname / ip : port>, e.g.
                '127.0.0.1:38947' 
            */

#if UNITY_EDITOR
            service.HideCommandPromptWindow = false;
#endif

            //service.PortServerAddress = "127.0.0.1";
            //options.PlatformName = @"windows";

            driver = new ChromeDriver(Application.dataPath + bin 
               , options);

            // ((ChromeDriver)driver).hide();
        }
    }

    void Update()
    {
        if (setField)
        {
            vidURL.text = InitSettings.videoURL;
            setField = false;
        }
    }
    public void URLStringFinder(string url)
    {

        if (driver != null)
        {
            if (hndFind.IsCompleted)
            {
                HtmlVideoFinder.url = url;
                CreatePopups.SendPopup("Finding video URL...\nCan take longer depending on internet connectivity");
                jobFind = new FindURLJob();
                hndFind = jobFind.Schedule();
            }
            else
                CreatePopups.SendPopup("Still going...\nHave some dam patience :<");

        }
        else
            CreatePopups.SendPopup("Driver Not Initialized");

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