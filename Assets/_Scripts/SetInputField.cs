using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static MyNetworking;

//makes an extention for string
public static class clip
{
    public static void CopyToClipboard(this string s)
    {
        TextEditor te = new TextEditor();
        te.text = s;
        te.SelectAll();
        te.Copy();
        CreatePopups.SendPopup("Copied to clipboard");
    }
}

[RequireComponent(typeof(TMP_InputField))]
public class SetInputField : MonoBehaviour
{
    static string tmp = "";
    bool ipv6 = false;

    void Awake()
    {
        setToPublicIP(false);
    }

    public void copyPublicIP()
    {
        GetComponent<TMP_InputField>().text.CopyToClipboard();
    }

    public void setToIpv6(bool val)
    {
        ipv6 = val;
        if (!GetComponent<TMP_InputField>().interactable)
            refreshPublicIP();
    }

    public void refreshPublicIP()
    {
        CreatePopups.SendPopup("waiting on Refresh...");
        GetComponent<TMP_InputField>().text = ipv6 ? GetPublicIPv6Address() : GetPublicIPv4Address();
        if (GetComponent<TMP_InputField>().text != "")
            CreatePopups.SendPopup("Refreshed public IP");
        else
            CreatePopups.SendPopup("Refreshed failed");

    }

    public void setToPublicIP(bool set)
    {
        if (!set)
        {
            tmp = GetComponent<TMP_InputField>().text;
            tmp = tmp == null ? "" : tmp;
            refreshPublicIP();
        }
        else
        {
            GetComponent<TMP_InputField>().text = tmp;
        }
    }

}