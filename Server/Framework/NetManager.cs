using System.Net;
using System.Net.Sockets;
using System.Reflection;

public static class NetManager
{
    public static Socket serverSocket;
    public static Dictionary<Socket, ClientState> clientStatesDic = new Dictionary<Socket, ClientState>();
    private static List<Socket> socketsSelectList = new List<Socket>();

    public static void SocketConnect(string ip, int port)
    {
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = IPAddress.Parse(ip);
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
        serverSocket.Bind(ipEndPoint);
        serverSocket.Listen();
        while (true) {
            socketsSelectList.Clear();
            socketsSelectList.Add(serverSocket);
            foreach (ClientState clientState in clientStatesDic.Values) {
                socketsSelectList.Add(clientState.clientSocket);
            }
            Socket.Select(socketsSelectList, null, null, 1000);
            foreach (Socket socket in socketsSelectList) {
                if (socket == serverSocket){ //有客户端请求连接
                    SocketAccept(socket);
                }
                else {
                    ReceiveMessage(socket); //有客户端发送消息
                }
            }
            Timer();
        }
    }
    
    private static void SocketAccept(Socket socket)
    {
        try
        {
            Socket clientSocket = socket.Accept();
            Console.WriteLine("Accept: "+ clientSocket.RemoteEndPoint);

            ClientState newState = new ClientState()
            {
                clientSocket = clientSocket,
                lastPingTime = GetNowTimeStamp()
            };
            clientStatesDic.Add(clientSocket, newState);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Accept Fail: " + e.ToString());
        }
    }
    
    private static void ReceiveMessage(Socket clientSocket)
    {
        ClientState clientState = clientStatesDic[clientSocket];
        ByteArray byteArray = clientState.byteArray;

        int count = 0;
        if (byteArray.Remain <= 0)
        {
            byteArray.MoveBytes();
        }
        if (byteArray.Remain <= 0)
        {
            Console.WriteLine("Receive Fail: 数组长度不足");
            Close(clientState);
            return;
        }

        try
        {
            count = clientSocket.Receive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, SocketFlags.None);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Receive Fail: "+ e.ToString());
            Close(clientState);
            return;
        }

        if (count <= 0)
        {
            Console.WriteLine("Socket Close: " + clientSocket.RemoteEndPoint.ToString());
            Close(clientState);
            return;
        }

        byteArray.writeIndex += count;
        OnReceiveData(clientState);
        byteArray.MoveBytes();
    }

    public static void Close(ClientState clientState)
    {
        ServerEventHandler.OnDisConnectHandler(clientState);
        
        clientState.clientSocket.Close();
        clientStatesDic.Remove(clientState.clientSocket);
    }

    public static void OnReceiveData(ClientState clientState)
    {
        ByteArray byteArray = clientState.byteArray;
        byte[] bytes = byteArray.bytes;
        
        if (byteArray.Length <= 2) return;
        //协议名与协议长度
        short msgLength = (short)(bytes[byteArray.readIndex + 1] * 256 + bytes[byteArray.readIndex]);
        if (byteArray.Length < msgLength) return;
        byteArray.readIndex += 2;

        int nameCount = 0;
        //解析协议名
        string protoName = MessageBase.DecodeName(byteArray.bytes, byteArray.readIndex, out nameCount);
        if (protoName == "")
        {
            Console.WriteLine("OnReceiveData Fail");
            Close(clientState);
            return;
        }
        byteArray.readIndex += nameCount;
        
        //解析协议
        int bodyCount = msgLength - nameCount;
        MessageBase messageBase = MessageBase.Decode(protoName, byteArray.bytes, byteArray.readIndex, bodyCount);
        byteArray.readIndex += bodyCount;
        byteArray.MoveBytes();

        MethodInfo methodInfo = typeof(MessageHandler).GetMethod(protoName);
        if (methodInfo != null)
        {
            object[] para = { clientState, messageBase };
            methodInfo.Invoke(null, para);
        }
        else
        {
            Console.WriteLine("OnReceiveData: 反射调用函数失败，请检查协议名！");
            Close(clientState);
            return;
        }

        if (byteArray.Length > 2)
        {
            OnReceiveData(clientState);
        }
    }

    public static void SocketSend(ClientState clientState, MessageBase messageBase)
    {
        if (clientState == null || !clientState.clientSocket.Connected) return;

        byte[] nameBytes = MessageBase.EncodeName(messageBase);
        byte[] bodyBytes = MessageBase.Encode(messageBase);
        int length = nameBytes.Length + bodyBytes.Length;

        byte[] sendBytes = new byte[length + 2];
        sendBytes[0] = (byte)(length % 256);
        sendBytes[1] = (byte)(length / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        try
        {
            clientState.clientSocket.Send(sendBytes, 0, sendBytes.Length, SocketFlags.None);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Socket Send Fail" + e);
        }
    }

    private static void Timer()
    {
        ServerEventHandler.OnOverTimeHandler();
    }

    public static long pingInterval = 30;

    public static long GetNowTimeStamp()
    {
        TimeSpan time = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        return Convert.ToInt64(time.TotalSeconds);
    }
}