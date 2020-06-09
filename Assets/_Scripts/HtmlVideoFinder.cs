using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using HtmlAgilityPack;
//using HTML_Finder;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.
//using ScrapySharp.Extensions;
//using ScrapySharp.Network;
using UnityEngine;

public class HtmlVideoFinder : MonoBehaviour
{
    static ChromeDriver driver;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (driver == null)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");
            driver = new ChromeDriver(options);
        }
    }

    public void URLStringFinder(string url)
    {
        driver.Navigate().GoToUrl(@"https://9anime.ru/watch/welcome-to-demon-school-iruma-kun-dub.1k0x/jvv8333");
        ReadOnlyCollection<IWebElement> ele = driver.FindElements(By.TagName("iframe"));
        // driver.Capabilities.BrowserName;

        string str;
        // OpenQA.Selenium.
        foreach (var element in ele)
        {
            str = element.GetAttribute("src");
            print(str);
            try
            {
                var tmp = element.FindElement(By.TagName("source"));
                print("step 1");
                print(tmp.GetAttribute("src"));
                print("step 2");
            }
            catch
            {
                print("this did not work");
            }

            if (str.Contains("mp4upload"))
            {
                driver.Navigate().GoToUrl(str);

                try
                {
                    print(driver.FindElement(By.TagName("source")).GetAttribute("src"));
                }
                catch
                {
                    print("could not find video");
                    //<div class="plyr__video-wrapper"><video poster="https://www12.mp4upload.com/i/01471/fyrrthn00d03.jpg" playsinline=""><source src="https://www12.mp4upload.com:282/d/q6xqbsjvz3b4quuowowryk2sknisf4o6yz7ujpy7bo5yo5m2bnaj4spm/video.mp4" type="video/mp4"></video></div>
                }
                break;
            }
        }

    }

    /// <summary>
    /// Callback sent to all game objects before the application is quit.
    /// </summary>
    void OnApplicationQuit()
    {
        driver.Quit();
        //        driver.Close();
    }

}