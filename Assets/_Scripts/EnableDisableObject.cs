using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisableObject : MonoBehaviour
{
    public int enableVal;

    public void ToggleObject(int val)
    {
        if (val == enableVal)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }
    public void ToggleObjectNot(bool val)
    {
        if (!val)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }
    public void ToggleObject(bool val) =>
        gameObject.SetActive(val);

}