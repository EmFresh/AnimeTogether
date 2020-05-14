using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HtmlAgilityPack;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using static Networking;

public class VideoStuff : MonoBehaviour
{
    #region Variables

    #region Editor

    public VideoPlayer player;
    //GameObject video;
    public GameObject video;

    public bool isClient;
    private static bool _isClient;
    public bool isIPv6;

    public string ipAddress;
    public short port;

    public string videoURL;
    public static string staticVideoURL;
    public float seekSpeed = 5, introSkip = 85; //1:25
    #endregion

    #region Private

    RenderTexture tmpTex;
    Controls controls;
    bool isPrepared;
    static short index = 0;
    static PlayerState state;
    static bool stateReceived = false, closeNetwork = false;
    static IPEndpointData ip;
    static SocketData soc;
    static List<Client> connections = new List<Client>();

    //  UnityWebRequest web;
    #endregion

    #endregion

    #region Jobs 
    AcceptNetworkJob jobAccept;
    ReceiveNetworkJob jobReceive;
    JobHandle hndAccept, hndReceive;

    public enum MessageType : int
    {
        Unknown,
        ClientIndex,
        PlayerState,
        ClientPrepared,

    }

    #region Classes
    public class Client
    {
        public SocketData soc;
        public PlayerState state;
        public ClientPrepared prepared;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class Packet
    {
        public int size = Marshal.SizeOf<Packet>();
        public MessageType type = MessageType.Unknown;
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

        public long timeStamp;
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
        public bool playerReady=false;
    }
    #endregion

    #region Structs
    public struct AcceptNetworkJob : IJob
    {
        public SocketData socket;
        public void Execute()
        {
            string err; //for viewing errors in debug
            while (true)
            {

                SocketData connect = new SocketData();
                IPEndpointData connectIP = new IPEndpointData();
                if (acceptSocket.Invoke(socket, connect, connectIP) == PResult.P_UnknownError)
                {
                    PrintError(err = getLastNetworkError());

                    if (closeNetwork)
                        break;
                    continue;
                }

                connections.Add(new Client());
                connections[connections.Count - 1].soc = connect;
                connections[connections.Count - 1].prepared = new ClientPrepared();

                int size = Marshal.SizeOf<ClientIndex>();
                sendAllPacket(connect, size);
                if (sendAllPacket(connect, new ClientIndex(index++)) == PResult.P_UnknownError)
                    PrintError(err = getLastNetworkError());

                size = staticVideoURL.Length + 1;
                sendAllPacket(connect, size);
                if (sendAllPacket(connect, staticVideoURL) == PResult.P_UnknownError)
                    PrintError(err = getLastNetworkError());

                size = Marshal.SizeOf<PlayerState>();
                sendAllPacket(connect, size);
                if (sendAllPacket(connect, state) == PResult.P_UnknownError)
                    PrintError(err = getLastNetworkError());

                print("new connection!");

                if (closeNetwork)
                    break;
            }
        }
    }

    public struct ReceiveNetworkJob : IJob
    {
        public void Execute()
        {

            string err; //for viewing errors in debug
            IntPtr tmp = IntPtr.Zero;
            int size;
            while (true)
            {
                Unknown unknown;
                unknown = new Unknown();
                if (_isClient)
                {
                    recvAllPacket(soc, out size);
                    if (recvAllPacket(soc, out unknown, size) == PResult.P_Success)
                    {
                        print("Received Packet!");
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

                                stateReceived = true;

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
                    }
                    else
                        PrintError(err = getLastNetworkError());
                }
                else
                {

                    for (int index = 0; index < connections.Count; index++)
                    {
                        //helps to minimize crashes
                        networkWaitForSeconds(0.2f);

                        if (pollEvents.Invoke(connections[index].soc, 10, (int)EventsPoll.EP_IN) == PResult.P_UnknownError)
                        {
                            //    PrintError(err = getLastNetworkError());
                            connections.RemoveAt(index--); //removes any connection that dose not exist
                            continue;
                        }

                        if (connections[index].soc.pollCount == 0)continue;

                        recvAllPacket(connections[index].soc, out size);
                        if (recvAllPacket(connections[index].soc, out unknown, size) == PResult.P_Success)
                        {
                            print("Received Packet!");
                            switch (unknown.type)
                            {
                                case MessageType.PlayerState:
                                    if (size != unknown.size)continue;

                                    tmp = Marshal.AllocHGlobal(Marshal.SizeOf<Unknown>());
                                    Marshal.StructureToPtr(unknown, tmp, true);
                                    PlayerState state = Marshal.PtrToStructure<PlayerState>(tmp);
                                    Marshal.FreeHGlobal(tmp);

                                    while (stateReceived); //this is correct 
                                    VideoStuff.state = state;

                                    if (state.seek)
                                        for (int a = 0; a < connections.Count; ++a)
                                            connections[a].prepared.playerReady = false;

                                    stateReceived = true;

                                    break;

                                case MessageType.ClientPrepared:
                                    if (size != unknown.size)continue;

                                    tmp = Marshal.AllocHGlobal(Marshal.SizeOf<Unknown>());
                                    Marshal.StructureToPtr(unknown, tmp, true);
                                    ClientPrepared prep = Marshal.PtrToStructure<ClientPrepared>(tmp);
                                    Marshal.FreeHGlobal(tmp);

                                    connections[prep.index].prepared = prep;
                                    break;

                                default:
                                    if (size != unknown.size)continue;

                                    //TODO: receive video url string from host
                                    tmp = Marshal.AllocHGlobal(Marshal.SizeOf<Unknown>());
                                    Marshal.StructureToPtr(unknown, tmp, true);
                                    string url = Marshal.PtrToStringAnsi(tmp);
                                    Marshal.FreeHGlobal(tmp);

                                    staticVideoURL = url;
                                    break;
                            }
                        }
                        else
                            PrintError(err = getLastNetworkError());

                        if (closeNetwork)
                            break;
                    }
                }
                if (closeNetwork)
                    break;
            }
        }
    }
    #endregion

    #endregion

    void Awake()
    {
        _isClient = isClient;
        state = new PlayerState();
        if (!_isClient)
            staticVideoURL = videoURL;
        //Setup controls
        controls = new Controls();
        controls.VideoPlayer.Play.performed += ctx => playNPause();
        controls.VideoPlayer.SeekLeft.performed += ctx => seekL();
        controls.VideoPlayer.SeekRight.performed += ctx => seekR();
        controls.VideoPlayer.VolumeUp.performed += ctx => volUp();
        controls.VideoPlayer.VolumeDown.performed += ctx => volDown();

        player.skipOnDrop = true;
        player.url = staticVideoURL;
        player.errorReceived += VideoError;
        player.prepareCompleted += ctx => VideoReady();
        player.seekCompleted += ctx => VideoSeekComplete();
        // player.clockResyncOccurred;
        // player.frameDropped;
        //player.frameReady;

        if (!_isClient)
            if (staticVideoURL != "")
                player.Prepare();

        initNetworkPlugin();
        initNetwork();

        ip = createIPEndpointData.Invoke(ipAddress, port, isIPv6 ? IPVersion.IPv6 : IPVersion.IPv4);
        soc = createSocketData.Invoke(isIPv6 ? IPVersion.IPv6 : IPVersion.IPv4);

        if (initSocket.Invoke(soc) == PResult.P_UnknownError)
        {
            print(getLastNetworkError());
            return;
        }
        if (!_isClient) //server
        {
            if (listenEndpointToSocket.Invoke(ip, soc) == PResult.P_Success)
            {
                jobAccept = new AcceptNetworkJob()
                {
                socket = soc
                };

                hndAccept = jobAccept.Schedule();

                //TODO: create a network listening job 
            }
            else
            {
                PrintError(getLastNetworkError());
                return;
            }

        }
        else
        {
            if (connectEndpointToSocket.Invoke(ip, soc) == PResult.P_Success)
            {

                print("connected to host");
            }
            else
            {
                PrintError(getLastNetworkError());
            }
        }

        jobReceive = new ReceiveNetworkJob();
        hndReceive = jobReceive.Schedule();
    }

    void OnEnable() =>
        controls.VideoPlayer.Enable();

    void OnDisable() =>
        controls.VideoPlayer.Disable();

    void Update()
    {
        _isClient = isClient;

        string err;

        //receiving video url
        if (!_isClient)
        {
            if (staticVideoURL != videoURL)
                foreach (var connect in connections)
                {
                    staticVideoURL = videoURL;
                    player.Stop();
                    updateState();

                    int size = Marshal.SizeOf<PlayerState>();
                    sendAllPacket(connect.soc, size);
                    if (sendAllPacket(connect.soc, state) == PResult.P_UnknownError)
                        PrintError(err = getLastNetworkError());

                    size = staticVideoURL.Length + 1;
                    sendAllPacket(connect.soc, size);
                    if (sendAllPacket(connect.soc, staticVideoURL) == PResult.P_UnknownError)
                        PrintError(err = getLastNetworkError());
                    player.url = staticVideoURL;
                    player.Prepare();
                }
        }
        else if (staticVideoURL != videoURL)
        {
            print("received new URL");

            videoURL = staticVideoURL;
            player.Stop();
            player.url = staticVideoURL;
            player.Prepare();
        }

        //remote controles
        if (player.isPrepared)
            if (stateReceived)
            {
                double delayTime = DateTime.Now.Subtract(new DateTime(state.timeStamp)).TotalSeconds;

                if (!_isClient)
                {
                    bool cont = true;
                    for (int index = 0; index < connections.Count; ++index)
                        if (!connections[index].prepared.playerReady)
                            cont = false;
                    if (!cont)return;

                    state.timeStamp = DateTime.Now.Ticks;
                    int size = Marshal.SizeOf<PlayerState>();
                    for (int index = 0; index < connections.Count; index++)
                    {
                        sendAllPacket(connections[index].soc, size);
                        sendAllPacket(connections[index].soc, state, size);
                    }

                }

                bool isDelayedPlay;
                if (isDelayedPlay = (state.isPaused != player.isPaused))
                    if (player.isPaused || !player.isPlaying)
                        player.Play();
                    else
                        player.Pause();

                isDelayedPlay = isDelayedPlay && !state.isPaused;
                if (state.seek || isDelayedPlay)
                    player.time = state.pos /*+ (isDelayedPlay ? delayTime : 0)*/ ;

                stateReceived = false;
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

    void VideoError(VideoPlayer source, string message)
    {
        Debug.LogError("Video Error Occurred: " + message);

        print("attempting retry");

        //  var tmp = source.url;
        source.Prepare();
    }

    void VideoReady()
    {
        print("Video is prepared!!");

        if (_isClient)
        {

            ClientPrepared tmp = new ClientPrepared();
            tmp.index = index;
            tmp.playerReady = true;
            sendAllPacket(soc, Marshal.SizeOf<ClientPrepared>());
            sendAllPacket(soc, tmp);
        }
        isPrepared = true;
        //   playNPause();
    }
    void VideoSeekComplete()
    {
        print("Video seek compleated!!");

        if (_isClient)
        {
            ClientPrepared tmp = new ClientPrepared();
            tmp.playerReady = true;
            sendAllPacket(soc, Marshal.SizeOf<ClientPrepared>());
            sendAllPacket(soc, tmp);
        }
        isPrepared = true;
    }
    void updateState()
    {
        state.isPaused = player.isPaused;
        state.isPlaying = player.isPlaying;
        state.timeStamp = DateTime.Now.Ticks;
        state.pos = player.time;
        state.seek = false;
    }

    public void playNPause()
    {

        updateState();
        state.isPaused = !state.isPaused;
        int size = Marshal.SizeOf<PlayerState>();
        if (_isClient)
        {
            sendAllPacket(soc, size);
            sendAllPacket(soc, state);
        }
        else
        {
            // bool cont = true;
            // for (int index = 0; index < connections.Count; ++index)
            //     if (!connections[index].prepared.playerReady)
            //         cont = false;
            // if (!cont)return;
            //
            // foreach (var client in connections)
            // {
            //     sendAllPacket(client.soc, size);
            //     sendAllPacket(client.soc, state);
            // }
            //
            // if (player.isPaused || !player.isPlaying)
            //     player.Play();
            // else
            //     player.Pause();

            stateReceived = true;
        }
    }
    public void skipIntro()
    {

        updateState();
        state.pos = Mathf.Clamp((float)player.time + introSkip, 0, (float)player.length);
        state.seek = true;
        int size = Marshal.SizeOf<PlayerState>();
        if (_isClient)
        {
            sendAllPacket(soc, size);
            sendAllPacket(soc, state);
        }
        else
        {
            //bool cont = true;
            //for (int index = 0; index < connections.Count; ++index)
            //    if (!connections[index].prepared.playerReady)
            //        cont = false;
            //if (!cont)return;
            //
            //foreach (var client in connections)
            //{
            //    sendAllPacket(client.soc, size);
            //    sendAllPacket(client.soc, state);
            //}
            //player.time = Mathf.Clamp((float)player.time + introSkip, 0, (float)player.length);
            stateReceived = true;
        }
    }
    public void seekL()
    {

        updateState();
        state.seek = true;
        state.pos = Mathf.Clamp((float)player.time - seekSpeed, 0, (float)player.length);
        int size = Marshal.SizeOf<PlayerState>();
        if (_isClient)
        {
            sendAllPacket(soc, size);
            sendAllPacket(soc, state);
        }
        else
        {
            //bool cont = true;
            //for (int index = 0; index < connections.Count; ++index)
            //    if (!connections[index].prepared.playerReady)
            //        cont = false;
            //if (!cont)return;
            //
            //foreach (var client in connections)
            //{
            //    sendAllPacket(client.soc, size);
            //    sendAllPacket(client.soc, state);
            //}
            //player.time = Mathf.Clamp((float)player.time - seekSpeed, 0, (float)player.length);
            stateReceived = true;
        }
    }
    public void seekR()
    {

        updateState();
        state.seek = true;
        state.pos = Mathf.Clamp((float)player.time + seekSpeed, 0, (float)player.length);
        int size = Marshal.SizeOf<PlayerState>();
        if (_isClient)
        {
            sendAllPacket(soc, size);
            sendAllPacket(soc, state);
        }
        else
        {
            //bool cont = true;
            //for (int index = 0; index < connections.Count; ++index)
            //    if (!connections[index].prepared.playerReady)
            //        cont = false;
            //if (!cont)return;
            //
            //foreach (var client in connections)
            //{
            //    sendAllPacket(client.soc, size);
            //    sendAllPacket(client.soc, state);
            //}
            //player.time = Mathf.Clamp((float)player.time + seekSpeed, 0, (float)player.length);
            stateReceived = true;
        }
    }
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

    // Callback sent to all game objects before the application is quit.
    void OnApplicationQuit()
    {
        closeNetwork = true;
        if (isNetworkInit)
        {
            string str;

            if (setBlocking.Invoke(soc, false) == PResult.P_UnknownError)
                PrintError(str = getLastNetworkError());

            closeSocket.Invoke(soc);

            if (!shutdownNetwork())
                PrintError(str = getLastNetworkError());

            hndReceive.Complete();
            hndAccept.Complete();
        }

        closeNetworkPlugin();
    }
}