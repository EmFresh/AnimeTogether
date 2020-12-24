using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ViewerConnections : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        GetComponent<TMP_Text>().SetText(" " + VideoStuff.connections.Count);
        transform.parent.gameObject.SetActive(!VideoStuff.isClient);
    }
}