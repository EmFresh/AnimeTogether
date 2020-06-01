using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepUISize : MonoBehaviour
{
    public GameObject canvas;
    public Vector2 offset;
    // Update is called once per frame
    void Update()
    {
        var rt1 = canvas.GetComponent<RectTransform>().sizeDelta;
        var rt2 = gameObject.GetComponent<RectTransform>().sizeDelta;
        transform.localScale = rt1 / rt2;

        GetComponent<RectTransform>().anchoredPosition = offset;
    }
}