using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepCanvasSize : MonoBehaviour
{
public GameObject canvas;

    // Update is called once per frame
    void Update()
    {
        var rt1 = canvas.GetComponent<RectTransform>().sizeDelta;
        gameObject.GetComponent<RectTransform>().sizeDelta =rt1;
        
    }
}