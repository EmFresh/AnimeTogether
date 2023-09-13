﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FadeInOut : MonoBehaviour
{
    public bool enableFadeIn = true, enableAutoFadeOut = true;

    public float delay, fadeoutTime, fadeinTime;
    float currentTime = 0, fadeTime = 1;
    bool isFadeOut = true;

    private Controls controls;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        controls = new Controls();

        if (enableFadeIn)
            controls.VideoPlayer.MouseInteract.performed += ctx => fadeInInvoke();
    }
    void OnEnable() => controls.VideoPlayer.Enable();

    void OnDisable() => controls.VideoPlayer.Disable();

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= delay && isFadeOut && enableAutoFadeOut)
            InvokeRepeating("fadeOut", 0, .01f);
    }

    public void fadeInInvoke()
    {
        if (!IsInvoking("fadeIn"))
            InvokeRepeating("fadeIn", 0, .01f);
    }

    public void fadeOutInvoke()
    {
        if (!IsInvoking("fadeOut"))
            InvokeRepeating("fadeOut", 0, .01f);
    }

    public void fadeIn()
    {
        CancelInvoke("fadeOut");
        currentTime = 0;
        var cg = GetComponent<CanvasGroup>();
        cg.blocksRaycasts = true; //enables the buttons to be pressed

        cg.alpha = Mathf.Clamp((fadeTime += Time.deltaTime) / fadeinTime, 0, 1);
        if (cg.alpha >= 1)
        {
            cg.alpha = 1;
            cg.blocksRaycasts = true;
            isFadeOut = true;
            fadeTime = fadeoutTime;
            CancelInvoke("fadeIn");
        }
    }

    public void fadeOut()
    {
        isFadeOut = false;

        var cg = GetComponent<CanvasGroup>();

        cg.alpha = Mathf.Clamp((fadeTime -= Time.deltaTime) / fadeoutTime, 0, 1);
        if (cg.alpha <= 0)
        {
            cg.blocksRaycasts = false; //block the buttons from being pressed
            fadeTime = 0;
            cg.alpha = 0;
            cg.blocksRaycasts = false;
            CancelInvoke("fadeOut");
        }
    }
}