using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerProgression : MonoBehaviour
{
    public GameObject videoPlayer;
    bool draging = false;

    // Update is called once per frame
    void Update()
    {
        if (!draging && !videoPlayer.GetComponent<VideoStuff>().seekInProgress)
        {
            GetComponent<Slider>().maxValue = (float)videoPlayer.GetComponent<VideoStuff>().player.length;
            GetComponent<Slider>().value = Mathf.Clamp((float)videoPlayer.GetComponent<VideoStuff>().player.time, 0.01f, GetComponent<Slider>().maxValue);
        }

        var times = gameObject.GetComponentsInChildren<TMP_Text>();
        foreach (var time in times)
        {
            switch (time.name.ToLower())
            {
                case "current time":
                    if (TimeSpan.FromSeconds(GetComponent<Slider>().value).Hours > 0)
                        time.text = TimeSpan.FromSeconds(GetComponent<Slider>().value).ToString(@"hh\:mm\:ss");
                    else
                        time.text = TimeSpan.FromSeconds(GetComponent<Slider>().value).ToString(@"mm\:ss");
                    break;

                case "total time":
                    if (TimeSpan.FromSeconds(GetComponent<Slider>().maxValue).Hours > 0)
                        time.text = TimeSpan.FromSeconds(GetComponent<Slider>().maxValue).ToString(@"hh\:mm\:ss");
                    else
                        time.text = TimeSpan.FromSeconds(GetComponent<Slider>().maxValue).ToString(@"mm\:ss");
                    break;
            }
        }
    }

    public void chooseSeekPosition() { draging = true; }
    public void setSeekPosition()
    {
        draging = false;
        videoPlayer.GetComponent<VideoStuff>().seek(GetComponent<Slider>().value);
    }

}