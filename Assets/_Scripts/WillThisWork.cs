using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Dropdown))]
public class WillThisWork : MonoBehaviour
{
    [Tooltip("Group of input fields to be switched")]
    public CanvasGroup group;
    public GameObject isClient;
    public TMP_InputField videoUrl;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TMP_Dropdown>().onValueChanged.AddListener(onValueChanged);
    }

    private void Update()
    {
        if (GetComponent<TMP_Dropdown>().IsExpanded)
        {
            group.blocksRaycasts = false;
            group.interactable = false;            
        }
        else
        {
            group.blocksRaycasts = true;
            group.interactable = true;
        }
    }
    void onValueChanged(int val)
    {
        videoUrl.gameObject.SetActive(!isClient.GetComponent<Toggle>().isOn);
    }
}