using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Networking
{
    #region Enums
    public enum IPVersion : int
    {
        IPUnknown,
        IPv4,
        IPv6
    }
    public enum SocketOption : int
    {
        TCP_NoDelay,
        IPV6_Only,
    }
    public enum SocketType : int
    {
        TCP,
        UDP
    }
    public enum PResult : int
    {
        P_Success,
        P_UnknownError
    }
    enum EventsPoll
    {
        EP_RDBAND = 0x0200, //Priority band (out-of-band) data may be read without blocking.
        EP_RDNORM = 0x0100, //Normal data may be read without blocking.
        EP_WRNORM = 0x0010, //Normal data may be written without blocking.
        EP_IN = EP_RDBAND | EP_RDNORM, //combination of the EPRDNORM and EPRDBAND
        EP_OUT = EP_WRNORM, //same as the EPWRNORM
    }

    enum REventsPoll
    {
        REP_ERR = 0x0001, //An error has occurred.
        REP_NVAL = 0x0004, //An invalid socket was used.
        REP_HUP = 0x0002, //A stream-oriented connection was either disconnected or aborted.
        REP_RDBAND = 0x0200, //Priority band (out-of-band) data may be read without blocking.
        REP_RDNORM = 0x0100, //Normal data may be read without blocking.
        REP_WRNORM = 0x0010, //Normal data may be written without blocking.
        REP_IN = REP_RDBAND | REP_RDNORM, //combination of the REPRDNORM and REPRDBAND
        REP_OUT = REP_WRNORM, //same as the REPWRNORM
    }
    #endregion

    #region Structs
    [StructLayout(LayoutKind.Sequential)]
    public struct SocketData
    {
        public IPVersion m_IPVersion;
        public UInt64 m_hnd;
        uint pollCount;
        short revents;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct IPEndpointData
    {

        //public IPEndpointData(IPVersion ver = IPVersion.IPUnknown, string host = "", string str = "", long bytessize = 0, string bytes = "", short port = 0)
        //{
        //    m_IPVersion = ver; m_hostname = host; m_ipString = str;
        //    m_ipBytesSize = bytessize; m_ipBytes = bytes; m_port = port;
        //}

        public IPVersion m_IPVersion;
        [MarshalAs(UnmanagedType.LPStr)]
        [NativeDisableUnsafePtrRestriction] public IntPtr m_hostname;
        [MarshalAs(UnmanagedType.LPStr)]
        [NativeDisableUnsafePtrRestriction] public IntPtr m_ipString;
        public uint m_ipBytesSize;
        [NativeDisableUnsafePtrRestriction] public IntPtr m_ipBytes;
        public short m_port;
    };
    #endregion

    #region Variables
    public static ref readonly bool isNetworkInit => ref _isNetworkinit;

    private static bool _isNetworkinit = false;
    const string DLL = "/_Plugins/Networking.DLL";
    static IntPtr _pluginHandle = IntPtr.Zero;
    #endregion

    #region Plugin Handling
    public static void initNetworkPlugin()
    {
        if (_pluginHandle != IntPtr.Zero)return;

        if ((_pluginHandle = ManualPluginImporter.OpenLibrary(Application.dataPath + DLL)) == IntPtr.Zero)return;

        getlastnetworkerror = ManualPluginImporter.GetDelegate<getLastNetworkErrorDelegate>(_pluginHandle, "getLastNetworkError");
        initnetwork = ManualPluginImporter.GetDelegate<initNetworkDelegate>(_pluginHandle, "initNetwork");
        shutdownnetwork = ManualPluginImporter.GetDelegate<shutdownNetworkDelegate>(_pluginHandle, "shutdownNetwork");
        createIPEndpointData = ManualPluginImporter.GetDelegate<createIPEndpointDataDelegate>(_pluginHandle, "createIPEndpointData");
        initSocketData = ManualPluginImporter.GetDelegate<initSocketDataDelegate>(_pluginHandle, "initSocketData");
        createSocket = ManualPluginImporter.GetDelegate<createSocketDelegate>(_pluginHandle, "createSocket");
        closeSocket = ManualPluginImporter.GetDelegate<closeSocketDelegate>(_pluginHandle, "closeSocket");
        setBlocking = ManualPluginImporter.GetDelegate<setBlockingDelegate>(_pluginHandle, "setBlocking");
        bindEndpointToSocket = ManualPluginImporter.GetDelegate<bindEndpointToSocketDelegate>(_pluginHandle, "bindEndpointToSocket");
        listenEndpointToSocket = ManualPluginImporter.GetDelegate<listenEndpointToSocketDelegate>(_pluginHandle, "listenEndpointToSocket");
        acceptSocket = ManualPluginImporter.GetDelegate<acceptSocketDelegate>(_pluginHandle, "acceptSocket");
        connectEndpoint = ManualPluginImporter.GetDelegate<connectEndpointDelegate>(_pluginHandle, "connectEndpoint");
        sendPacketData = ManualPluginImporter.GetDelegate<sendPacketDataDelegate>(_pluginHandle, "sendPacketData");
        recvPacketData = ManualPluginImporter.GetDelegate<recvPacketDataDelegate>(_pluginHandle, "recvPacketData");
        sendAllPacketData = ManualPluginImporter.GetDelegate<sendAllPacketDataDelegate>(_pluginHandle, "sendAllPacketData");
        recvAllPacketData = ManualPluginImporter.GetDelegate<recvAllPacketDataDelegate>(_pluginHandle, "recvAllPacketData");
        recvFromPacketData = ManualPluginImporter.GetDelegate<recvFromPacketDataDelegate>(_pluginHandle, "recvFromPacketData");
        sendToPacketData = ManualPluginImporter.GetDelegate<sendToPacketDataDelegate>(_pluginHandle, "sendToPacketData");
    }

    public static void closeNetworkPlugin()
    {
        if (_pluginHandle != IntPtr.Zero)
            ManualPluginImporter.CloseLibrary(_pluginHandle);
    }
   
    #endregion


    //ERROR//

    ///<summary>
    ///gets the last error that happened
    ///</summary>
    private static getLastNetworkErrorDelegate getlastnetworkerror;
    private delegate IntPtr getLastNetworkErrorDelegate(int idk);
    public static string getLastNetworkError()
    {
        IntPtr ptr = getlastnetworkerror(0);
        return Marshal.PtrToStringAnsi(ptr);
    }

    public static void PrintError(object ob) => Debug.LogError(ob);
    
    
    //NETWORK//

    ///<summary>
    ///initializes Winsock
    ///</summary>
    private static initNetworkDelegate initnetwork;
    private delegate bool initNetworkDelegate(int a);
    ///<summary>
    ///initializes Winsock
    ///</summary>
    public static bool initNetwork()
    {
        if (!_isNetworkinit)
            if (initnetwork(0))
                return _isNetworkinit = true;

        return false;
    }

    ///<summary>
    ///Shutdown Winsoc
    ///</summary>
    private static shutdownNetworkDelegate shutdownnetwork;
    private delegate bool shutdownNetworkDelegate(int a);
    ///<summary>
    ///Shutdown Winsoc
    ///</summary>
    public static bool shutdownNetwork()
    {
        if (_isNetworkinit)
            if (shutdownnetwork(0))
                return !(_isNetworkinit = false); //true

        return false;
    }

    //ENDPOINT//

    ///<summary>
    ///Creates a new IPEndpoint handle for a given IP and port. Multiple IPEndpoints can be created
    ///</summary>
    public static createIPEndpointDataDelegate createIPEndpointData;
    public delegate IPEndpointData createIPEndpointDataDelegate([MarshalAs(UnmanagedType.LPStr)] string ip, short port, IPVersion ipv = IPVersion.IPv4);

    //SOCKET//

    ///<summary>
    ///Creates a new Socket handle for manageing IPEndpoints. 
    ///</summary>
    public static initSocketDataDelegate initSocketData;
    public delegate SocketData initSocketDataDelegate(IPVersion ipv = IPVersion.IPv4);
    ///<summary>
    ///initializes the socket to use UDP or TCP conection types
    ///</summary>
    public static createSocketDelegate createSocket;
    public delegate PResult createSocketDelegate(in SocketData soc, SocketType typ = SocketType.TCP, bool blocking = true);
    ///<summary>
    ///sets a socket to be either blocking or non-blocking. Must be called after socket is created
    ///</summary>
    public static setBlockingDelegate setBlocking;
    public delegate PResult setBlockingDelegate(in SocketData soc, bool blocking = true);

    ///<summary>
    ///closes socket so it can not be used unless createSocket() is called once again.
    ///</summary>
    public static closeSocketDelegate closeSocket;
    public delegate PResult closeSocketDelegate(in SocketData soc);

    ///<summary>
    ///Bind Endpoint to socket.
    ///</summary>
    public static bindEndpointToSocketDelegate bindEndpointToSocket;
    public delegate PResult bindEndpointToSocketDelegate(in IPEndpointData ip, in SocketData soc);
    ///<summary>
    ///Listens for endpoint connection to the socket. It will block until a new connection is found. 
    ///</summary>
    public static listenEndpointToSocketDelegate listenEndpointToSocket;
    public delegate PResult listenEndpointToSocketDelegate(in IPEndpointData ip, in SocketData soc, int backlog = 5);
    ///<summary>
    ///Attempts to accept the listened connection. 
    ///</summary>
    public static acceptSocketDelegate acceptSocket;
    public delegate PResult acceptSocketDelegate(in SocketData soc, in SocketData outsoc);
    ///<summary>
    ///Connects endpoint to socket
    ///</summary>
    public static connectEndpointDelegate connectEndpoint;
    public delegate PResult connectEndpointDelegate(in IPEndpointData ip, in SocketData soc);
    // Sending Data
    // TCP

    private delegate PResult sendPacketDataDelegate(in SocketData soc, IntPtr data, int datasize, out int bytesSent);
    private static sendPacketDataDelegate sendPacketData;
    ///<summary>
    ///Send packet over TCP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendPacket<T>(in SocketData soc, in T data, int datasize, out int bytesSent)
    {
        IntPtr tmp = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
        Marshal.StructureToPtr(data, tmp, true);
        bytesSent = 0;
        PResult res = sendPacketData(in soc, tmp, datasize, out bytesSent);
        Marshal.FreeHGlobal(tmp);

        return res;
    }
    ///<summary>
    ///Send packet over TCP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendPacket<T>(in SocketData soc, in T data, out int bytesSent) => sendPacket(soc, data, Marshal.SizeOf<T>(), out bytesSent);

    ///<summary>
    ///Send packet over TCP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendPacket(in SocketData soc, in string data, int datasize, out int bytesSent)
    {
        IntPtr tmp = Marshal.StringToHGlobalAnsi(data);
        bytesSent = 0;
        PResult res = sendPacketData(in soc, tmp, datasize, out bytesSent);

        return res;
    }
    ///<summary>
    ///Send packet over TCP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendPacket(in SocketData soc, in string data, out int bytesSent) => sendPacket(soc, data, data.Length + 1, out bytesSent);

    ///<summary>
    ///Send packet over TCP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendPacket(in SocketData soc, in string data, int datasize = -1)
    {
        datasize = datasize >= 0 ? datasize : data.Length + 1;
        IntPtr tmp = Marshal.StringToHGlobalAnsi(data);
        int bytesSent = 0;
        PResult res = sendPacketData(in soc, tmp, datasize, out bytesSent);

        return res;
    }

    private delegate PResult recvPacketDataDelegate(in SocketData soc, IntPtr dest, int numberOfBytes, out int bytesRecv);
    private static recvPacketDataDelegate recvPacketData;
    ///<summary>
    ///Receive packet over TCP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvPacket<T>(in SocketData soc, out T dest, int numberOfBytes, out int bytesRecv)
    {

        IntPtr tmp = Marshal.AllocHGlobal(numberOfBytes);
        //Marshal.StructureToPtr(dest, tmp, true);
        bytesRecv = 0;
        PResult res = recvPacketData(in soc, tmp, numberOfBytes, out bytesRecv);
        dest = default(T);
        if (res == PResult.P_Success)
            dest = Marshal.PtrToStructure<T>(tmp);
        Marshal.FreeHGlobal(tmp);

        return res;
    }
    ///<summary>
    ///Receive packet over TCP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvPacket<T>(in SocketData soc, out T dest, out int bytesRecv) => recvPacket(soc, out dest, Marshal.SizeOf<T>(), out bytesRecv);

    ///<summary>
    ///Receive packet over TCP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvPacket(in SocketData soc, out string dest, int numberOfBytes, out int bytesRecv)
    {

        IntPtr tmp = Marshal.AllocHGlobal(numberOfBytes);
        bytesRecv = 0;
        PResult res = recvPacketData(in soc, tmp, numberOfBytes, out bytesRecv);
        dest = "";
        if (res == PResult.P_Success)
            dest = Marshal.PtrToStringAnsi(tmp);
        Marshal.FreeHGlobal(tmp);

        return res;
    }
    ///<summary>
    ///Receive packet over TCP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvPacket(in SocketData soc, out string dest, int numberOfBytes)
    {

        IntPtr tmp = Marshal.AllocHGlobal(numberOfBytes);
        int bytesRecv = 0;
        PResult res = recvPacketData(in soc, tmp, numberOfBytes, out bytesRecv);
        dest = "";
        if (res == PResult.P_Success)
            dest = Marshal.PtrToStringAnsi(tmp);
        Marshal.FreeHGlobal(tmp);

        return res;
    }

    private delegate PResult sendAllPacketDataDelegate(in SocketData soc, IntPtr data, int numberOfBytes);
    private static sendAllPacketDataDelegate sendAllPacketData;
    ///<summary>
    ///Send entire packet over TCP server. guaranteed to send all bytes.
    ///</summary>
    public static PResult sendAllPacket<T>(in SocketData soc, in T data, int numberOfBytes = -1)
    {
        numberOfBytes = numberOfBytes >= 0 ? numberOfBytes : Marshal.SizeOf<T>();

        IntPtr tmp = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
        Marshal.StructureToPtr(data, tmp, true);
        PResult res = sendAllPacketData(in soc, tmp, numberOfBytes);

        Marshal.FreeHGlobal(tmp);

        return res;
    }
    ///<summary>
    ///Send entire packet over TCP server. guaranteed to send all bytes.
    ///</summary>
    public static PResult sendAllPacket(in SocketData soc, in string data, int numberOfBytes = -1)
    {
        numberOfBytes = numberOfBytes >= 0 ? numberOfBytes : data.Length + 1;

        IntPtr tmp = Marshal.StringToHGlobalAnsi(data);
        PResult res = sendAllPacketData(in soc, tmp, numberOfBytes);

        return res;
    }

    private delegate PResult recvAllPacketDataDelegate(in SocketData soc, IntPtr dest, int numberOfBytes);
    private static recvAllPacketDataDelegate recvAllPacketData;
    ///<summary>
    ///Receive entire packet over TCP server. guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvAllPacket<T>(in SocketData soc, out T dest, int numberOfBytes = -1)
    {

        numberOfBytes = numberOfBytes >= 0 ? numberOfBytes : Marshal.SizeOf<T>();

        IntPtr tmp = Marshal.AllocHGlobal(numberOfBytes);
        //Marshal.StructureToPtr(dest, tmp, true);
        PResult res = recvAllPacketData(in soc, tmp, numberOfBytes);
        dest = default(T);
        if (res == PResult.P_Success)
            dest = Marshal.PtrToStructure<T>(tmp);
        Marshal.FreeHGlobal(tmp);

        return res;
    }
    ///<summary>
    ///Receive entire packet over TCP server. guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvAllPacket(in SocketData soc, out string dest, int numberOfBytes)
    {
        IntPtr tmp = Marshal.AllocHGlobal(numberOfBytes);
        PResult res = recvAllPacketData(in soc, tmp, numberOfBytes);
        dest = "";
        if (res == PResult.P_Success)
            dest = Marshal.PtrToStringAnsi(tmp);
        Marshal.FreeHGlobal(tmp);

        return res;
    }

    //UDP

    private delegate PResult recvFromPacketDataDelegate(in SocketData soc, IntPtr data, int numberOfBytes, out int bytesRecv, out IPEndpointData ip);
    private static recvFromPacketDataDelegate recvFromPacketData;
    ///<summary>
    ///Receive packet over UDP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvFromPacket<T>(in SocketData soc, out T data, int numberOfBytes, out int bytesRecv, out IPEndpointData ip)
    {
        IntPtr tmp = Marshal.AllocHGlobal(numberOfBytes);
        bytesRecv = 0;
        ip = new IPEndpointData();
        PResult res = recvFromPacketData(in soc, tmp, numberOfBytes, out bytesRecv, out ip);
        data = default(T);
        if (res == PResult.P_Success)
            data = Marshal.PtrToStructure<T>(tmp);
        Marshal.FreeHGlobal(tmp);

        return res;
    }
    ///<summary>
    ///Receive packet over UDP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvFromPacket<T>(in SocketData soc, out T data, out int bytesRecv, out IPEndpointData ip) => recvFromPacket(soc, out data, Marshal.SizeOf<T>(), out bytesRecv, out ip);

    ///<summary>
    ///Receive packet over UDP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvFromPacket(in SocketData soc, out string data, int numberOfBytes, out int bytesRecv, out IPEndpointData ip)
    {
        IntPtr tmp = Marshal.AllocHGlobal(numberOfBytes); //allocates to unmanaged memory
        ip = new IPEndpointData();
        bytesRecv = 0;
        PResult res = recvFromPacketData(in soc, tmp, numberOfBytes, out bytesRecv, out ip);
        data = "";
        if (res == PResult.P_Success)
            data = Marshal.PtrToStringAnsi(tmp);
        Marshal.FreeHGlobal(tmp);
        return res;
    }

    ///<summary>
    ///Receive packet over UDP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvFromPacket<T>(in SocketData soc, out T data, int numberOfBytes, out IPEndpointData ip)
    {
        int bytesRecv;
        return recvFromPacket(soc, out data, numberOfBytes, out bytesRecv, out ip);
    }

    ///<summary>
    ///Receive packet over UDP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvFromPacket<T>(in SocketData soc, out T data, out IPEndpointData ip) =>
        recvFromPacket(soc, out data, Marshal.SizeOf<T>(), out ip);

    ///<summary>
    ///Receive packet over UDP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvFromPacket(in SocketData soc, out string data, int numberOfBytes, out IPEndpointData ip)
    {
        int bytesRecv = 0;
        return recvFromPacket(in soc, out data, numberOfBytes, out bytesRecv, out ip);
    }

    private delegate PResult sendToPacketDataDelegate(in SocketData soc, IntPtr data, int numberOfBytes, out int bytesSent, in IPEndpointData ip);
    private static sendToPacketDataDelegate sendToPacketData;
    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket<T>(in SocketData soc, in T data, int numberOfBytes, out int bytesSent, in IPEndpointData ip)
    {
        IntPtr tmp = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
        Marshal.StructureToPtr(data, tmp, true);
        bytesSent = 0;
        PResult res = sendToPacketData(in soc, tmp, numberOfBytes, out bytesSent, in ip);
        Marshal.FreeHGlobal(tmp);

        return res;
    }

    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket<T>(in SocketData soc, in T data, out int bytesSent, in IPEndpointData ip) => sendToPacket(soc, data, Marshal.SizeOf<T>(), out bytesSent, ip);

    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket(in SocketData soc, in string data, int numberOfBytes, out int bytesSent, in IPEndpointData ip)
    {
        IntPtr tmp = Marshal.StringToHGlobalAnsi(data);
        bytesSent = 0;
        PResult res = sendToPacketData(soc, tmp, numberOfBytes, out bytesSent, ip);

        return res;
    }
    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket(in SocketData soc, in string data, out int bytesSent, in IPEndpointData ip) => sendToPacket(soc, data, data.Length + 1, out bytesSent, ip);

    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket<T>(in SocketData soc, in T data, int numberOfBytes, in IPEndpointData ip)
    {
        int sentData = 0;
        return sendToPacket(soc, data, numberOfBytes, out sentData, ip);

    }
    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket<T>(in SocketData soc, in T data, in IPEndpointData ip) => sendToPacket(soc, data, Marshal.SizeOf<T>(), ip);

    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket(in SocketData soc, in string data, int numberOfBytes, in IPEndpointData ip)
    {
        int bytesSent;
        return sendToPacket(soc, data, numberOfBytes, out bytesSent, in ip);
    }

    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket(in SocketData soc, in string data, in IPEndpointData ip) => sendToPacket(soc, data, data.Length + 1, ip);

}