using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Dropdown))]
public class WillThisWork : MonoBehaviour
{
    public GameObject isClient;
    public TMP_InputField videoUrl,path,file;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TMP_Dropdown>().onValueChanged.AddListener(onValueChanged);
    }

    private void Update()
    {
        if (GetComponent<TMP_Dropdown>().IsExpanded)
        {
            videoUrl.interactable = false;
            path.interactable = false;
            file.interactable = false;
        } else{
            videoUrl.interactable = true;
            path.interactable = true;
            file.interactable = true;
        }
    }
    void onValueChanged(int val)
    {
        videoUrl.gameObject.SetActive(!isClient.GetComponent<Toggle>().isOn);

    }
}