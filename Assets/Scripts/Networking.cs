using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Networking
{

    public enum IPVersion : int
    {
        IPUnknown,
        IPv4,
        IPv6
    }
    public enum SocketOption : int
    {
        TCP_NoDelay
    }
    public enum SocketType : int
    {
        TCP,
        UDP
    }
    public enum PResult : int
    {
        P_Success,
        P_GenericError
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SocketData
    {
        public IPVersion m_IPVersion;
        public UInt64 m_hnd;
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

    private static bool _isNetworkinit = false;
    public static ref readonly bool isNetworkInit => ref _isNetworkinit;
    const string DLL = "Networking.DLL";

    //ERROR//

    ///<summary>
    ///gets the last error that happened
    ///</summary>
    [DllImport(DLL)] private static extern IntPtr getLastNetworkError(int idk);
    public static string getLastNetworkError()
    {
        IntPtr ptr = getLastNetworkError(0);
        return Marshal.PtrToStringAnsi(ptr);
    }

    public static void PrintError(object ob) => Debug.LogError(ob);
    //NETWORK//

    ///<summary>
    ///initializes Winsock
    ///</summary>
    [DllImport(DLL)] private static extern bool initNetwork(int a);
    ///<summary>
    ///initializes Winsock
    ///</summary>
    public static bool initNetwork()
    {
        if (!_isNetworkinit)
            if (initNetwork(0))
                return _isNetworkinit = true;

        return false;
    }
    ///<summary>
    ///Shutdown Winsoc
    ///</summary>
    [DllImport(DLL)] private static extern bool shutdownNetwork(int a);
    ///<summary>
    ///Shutdown Winsoc
    ///</summary>
    public static bool shutdownNetwork()
    {
        if (_isNetworkinit)
            if (shutdownNetwork(0))
                return !(_isNetworkinit = false); //true

        return false;
    }

    //ENDPOINT//

    ///<summary>
    ///Creates a new IPEndpoint handle for a given IP and port. Multiple IPEndpoints can be created
    ///</summary>
    [DllImport(DLL)] public static extern IPEndpointData createIPEndpointData([MarshalAs(UnmanagedType.LPStr)] string ip, short port);

    //SOCKET//

    ///<summary>
    ///Creates a new Socket handle for manageing IPEndpoints. 
    ///</summary>
    [DllImport(DLL)] public static extern SocketData initSocketData();
    ///<summary>
    ///initializes the socket to use UDP or TCP conection types
    ///</summary>
    [DllImport(DLL)] public static extern PResult createSocket(in SocketData soc, SocketType typ = SocketType.TCP);

    ///<summary>
    ///closes socket so it can not be used unless createSocket() is called once again.
    ///</summary>
    [DllImport(DLL)] public static extern PResult closeSocket(in SocketData soc);

    ///<summary>
    ///Bind Endpoint to socket.
    ///</summary>
    [DllImport(DLL)] public static extern PResult bindEndpointToSocket(in IPEndpointData ip, in SocketData soc);
    ///<summary>
    ///Listens for endpoint connection to the socket. It will block until a new connection is found. 
    ///</summary>
    [DllImport(DLL)] public static extern PResult listenEndpointToSocket(in IPEndpointData ip, in SocketData soc, int backlog = 5);
    ///<summary>
    ///Attempts to accept the listened connection. 
    ///</summary>
    [DllImport(DLL)] public static extern PResult acceptSocket(in SocketData soc, in SocketData outsoc);
    ///<summary>
    ///Connects endpoint to socket
    ///</summary>
    [DllImport(DLL)] public static extern PResult connectEndpoint(in IPEndpointData ip, in SocketData soc);

    // Sending Data
    // TCP

    [DllImport(DLL)] private static extern PResult sendPacketData(in SocketData soc, IntPtr data, int datasize, out int bytesSent);
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

    [DllImport(DLL)] private static extern PResult recvPacketData(in SocketData soc, IntPtr dest, int numberOfBytes, out int bytesRecv);
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

    [DllImport(DLL)] private static extern PResult sendAllPacketData(in SocketData soc, IntPtr data, int numberOfBytes);
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

    [DllImport(DLL)] private static extern PResult recvAllPacketData(in SocketData soc, IntPtr dest, int numberOfBytes);
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

    [DllImport(DLL)] private static extern PResult recvFromPacketData(in SocketData soc, IntPtr data, int numberOfBytes, out int bytesRecv, out IPEndpointData ip);
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

    [DllImport(DLL)] private static extern PResult sendToPacketData(in SocketData soc, IntPtr data, int numberOfBytes, out int bytesSent, in IPEndpointData ip);
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