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
    static string tmp;

    void Awake()
    {
        setToPublicIP(false);
    }

    public void copyPublicIP()
    {
        GetComponent<TMP_InputField>().text.CopyToClipboard();
    }

    public void refreshPublicIP()
    {
        CreatePopups.SendPopup("waiting on Refresh...");
        GetComponent<TMP_InputField>().text = GetPublicIPAddress();
        if (GetComponent<TMP_InputField>().text!="")
            CreatePopups.SendPopup("Refreshed public IP");
            else
            CreatePopups.SendPopup("Refreshed failed");
            
    }

    public void setToPublicIP(bool set)
    {
        if (!set)
        {
            tmp = GetComponent<TMP_InputField>().text;
            GetComponent<TMP_InputField>().text = GetPublicIPAddress();
        }
        else
        {
            GetComponent<TMP_InputField>().text = tmp;
        }
    }

    /// <summary>
    /// Callback sent to all game objects before the application is quit.
    /// </summary>
    void OnApplicationQuit()
    {
        GetComponent<TMP_InputField>().text = "";
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    void OnDestroy()
    {
            GetComponent<TMP_InputField>().text = "";
   
    }
}