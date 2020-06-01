using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Dropdown))]
public class WillThisWork : MonoBehaviour
{
    public GameObject isClient;
    public GameObject videoUrl;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TMP_Dropdown>().onValueChanged.AddListener(onValueChanged);
    }

    void onValueChanged(int val)
    {
        videoUrl.SetActive(!isClient.GetComponent<Toggle>().isOn);

    }
}