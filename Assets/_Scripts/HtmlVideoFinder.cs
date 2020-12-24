using System;
using System.Collections;
using System.Collections.Generic;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using UnityEngine;

public class HtmlVideoFinder : MonoBehaviour
{
    public string url;
    // Start is called before the first frame update
    void Start()
    {
        ScrapingBrowser browser = new ScrapingBrowser();
        // browser.NavigateToPage();
        var browser2 = browser.NavigateToPage(new Uri(url));

        var list = browser2.Html.CssSelect("video");

        foreach (var node in list)
         CreatePopups.SendPopup(node);
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (false)
            if (driver == null)
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--headless");
                var service = ChromeDriverService.CreateDefaultService(Application.dataPath + bin);

#if UNITY_EDITOR
                service.HideCommandPromptWindow = true;
#endif

                driver = new ChromeDriver(service, options);

                // ((ChromeDriver)driver).hide();
            }
    }

    void Update()
    {

    }
}