using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HtmlAgilityPack;
//using MyBox;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using static MyNetworking;

[RequireComponent(typeof(VideoPlayer))]
public class VideoStuff : MonoBehaviour
{
    #region Variables

    #region Public

    //   [Foldout("Setable Objects", true)]
    //   [InitializationField]
    public VideoPlayer player;

    //    [InitializationField]
    public GameObject video;

    //    [Foldout("Networking Settings", true)]

    // public bool isClient;
    public static bool isClient;
    public bool isIPv6;

    //    [ConditionalField("isClient")]
    public string ipAddress;
    public ushort port;

    //    [Foldout("Video Settings", true)]

    public VideoSource source;

    //    [ConditionalField("source", false, VideoSource.Url)]
    public string videoURL;

    //    [ConditionalField("source", false, VideoSource.VideoClip)]
    public string path;
    //    [ConditionalField("source", false, VideoSource.VideoClip)]
    public string file;
    public static string staticVideoURL;
    [Tooltip("Set seek speed in seconds")]
    public float seekSpeed = 5;
    [Tooltip("Set intro skip in seconds")] public float introSkip = 85; //1:25
    #endregion

    #region Private/Internal

    RenderTexture tmpTex;
    Controls controls;
    bool isPrepared = false;
    static short index = 0;
    static PlayerState state;
    static bool stateReceived = false, closeNetwork = false, resume = false;
    static IPEndpointData ip;
    static SocketData soc;
    static List<Client> connections = new List<Client>();

    //  UnityWebRequest web;
    #endregion

    #endregion

    #region Jobs 

    #region Job vars
    static AcceptNetworkJob jobAccept;
    static ReceiveNetworkJob jobReceive;
    static JobHandle hndAccept, hndReceive;

    public enum MessageType : int
    {
        Unknown,
        ClientIndex,
        PlayerState,
        ClientPrepared,

    }
    #endregion

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
    }

    [StructLayout(LayoutKind.Sequential)]
    public class Unknown : Packet
    {
        public Unknown()
        {
            type = MessageType.Unknown; //int
            size = Marshal.SizeOf<Unknown>(); //int
        }
        public Quaternion l1, l2, l3, l4, l5, l6, l7, l8, l9, l10, l11, l12, l13, l14, l15, l16; //data buffering (the hard way) 
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ClientIndex : Packet
    {
        public ClientIndex(short index)
        {
            type = MessageType.ClientIndex;
            size = Marshal.SizeOf<ClientIndex>();
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
        public bool isPlaying = false;
        public bool isPaused = true;
        public bool seek = false;

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
        public bool playerReady = false;
    }

    #endregion

    #region Structs
    public struct AcceptNetworkJob : IJob
    {
        public SocketData socket;
        public void Execute()
        {
            CreatePopups.SendPopup("Started server");
            string err; //for viewing errors in debug
            while (true)
            {

                SocketData connect = new SocketData();
                IPEndpointData connectIP = new IPEndpointData();
                if (acceptSocket.Invoke(socket, connect, connectIP) == PResult.P_UnknownError) //check
                {
                    if (!closeNetwork)
                    {
                        PrintError(err = getLastNetworkError());
                        CreatePopups.SendPopup(err);
                        continue;
                    }
                    else
                        break;
                }

                connections.Add(new Client());
                connections[connections.Count - 1].soc = connect;
                connections[connections.Count - 1].prepared = new ClientPrepared();

                Unknown theurl = new Unknown();
                IntPtr tmp = Marshal.AllocHGlobal(theurl.size);
                Marshal.StructureToPtr(theurl, tmp, true);
                var pTmpStr = Marshal.StringToHGlobalAnsi(staticVideoURL.Substring(0, staticVideoURL.Length < 255 ? staticVideoURL.Length : 255));
                CopyMemory((tmp + Marshal.SizeOf<Packet>()), (pTmpStr), (uint)staticVideoURL.Substring(0, staticVideoURL.Length < 255 ? staticVideoURL.Length : 255).Length);
                Marshal.PtrToStructure(tmp, theurl);
                Marshal.FreeHGlobal(tmp);
                Marshal.FreeHGlobal(pTmpStr);
                if (sendAllPacket(connect, theurl) == PResult.P_UnknownError)
                    PrintError(err = getLastNetworkError());

                if (sendAllPacket(connect, state) == PResult.P_UnknownError)
                    PrintError(err = getLastNetworkError());

                print(err = "new connection!");
                CreatePopups.SendPopup(err);

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
            IntPtr unknown = IntPtr.Zero;
            int size;
            while (true)
            {
                if (isClient)
                {
                    //prevent unknown data collection
                    if (pollEvents.Invoke(soc, 10, (int)EventsPoll.EP_IN) == PResult.P_UnknownError)
                    {
                        if (closeNetwork)
                            break;
                        continue;
                    }
                    if (soc.pollCount == 0)continue;

                    recvAllPacket(soc, out size);
                    unknown = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(size, unknown, true);
                    if (recvAllPacket(soc, ref unknown, 4, size - 4) == PResult.P_Success)
                    {

                        print("Received Packet!");
                        switch (Marshal.PtrToStructure<Unknown>(unknown).type)
                        {
                            case MessageType.ClientIndex:
                                if (size != Marshal.PtrToStructure<Unknown>(unknown).size)continue;

                                ClientIndex index = Marshal.PtrToStructure<ClientIndex>(unknown);
                                Marshal.FreeHGlobal(tmp);

                                break;
                            case MessageType.PlayerState:
                                if (size != Marshal.PtrToStructure<Unknown>(unknown).size)continue;

                                PlayerState state = Marshal.PtrToStructure<PlayerState>(unknown);
                                Marshal.FreeHGlobal(tmp);

                                VideoStuff.state = state;
                                stateReceived = true;
                                break;
                            case MessageType.ClientPrepared:
                                if (size != Marshal.PtrToStructure<Unknown>(unknown).size)continue;

                                ClientPrepared prep = Marshal.PtrToStructure<ClientPrepared>(unknown);
                                Marshal.FreeHGlobal(tmp);

                                resume = true;
                                break;
                            default:
                                try
                                {
                                    string url = Marshal.PtrToStringAnsi(unknown + Marshal.SizeOf<Packet>());
                                    Marshal.FreeHGlobal(tmp);

                                    if (url.Contains("https://") || url.Contains("http://"))
                                        staticVideoURL = url;
                                }
                                catch {}
                                break;
                        }
                    }
                    else
                    if (!closeNetwork)
                        PrintError(err = getLastNetworkError());

                    Marshal.FreeHGlobal(unknown);
                }
                else //server
                {

                    for (int index = 0; index < connections.Count; index++)
                    {
                        //helps to minimize crashes
                        networkWaitForSeconds(0.2f);

                        if (pollEvents.Invoke(connections[index].soc, 10, (int)EventsPoll.EP_IN) == PResult.P_UnknownError)
                        {
                            PrintError(err = getLastNetworkError());
                            if (closeNetwork)
                                break;

                            continue;
                        }
                        else if (pollEvents.Invoke(connections[index].soc, 10, (int)EventsPoll.EP_IN) == PResult.P_Disconnection)
                        {
                            try
                            {
                                connections.RemoveAt(index--); //removes any connections that do not exist
                                print(err = "Connection removed!!");
                                CreatePopups.SendPopup(err);

                            }
                            catch { /*just incase*/ }
                            continue;
                        }

                        if (connections[index].soc.pollCount == 0)continue;

                        recvAllPacket(connections[index].soc, out size);
                        unknown = Marshal.AllocHGlobal(size);
                        Marshal.StructureToPtr(size, unknown, true);

                        if (recvAllPacket(connections[index].soc, ref unknown, 4, size - 4) == PResult.P_Success)
                        {
                            print("Received Packet!");
                            switch (Marshal.PtrToStructure<Unknown>(unknown).type)
                            {
                                case MessageType.PlayerState:

                                    PlayerState state = Marshal.PtrToStructure<PlayerState>(unknown);
                                    Marshal.FreeHGlobal(tmp);

                                    while (stateReceived); //this is correct 
                                    VideoStuff.state = state;

                                    for (int a = 0; a < connections.Count; ++a)
                                        connections[a].prepared.playerReady = false;

                                    stateReceived = true;
                                    break;

                                case MessageType.ClientPrepared:

                                    ClientPrepared prep = Marshal.PtrToStructure<ClientPrepared>(unknown);
                                    Marshal.FreeHGlobal(tmp);

                                    connections[index].prepared = prep;
                                    break;
                                default:

                                    string url = Marshal.PtrToStringAnsi(unknown + Marshal.SizeOf<Packet>());
                                    Marshal.FreeHGlobal(tmp);

                                    staticVideoURL = url;
                                    break;
                            }
                            if (closeNetwork)
                                break;
                        }
                        else if (!closeNetwork)
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

    void setVideoVariables()
    {
        isClient = InitSettings.isClient;
        isIPv6 = InitSettings.isIPv6;

        ipAddress = InitSettings.ipAddress;
        port = InitSettings.port;

        source = InitSettings.source;

        staticVideoURL = InitSettings.videoURL;
        path = InitSettings.path;
        file = InitSettings.file;
    }

    void Awake()
    {

        setVideoVariables();

        state = new PlayerState();

        //Setup controls
        controls = new Controls();
        controls.VideoPlayer.Play.performed += ctx => playNPause();
        controls.VideoPlayer.SeekLeft.performed += ctx => seekL();
        controls.VideoPlayer.SeekRight.performed += ctx => seekR();
        controls.VideoPlayer.VolumeUp.performed += ctx => volUp();
        controls.VideoPlayer.VolumeDown.performed += ctx => volDown();

        player = GetComponent<VideoPlayer>();
        player.skipOnDrop = true;
        player.playOnAwake = false;
        player.isLooping = false;
        player.source = VideoSource.Url; //I only need this one

        if (source == VideoSource.VideoClip)
        {
            player.url = path + file;
            player.Prepare();
        }
        else
        {
            if (staticVideoURL != "")
                player.url = staticVideoURL;
        }

        player.errorReceived += VideoError;
        player.prepareCompleted += ctx => VideoReady();
        player.seekCompleted += ctx => VideoSeekComplete();

        if (!isClient) //server
            if (staticVideoURL != "")
                player.Prepare();

        if (isNetworkInit)
        {
            CreatePopups.SendPopup("Network already initialized");
            return;
        }

        initNetworkPlugin();
        initNetwork();

        ip = createIPEndpointData.Invoke(isClient ? ipAddress : "", (short)port, isIPv6 ? IPVersion.IPv6 : IPVersion.IPv4);
        soc = createSocketData.Invoke(isIPv6 ? IPVersion.IPv6 : IPVersion.IPv4);
        string err;

        if (initSocket.Invoke(soc) == PResult.P_UnknownError)
        {
            PrintError(err = getLastNetworkError());
            if (!shutdownNetwork())
                PrintError(err = getLastNetworkError());

            return;
        }
        if (!isClient) //server
        {
            if (listenEndpointToSocket.Invoke(ip, soc) == PResult.P_Success)
            {
                jobAccept = new AcceptNetworkJob()
                {
                socket = soc
                };
                hndAccept = jobAccept.Schedule();

                jobReceive = new ReceiveNetworkJob() {};
                hndReceive = jobReceive.Schedule();
            }
            else
                PrintError(err = getLastNetworkError());

        }
        else
        {
            if (connectEndpointToSocket.Invoke(ip, soc) == PResult.P_Success)
            {
                jobReceive = new ReceiveNetworkJob();
                hndReceive = jobReceive.Schedule();
                print(err = "connected to host");
                CreatePopups.SendPopup(err);

            }
            else
                PrintError(err = getLastNetworkError());

        }

    }

    void OnEnable() =>
        controls.VideoPlayer.Enable();
    void OnDisable() =>
        controls.VideoPlayer.Disable();

    void Update()
    {

        string err;

        //receiving video url
        if (source == VideoSource.Url)
        {
            if (!isClient) //server
            {
                if (staticVideoURL != videoURL)
                    foreach (var connect in connections)
                    {
                        videoURL = staticVideoURL;
                        updateState();

                        int size = Marshal.SizeOf<PlayerState>();
                        //  sendAllPacket(connect.soc, size);
                        if (sendAllPacket(connect.soc, state) == PResult.P_UnknownError)
                            PrintError(err = getLastNetworkError());

                        Unknown theurl = new Unknown();
                        IntPtr tmp = Marshal.AllocHGlobal(theurl.size);
                        Marshal.StructureToPtr(theurl, tmp, true);
                        var pTmpStr = Marshal.StringToHGlobalAnsi(staticVideoURL.Substring(0, staticVideoURL.Length < 255 ? staticVideoURL.Length : 255));
                        CopyMemory((tmp + Marshal.SizeOf<Packet>()), (pTmpStr), (uint)staticVideoURL.Substring(0, staticVideoURL.Length < 255 ? staticVideoURL.Length : 255).Length);
                        Marshal.PtrToStructure(tmp, theurl);
                        Marshal.FreeHGlobal(tmp);
                        Marshal.FreeHGlobal(pTmpStr);
                        if (sendAllPacket(connect.soc, theurl) == PResult.P_UnknownError)
                            PrintError(err = getLastNetworkError());

                        player.url = staticVideoURL;
                        player.Prepare();
                    }
            }
            else //client 
                if (staticVideoURL != videoURL)
                {
                    print(err = "received new URL");
                    CreatePopups.SendPopup(err);

                    videoURL = staticVideoURL;
                    player.url = staticVideoURL;
                    player.Stop();
                    player.Prepare();
                }
        }
        else if (player.url != path + file)
        {
            player.url = path + file;
            player.Prepare();
        }

        //remote controles
        if (player.isPrepared)
            if (stateReceived)
            {
                double delayTime = DateTime.Now.Subtract(new DateTime(state.timeStamp)).TotalSeconds;

                player.time = state.pos + (false ? delayTime : 0);

                stateReceived = false; //connections can be updated again
            }

        if (resume)
        {
            for (int index = 0; index < connections.Count; index++)
                if (!connections[index].prepared.playerReady)
                    return;

            if (!state.isPaused)
                player.Play();
            else
                player.Pause();

            resume = false;
        }

        if (!tmpTex)
            if ((int)player.width != 0 && (int)player.height != 0)
            {
                //Create render texture for video the same size as the retrived video 
                tmpTex = new RenderTexture((int)player.width, (int)player.height, 1);
                tmpTex.autoGenerateMips = false;
                tmpTex.antiAliasing = 1;
                tmpTex.depth = 0;

                //Assign texture to UI texture and render target
                player.GetComponent<VideoPlayer>().targetTexture = tmpTex;
                video.GetComponent<RawImage>().texture = tmpTex;
            }

    }

    void VideoError(VideoPlayer source, string message)
    {
        string err;
        Debug.LogError(err = "Video Error Occurred: " + message);

        CreatePopups.SendPopup(err);
        print("attempting retry");

        source.Prepare();
    }
    void VideoReady()
    {
        print("Video is prepared!!");
        CreatePopups.SendPopup("Video is prepared!!");

        ClientPrepared tmp = new ClientPrepared();
        tmp.playerReady = true;

        if (isClient)
        {
            sendAllPacket(soc, tmp);
        }
        else //Server
        {
            resume = true;

            for (int index = 0; index < connections.Count; index++)
                sendAllPacket(connections[index].soc, tmp);
        }

        isPrepared = true;
    }
    void VideoSeekComplete()
    {
        print("Video seek compleated!!");
        CreatePopups.SendPopup("Video seek compleated!!");

        if (!isClient) //server
        {
            player.Pause(); //trying to start at the same time

            state.timeStamp = DateTime.Now.Ticks;

            for (int index = 0; index < connections.Count; index++)
            {
                connections[index].prepared.playerReady = false;
                sendAllPacket(connections[index].soc, state, Marshal.SizeOf<PlayerState>());
            }
        }

        ClientPrepared tmp = new ClientPrepared();
        tmp.playerReady = true;

        if (isClient)
        {
            sendAllPacket(soc, tmp);
        }
        else //Server
        {
            resume = true;

            for (int index = 0; index < connections.Count; index++)
                sendAllPacket(connections[index].soc, tmp);
        }

        isPrepared = true;
    }
    void updateState()
    {
        state.isPaused = player.isPaused;
        state.isPlaying = player.isPlaying;
        state.timeStamp = DateTime.Now.Ticks;
        state.pos = player.time;
        state.seek = true;
    }

    public void playNPause()
    {
        if (!isClient)
            stateReceived = true;

        updateState();
        state.isPaused = !state.isPaused;
        int size = Marshal.SizeOf<PlayerState>();
        if (isClient)
            sendAllPacket(soc, state);

    }
    public void skipIntro()
    {
        if (!isClient)
            stateReceived = true;

        updateState();
        state.pos = Mathf.Clamp((float)player.time + introSkip, 0, (float)player.length);
        state.seek = true;
        int size = Marshal.SizeOf<PlayerState>();
        if (isClient)
            sendAllPacket(soc, state);

    }
    public void unskipIntro()
    {
        if (!isClient)
            stateReceived = true;

        updateState();
        state.pos = Mathf.Clamp((float)player.time - introSkip, 0, (float)player.length);
        state.seek = true;
        int size = Marshal.SizeOf<PlayerState>();
        if (isClient)
            sendAllPacket(soc, state);

    }
    public void seekL()
    {
        if (!isClient)
            stateReceived = true;

        updateState();
        state.seek = true;
        state.pos = Mathf.Clamp((float)player.time - seekSpeed, 0, (float)player.length);
        int size = Marshal.SizeOf<PlayerState>();
        if (isClient)
            sendAllPacket(soc, state);

    }
    public void seekR()
    {

        if (!isClient)
            stateReceived = true;

        updateState();
        state.seek = true;
        state.pos = Mathf.Clamp((float)player.time + seekSpeed, 0, (float)player.length);
        int size = Marshal.SizeOf<PlayerState>();
        if (isClient)

            sendAllPacket(soc, state);

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

    public void ShutdownJobs() => shutdownJobs();
    public static void shutdownJobs()
    {

        CreatePopups.SendPopup("Jobs Shutdown");
        closeNetwork = true;
        if (isNetworkInit)
        {
            string err;

            if (setBlocking.Invoke(soc, false) == PResult.P_UnknownError)
                PrintError(err = getLastNetworkError());

            closeSocket.Invoke(soc);

            if (!shutdownNetwork())
                PrintError(err = getLastNetworkError());

            hndReceive.Complete();
            hndAccept.Complete();
        }
        closeNetwork = false;
    }

    // Callback sent to all game objects before the application is quit.
    void OnApplicationQuit()
    {
        shutdownJobs();
        closeNetworkPlugin();
    }

    // This function is called when the MonoBehaviour will be destroyed.
    void OnDestroy()
    {
        staticVideoURL = "";
    }
}