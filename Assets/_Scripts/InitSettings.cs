using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class InitSettings : MonoBehaviour
{

    public void Awake()
    {
        IsClient(false);
        IsIPv6(false);
       // IPAddress(MyNetworking.GetPublicIPAddress());
        Port("5555");
        Source(0);
        VideoURL("");
        Path("");
        File("");
    }
    public static bool isClient = false;
    public static bool isIPv6 = false;

    public static string ipAddress;
    public static ushort port;

    public static VideoSource source = VideoSource.Url;

    public static string videoURL;
    public static string path;
    public static string file;

    public void IsClient(bool enable) => isClient = enable;
    public void IsIPv6(bool enable) => isIPv6 = enable;

    public void IPAddress(string str) => ipAddress = str;
    public void Port(string num) => port = ushort.Parse(num);
    public void Source(int val)
    {
        switch (val)
        {
            case 0: //Url
                source = VideoSource.Url;
                break;
            case 1: //video clip
                source = VideoSource.VideoClip;
                break;
        }
    }

    public void VideoURL(string str) => videoURL = str;
    public void Path(string str) => path = str;
    public void File(string str) => file = str;

}