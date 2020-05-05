using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FadeInOut : MonoBehaviour
{
    public float delay, fadeoutTime,fadeinTime;
    float currentTime = 0, fadeTime = 1;
    bool isFadeOut = true;

    private Controls controls;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        controls = new Controls();
        controls.VideoPlayer.MouseInteract.performed += ctx => fadeInInvoke();
    }

    void OnEnable()
    {
        controls.VideoPlayer.Enable();
    }
    void OnDisable()
    {
        controls.VideoPlayer.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= delay && isFadeOut)
            InvokeRepeating("fadeOut", 0, .01f);
    }

    void fadeInInvoke()
    {
        if (!isFadeOut)
            InvokeRepeating("fadeIn", 0, .01f);
    }

    public void fadeIn()
    {
        isFadeOut = true;
        currentTime = 0;
        CancelInvoke("fadeOut");

        var cg = GetComponent<CanvasGroup>();
        cg.blocksRaycasts = true; //enables the buttons to be pressed
        cg.alpha = ((fadeTime += Time.deltaTime) / fadeoutTime);
        if (GetComponent<CanvasGroup>().alpha >= 1)
        {
            fadeTime = fadeoutTime;
            GetComponent<CanvasGroup>().alpha = 1;
            CancelInvoke("fadeIn");
        }
    }

    public void fadeOut()
    {
        isFadeOut = false;

        var cg = GetComponent<CanvasGroup>();

        cg.blocksRaycasts = false; //block the buttons from being pressed
        cg.alpha = ((fadeTime -= Time.deltaTime) / fadeoutTime);
        if (GetComponent<CanvasGroup>().alpha <= 0)
        {
            fadeTime = 0;
            GetComponent<CanvasGroup>().alpha = 0;
            CancelInvoke("fadeOut");
        }
    }
}