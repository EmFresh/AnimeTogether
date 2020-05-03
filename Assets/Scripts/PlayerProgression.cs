using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProgression : MonoBehaviour
{
    public GameObject videoPlayer;

    // Update is called once per frame
    void Update()
    {

        GetComponent<Slider>().maxValue = (float)videoPlayer.GetComponent<VideoStuff>().player.length;
        GetComponent<Slider>().value = (float)videoPlayer.GetComponent<VideoStuff>().player.time;
        var t = System.TimeSpan.FromSeconds(GetComponent<Slider>().value);
       // print(t);
    }
}