using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepUISize : MonoBehaviour
{
    public GameObject canvas;

    // Update is called once per frame
    void Update()
    {
        var rt1 = canvas.GetComponent<RectTransform>().sizeDelta ;
        var rt2 = gameObject.GetComponent<RectTransform>().sizeDelta ;
        gameObject.GetComponent<RectTransform>().localScale = rt1 / rt2;

    }
}