using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
//using CefSharp;
//using CefSharp.OffScreen;
using UnityEngine;

public class Browser2D : MonoBehaviour
{
    public string startUrl;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        AppDomain.CurrentDomain.AssemblyResolve += Resolver;

//        var settings = new CefSettings();
//
//        // Set BrowserSubProcessPath based on app bitness at runtime
//        settings.BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
//            Environment.Is64BitProcess ? "x64" : "x86",
//            "CefSharp.BrowserSubprocess.exe");
//        var browser = new ChromiumWebBrowser(startUrl);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Will attempt to load missing assembly from either x86 or x64 subdir
    private static Assembly Resolver(object sender, ResolveEventArgs args)
    {
        if (args.Name.StartsWith("CefSharp"))
        {
       //     string assemblyName = args.Name.Split(new [] { ',' }, 2)[0] + ".dll";
       //     string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
       //         Environment.Is64BitProcess ? "x64" : "x86",
       //         assemblyName);
//
       //     return File.Exists(archSpecificPath) ?
       //         Assembly.LoadFile(archSpecificPath) :
       //         null;
        }

        return null;
    }
}