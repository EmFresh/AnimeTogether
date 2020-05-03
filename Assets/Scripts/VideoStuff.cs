using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoStuff : MonoBehaviour
{
    public VideoPlayer player;
    //GameObject video;
    public GameObject video;
    RenderTexture tmpTex;
    private Controls controls;
    //  UnityWebRequest web;
    float seekSpeed = 5, introSkip = 85;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        controls = new Controls();
        controls.VideoPlayer.Play.performed += ctx => playNPause();
        controls.VideoPlayer.SeekLeft.performed += ctx => seekL();
        controls.VideoPlayer.SeekRight.performed += ctx => seekR();

        player.errorReceived += VideoError;
        player.prepareCompleted += VideoReady;
        player.Prepare();
    }

    void VideoError(VideoPlayer source, string message)
    {
        print("Video Error Occurred: " + message);

        print("attemptimg retry");
        source.Prepare();
    }

    void VideoReady(VideoPlayer source)
    {
        print("Video is prepared!!");
        playNPause();
    }

    // Start is called before the first frame update
    void Start()
    {
        // web =  UnityWebRequest.Get("https://9anime.ru/filter?page=");
        // web.SendWebRequest();
        // print(web.downloadedBytes);

        player.skipOnDrop = true;
        player.EnableAudioTrack(0, true);
        //var audio = GetComponent<AudioSource>();

        //video = new RenderTexture();
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

        if (!tmpTex)
            if ((int)player.width != 0 && (int)player.height != 0)
            {

                tmpTex = new RenderTexture((int)player.width, (int)player.height, 1);
                tmpTex.autoGenerateMips = false;
                tmpTex.antiAliasing = 1;
                tmpTex.depth = 0;

                video.GetComponent<RawImage>().texture = tmpTex;
                player.GetComponent<VideoPlayer>().targetTexture = tmpTex;
            }

    }

    public void playNPause()
    {
        if (player.isPaused || !player.isPlaying)
            player.Play();
        else
            player.Pause();
    }
    public void seekL() =>
        player.time = (player.time - seekSpeed) > 0 ? player.time - seekSpeed : 0;

    public void seekR() =>
        player.time = (player.time + seekSpeed) < player.length ? player.time + seekSpeed : player.length;
    public void skipIntro() =>
        player.time = (player.time + introSkip) < player.length ? player.time + introSkip : player.length;

}