using System.Collections;
using System.Collections.Generic;
//using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class InitSettings : MonoBehaviour
{

    public static bool isClient = false;
    public static bool isIPv6 = false;

    public static string ipAddress = "";
    public static ushort port = 5555;

    public static VideoSource source = VideoSource.Url;

    public static string videoURL = "";
    public static string path = "";
    public static string file = "";
    
    public void Awake()
    {
        IsClient(isClient);
        IsIPv6(isIPv6);
        // IPAddress(MyNetworking.GetPublicIPAddress());
        Port("5000");
        Source((int)source);
        VideoURL(videoURL);
        Path(path);
        File(file);
    }

    public void IsClient(bool enable) =>
        isClient = enable;
    public void IsIPv6(bool enable) => isIPv6 = enable;

    public void IPAddress(string str) => ipAddress = isClient ? str : "";
    public void Port(string num) => port = ushort.Parse(num);
    public void Source(int val) =>
        source = (VideoSource)val;

    public void VideoURL(string str) => videoURL = str == null? "": str;
    public void Path(string str) => path = str == null? "": str;
    public void File(string str) => file = str == null? "": str;

      void OnApplicationQuit()
    {
        VideoStuff.shutdownJobs();
        
    }

}