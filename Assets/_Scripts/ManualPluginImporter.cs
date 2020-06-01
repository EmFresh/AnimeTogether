using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class ManualPluginImporter : MonoBehaviour
{
    // see https://jacksondunstan.com/articles/3945 for source

    // Import the functions that'll let us open the libraries manually, from kernel32
    [DllImport("kernel32")]
    public static extern IntPtr LoadLibrary(string path);

    [DllImport("kernel32")]
    public static extern IntPtr GetProcAddress(IntPtr libraryHandle, string symbolName);

    [DllImport("kernel32")]
    public static extern IntPtr FreeLibrary(IntPtr libraryHandle);

    // Wrapper of the LoadLibrary function above
    public static IntPtr OpenLibrary(string path)
    {
        IntPtr handle = LoadLibrary(path);
        if (handle == IntPtr.Zero)
        {
            throw new Exception("Couldn't open native library: " + path);
        }
        return handle;
    }

    // Wraper of the FreeLibrary function above
    public static void CloseLibrary(IntPtr libraryHandle)
    {
        FreeLibrary(libraryHandle);
    }

    // Wrapper of the GetProcAddress above
    public static T GetDelegate<T>(IntPtr libraryHandle, string functionName)
        where T : class
    {
        IntPtr symbol = GetProcAddress(libraryHandle, functionName);
        if (symbol == IntPtr.Zero)
        {
            throw new Exception("Couldn't Get Function : " + functionName);
        }
        return Marshal.GetDelegateForFunctionPointer(symbol, typeof(T)) as T;
    }
}