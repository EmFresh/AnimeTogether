using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
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
          //  ScrapingBrowser browser = new ScrapingBrowser();
          //  // browser.NavigateToPage();
          //  var browser2 = browser.NavigateToPage(new Uri(url));
//
          //  var list = browser2.Html.CssSelect("video");
//
          //  foreach (var node in list)
          //      CreatePopups.SendPopup(node);
          //  throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        //        if (false)
        //            if (driver == null)
        //            {
        //                ChromeOptions options = new ChromeOptions();
        //                options.AddArgument("--headless");
        //                var service = ChromeDriverService.CreateDefaultService(Application.dataPath + bin);
        //
        //#if UNITY_EDITOR
        //                service.HideCommandPromptWindow = true;
        //#endif
        //
        //                driver = new ChromeDriver(service, options);
        //
        //                // ((ChromeDriver)driver).hide();
        //            }
    }
#if UNITY_EDITOR
    const string bin = "";
#else
    const string bin = "/..";
#endif

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