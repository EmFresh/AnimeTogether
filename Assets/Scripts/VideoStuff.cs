﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using static Networking;

public class VideoStuff : MonoBehaviour
{
    public VideoPlayer player;
    //GameObject video;
    public GameObject video;
    public bool isClient;
    public string ipAddress;

    public string videoURL;
    public static string staticVideoURL;

    public static short index = 0;
    static PlayerState state = new PlayerState();
    RenderTexture tmpTex;
    private Controls controls;
    static bool closeNetwork = false;

    static List<Client> connections = new List<Client>();

    //  UnityWebRequest web;
    float seekSpeed = 5, introSkip = 85; //1:25

    #region Jobs 
    ListenOnNetworkJob jobListen;
    ReceiveNetworkJob jobReceive;
    JobHandle hndListen, hndReceive;

    public enum MessageType : int
    {
        Unknown,
        ClientIndex,
        PlayerState,
        ClientPrepared,

    }

    public class Client
    {
        public SocketData soc;
        public PlayerState state;
        public ClientPrepared prepared;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class Packet
    {
        public MessageType type = MessageType.Unknown;
        public int size = Marshal.SizeOf<Packet>();
        public short index = -1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class Unknown : Packet
    {
        public Unknown()
        {
            type = MessageType.Unknown; //int
            size = Marshal.SizeOf<Unknown>(); //int
        }
        public Quaternion l1, l2, l3, l4, l5, l6, l7, l8, l9, l10, l11, l12, l13, l14, l15; //data buffering (the hard way) 
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ClientIndex : Packet
    {
        public ClientIndex(short index)
        {
            type = MessageType.ClientIndex;
            size = Marshal.SizeOf<ClientIndex>();
            this.index = index;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public class PlayerState : Packet
    {
        public PlayerState()
        {
            type = MessageType.PlayerState;
            size = Marshal.SizeOf<PlayerState>();
        }
        public bool isPlaying;
        public bool isPaused;
        public bool seek;
        public double pos;

    }

    [StructLayout(LayoutKind.Sequential)]
    public class ClientPrepared : Packet
    {
        public ClientPrepared()
        {
            type = MessageType.ClientPrepared;
            size = Marshal.SizeOf<ClientPrepared>();
        }
        public bool playerReady;
    }

    public struct ListenOnNetworkJob : IJob
    {
        public IPEndpointData ip;
        public SocketData socket;
        public void Execute()
        {

            while (true)
            {
                if (listenEndpointToSocket.Invoke(ip, socket) == PResult.P_Success)
                {
                    SocketData connect = new SocketData();
                    if (acceptSocket.Invoke(socket, connect) == PResult.P_UnknownError)
                    {
                        PrintError(getLastNetworkError());
                        continue;
                    }
                    var tmp = new Client();
                    tmp.soc = connect;
                    connections.Add(tmp);
                    sendAllPacket(connect, new ClientIndex(index++));
                    sendAllPacket(connect, staticVideoURL);
                    sendAllPacket(connect, state);
                    print("new connection!");
                }
                else
                    PrintError(getLastNetworkError());

                if (closeNetwork)
                    break;
            }
        }
    }
    public struct ReceiveNetworkJob : IJob
    {
        public IPEndpointData ip;
        public SocketData socket;
        public void Execute()
        {

            IntPtr tmp = IntPtr.Zero;
            while (true)
            {
                Unknown unknown;
                unknown = new Unknown();
                recvAllPacket(socket, out unknown);

                switch (unknown.type)
                {
                    case MessageType.ClientIndex:
                        tmp = Marshal.AllocHGlobal(Marshal.SizeOf<Unknown>());
                        Marshal.StructureToPtr(unknown, tmp, true);
                        ClientIndex index = Marshal.PtrToStructure<ClientIndex>(tmp);
                        Marshal.FreeHGlobal(tmp);

                        VideoStuff.index = index.index;
                        break;
                    case MessageType.PlayerState:
                        tmp = Marshal.AllocHGlobal(Marshal.SizeOf<Unknown>());
                        Marshal.StructureToPtr(unknown, tmp, true);
                        PlayerState state = Marshal.PtrToStructure<PlayerState>(tmp);
                        Marshal.FreeHGlobal(tmp);

                        if (VideoStuff.state.isPaused != state.isPaused)
                            VideoStuff.state.isPaused = state.isPaused;

                        if (state.seek)
                            VideoStuff.state.pos = state.pos;

                        foreach (var client in connections)
                            sendAllPacket(client.soc, state);

                        break;
                    case MessageType.ClientPrepared:
                        tmp = Marshal.AllocHGlobal(Marshal.SizeOf<Unknown>());
                        Marshal.StructureToPtr(unknown, tmp, true);
                        ClientPrepared prep = Marshal.PtrToStructure<ClientPrepared>(tmp);
                        Marshal.FreeHGlobal(tmp);

                        connections[prep.index].prepared = prep;
                        break;
                    default:
                        //TODO: receive video url string from host
                        tmp = Marshal.AllocHGlobal(Marshal.SizeOf<Unknown>());
                        Marshal.StructureToPtr(unknown, tmp, true);
                        string url = Marshal.PtrToStringAnsi(tmp);
                        Marshal.FreeHGlobal(tmp);

                        staticVideoURL = url;

                        break;
                }

                if (closeNetwork)
                    break;
            }
        }
    }

    #endregion

    IPEndpointData ip;
    SocketData soc;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (!isClient)
            staticVideoURL = videoURL;
        //Setup controls
        controls = new Controls();
        controls.VideoPlayer.Play.performed += ctx => playNPause();
        controls.VideoPlayer.SeekLeft.performed += ctx => seekL();
        controls.VideoPlayer.SeekRight.performed += ctx => seekR();
        controls.VideoPlayer.VolumeUp.performed += ctx => volUp();
        controls.VideoPlayer.VolumeDown.performed += ctx => volDown();

        player.url = staticVideoURL;
        player.errorReceived += VideoError;
        player.prepareCompleted += VideoReady;
        if (staticVideoURL != "")
            player.Prepare();

        initNetworkPlugin();
        initNetwork();
        ip = createIPEndpointData.Invoke(ipAddress, 22, IPVersion.IPv6);
        soc = createSocketData.Invoke(IPVersion.IPv6);

        if (initSocket.Invoke(soc) == PResult.P_UnknownError)
        {
            print(getLastNetworkError());
            return;
        }
        if (!isClient) //server
        {

            jobListen = new ListenOnNetworkJob()
            {
                socket = soc,
                ip = ip
            };

            hndListen = jobListen.Schedule();

            //TODO: create a network listening job 
        }
        else
        {
            if (connectEndpointToSocket.Invoke(ip, soc) == PResult.P_Success)
            {

                print("connected to host");
            }
        }

        jobReceive = new ReceiveNetworkJob()
        {
            ip = ip,
            socket = soc
        };
        hndReceive = jobReceive.Schedule();
    }

    void VideoError(VideoPlayer source, string message)
    {
        print("Video Error Occurred: " + message);

        print("attempting retry");

        var tmp = source.url;
        source.Prepare();
    }

    void VideoReady(VideoPlayer source)
    {
        print("Video is prepared!!");
        //   playNPause();
    }

    // Start is called before the first frame update
    void Start()
    {
        // web =  UnityWebRequest.Get("https://9anime.ru/filter?page=");
        // web.SendWebRequest();
        // print(web.downloadedBytes);

        player.skipOnDrop = true;
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
        if (!isClient)
        {
            if (staticVideoURL != videoURL)
                foreach (var connect in connections)
                {
                    staticVideoURL = videoURL;
                    player.Stop();
                    updateState();
                    sendAllPacket(connect.soc, state);
                    sendAllPacket(connect.soc, staticVideoURL);
                    player.url = staticVideoURL;
                    player.Prepare();
                }
        }
        else if (staticVideoURL != videoURL)
        {
            videoURL = staticVideoURL;
            player.Stop();
            player.url = staticVideoURL;
            player.Prepare();
        }

        if (!tmpTex)
            if ((int)player.width != 0 && (int)player.height != 0)
            {
                //Create render texture for video the same size as the retrived video 
                tmpTex = new RenderTexture((int)player.width, (int)player.height, 1);
                tmpTex.autoGenerateMips = false;
                tmpTex.antiAliasing = 1;
                tmpTex.depth = 0;

                //Assign texture as UI texture and render target
                player.GetComponent<VideoPlayer>().targetTexture = tmpTex;
                video.GetComponent<RawImage>().texture = tmpTex;
            }

    }

    void updateState()
    {
        state.isPaused = player.isPaused;
        state.isPlaying = player.isPlaying;
        state.pos = player.time;
        state.seek = false;
    }

    public void playNPause()
    {
        if (player.isPaused || !player.isPlaying)
            player.Play();
        else
            player.Pause();

        if (player.isPlaying)
        {
            updateState();
            sendAllPacket(soc, state);
        }
    }
    public void skipIntro() =>
        player.time = Mathf.Clamp((float)player.time + introSkip, 0, (float)player.length);

    public void seekL() =>
        player.time = Mathf.Clamp((float)player.time - seekSpeed, 0, (float)player.length);

    public void seekR() =>
        player.time = Mathf.Clamp((float)player.time + seekSpeed, 0, (float)player.length);
    public void volUp()
    {
        for (ushort a = 0; a < player.audioTrackCount; ++a)
            player.SetDirectAudioVolume(a, Mathf.Clamp(player.GetDirectAudioVolume(a) + 0.1f, 0, 1)); //go up 10%
    }
    public void volDown()
    {
        for (ushort a = 0; a < player.audioTrackCount; ++a)
            player.SetDirectAudioVolume(a, Mathf.Clamp(player.GetDirectAudioVolume(a) - 0.1f, 0, 1)); //go down 10%
    }

    public void mute()
    {
        for (ushort a = 0; a < player.audioTrackCount; ++a)
            player.SetDirectAudioMute(a, !player.GetDirectAudioMute(a));
    }

    /// <summary>
    /// Callback sent to all game objects before the application is quit.
    /// </summary>
    void OnApplicationQuit()
    {
        closeNetwork = true;
        if (isNetworkInit)
        {
            if (!shutdownNetwork())
                PrintError(getLastNetworkError());
            //closeSocket(soc);

            closeSocket.Invoke(soc);
            hndReceive.Complete();
            hndListen.Complete();
        }
        closeNetworkPlugin();
    }
}