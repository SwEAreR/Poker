using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;

public class NetManager
{
    /// <summary>
    /// 客户端Socket
    /// </summary>
    private static Socket clientSocket;
    /// <summary>
    /// 字节数组
    /// </summary>
    private static ByteArray byteArray;
    /// <summary>
    /// 读队列
    /// </summary>
    private static Queue<ByteArray> readQueue;
    /// <summary>
    /// 写队列
    /// </summary>
    private static Queue<ByteArray> writeQueue;
    /// <summary>
    /// 是否正在连接
    /// </summary>
    private static bool isConnecting;
    /// <summary>
    /// 是否正在关闭
    /// </summary>
    private static bool isClosing;
    /// <summary>
    /// 状态
    /// </summary>
    public enum NetEvent
    {
        ConnectSuccess = 1,
        ConnectFail = 2,
        Close = 3,
    }
    /// <summary>
    /// 网络状态事件委托
    /// </summary>
    public delegate void StateEventListener(string str);
    
    public static Dictionary<NetEvent, StateEventListener> netEventListenersDic = new Dictionary<NetEvent, StateEventListener>();
    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="ip">ip地址</param>
    /// <param name="port">端口号</param>
    public static void SocketConnect(string ip, int port)
    {
        //if (clientSocket is { Connected: false })
        if (clientSocket != null && clientSocket.Connected)
        {
            Debug.LogWarning("连接失败，已连接");
            return;
        }
    
        if (isConnecting)
        {
            Debug.Log("正在连接");
            return;
        }
        Init();
        isConnecting = true;
        clientSocket.BeginConnect(ip, port, ConnectCallback, clientSocket);
    }
    /// <summary>
    /// 连接回调
    /// </summary>
    /// <param name="ar"></param>
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket clientSocket = (Socket)ar.AsyncState;
            clientSocket.EndConnect(ar);
            isConnecting = false;
            FireStateEvent(NetEvent.ConnectSuccess, "ConnectSuccess");
            clientSocket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0, ReceiveCallback,
                clientSocket);
        }
        catch (Exception e)
        {
            isConnecting = false;
            FireStateEvent(NetEvent.ConnectFail, e.ToString());
            Debug.Log("连接失败\n" + e);
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket clientSocket = ar.AsyncState as Socket;
            int count = clientSocket.EndReceive(ar);
            if (count == 0) //消息接受完毕，断开连接等待下次接受
            {
                Close();
                return;
            }

            byteArray.writeIndex += count;
            OnReceiveData();
            if (byteArray.Remain < 8)// byteArray长度过小，扩容
            {
                byteArray.MoveBytes();
                byteArray.SetSize(byteArray.Length * 2);
            }

            clientSocket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, SocketFlags.None,
                ReceiveCallback, clientSocket);
        }
        catch (Exception e)
        {
            Debug.LogWarning("接受消息失败" + e);
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private static void Init()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        byteArray = new ByteArray();
        readQueue = new Queue<ByteArray>();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        isClosing = false;

        messageList = new List<MessageBase>();
        messageCount = 0;

        lastPingTime = Time.time;
        lastPongTime = Time.time;

        if (!messageListenersDic.ContainsKey("MessagePong"))
        {
            AddMessageListener("MessagePong", OnMessagePong);
        }
    }

    /// <summary>
    /// 添加状态事件
    /// </summary>
    /// <param name="event"></param>
    /// <param name="listener"></param>
    public static void AddStateEvent(NetEvent @event, StateEventListener listener)
    {
        if (netEventListenersDic.ContainsKey(@event))
        {
            netEventListenersDic[@event] += listener;
        }
        else
        {
            netEventListenersDic[@event] = listener;
        }
    }
    /// <summary>
    /// 删除状态事件
    /// </summary>
    /// <param name="event"></param>
    /// <param name="listener"></param>
    public static void RemoveStateEvent(NetEvent @event, StateEventListener listener)
    {
        if (netEventListenersDic.ContainsKey(@event))
        {
            netEventListenersDic[@event] -= listener;
            if (netEventListenersDic[@event] == null)
            {
                netEventListenersDic.Remove(@event);
            }
        }
    }
    /// <summary>
    /// 调用状态事件
    /// </summary>
    /// <param name="event"></param>
    /// <param name="str"></param>
    private static void FireStateEvent(NetEvent @event, string str)
    {
        if (netEventListenersDic.ContainsKey(@event))
        {
            netEventListenersDic[@event].Invoke(str);
        }
    }
    /// <summary>
    /// 断开连接
    /// </summary>
    public static void Close()
    {
        if (clientSocket == null || !clientSocket.Connected) return;
        if (isConnecting) return;
        if (writeQueue.Count > 0)   //有消息未发送
        {
            isClosing = true;
        }
        else
        {
            FireStateEvent(NetEvent.Close, "ConnectClose");
            Debug.Log("客户端断开连接");
            clientSocket.Close();
        }
    }

    public static void Send(MessageBase messageBase)
    {
        if (clientSocket == null || !clientSocket.Connected) return;
        if (isConnecting || isClosing) return;

        byte[] nameBytes = MessageBase.EncodeName(messageBase);
        byte[] bodyBytes = MessageBase.Encode(messageBase);
        int length = nameBytes.Length + bodyBytes.Length;   //协议名与协议的长度
        byte[] sendBytes = new byte[length + 2];
        //协议名与协议的长度转为两个字节
        sendBytes[0] = (byte)(length % 256);
        sendBytes[1] = (byte)(length / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, nameBytes.Length + 2, bodyBytes.Length);

        ByteArray sendByteArray = new ByteArray(sendBytes);
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(sendByteArray);
            count = writeQueue.Count;
            
            if (count == 1)
            {
                clientSocket.BeginSend(sendBytes, 0, sendBytes.Length, SocketFlags.None, SendCallBack, clientSocket);
            }
        }
    }
    private static void SendCallBack(IAsyncResult ar)
    {
        Socket clientSocket = ar.AsyncState as Socket;
        if (clientSocket == null || !clientSocket.Connected) return;
        //count 为成功发送的字节数
        int count = clientSocket.EndSend(ar);

        ByteArray byteArray;
        
        lock (writeQueue)
        {
            byteArray = writeQueue.First();
        }
        
        byteArray.readIndex += count;

        if (byteArray.Length == 0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();
                if (writeQueue.Count > 0)
                    byteArray = writeQueue.First();
            }
        }

        if (byteArray != null && byteArray.Length != 0)
        {
            Debug.Log("继续发消息");
            clientSocket.BeginSend(byteArray.bytes, byteArray.readIndex, byteArray.Length, SocketFlags.None, SendCallBack, clientSocket);
        }
        else if (isClosing) //消息发完且待关闭
        {
            clientSocket.Close();
        }
    }

    /// <summary>
    /// 消息委托
    /// </summary>
    public delegate void MessageListener(MessageBase messageBase);
    /// <summary>
    /// 消息委托字典
    /// </summary>
    private static Dictionary<string, MessageListener> messageListenersDic = new Dictionary<string, MessageListener>();
    /// <summary>
    /// 根据协议名添加事件
    /// </summary>
    /// <param name="messageName">协议名</param>
    /// <param name="listener">事件</param>
    public static void AddMessageListener(string messageName, MessageListener listener)
    {
        if (messageListenersDic.ContainsKey(messageName))
        {
            messageListenersDic[messageName] += listener;
        }
        else
        {
            messageListenersDic[messageName] = listener;
        }
    }
    /// <summary>
    /// 根据协议名移除事件
    /// </summary>
    /// <param name="messageName"></param>
    /// <param name="listener"></param>
    public static void RemoveMessageListener(string messageName, MessageListener listener)
    {
        if (messageListenersDic.ContainsKey(messageName))
        {
            messageListenersDic[messageName] -= listener;
            if (messageListenersDic[messageName] == null)
            {
                messageListenersDic.Remove(messageName);
            }
        }
    }
    /// <summary>
    /// 委托调用
    /// </summary>
    /// <param name="messageName">协议名</param>
    /// <param name="messageBase"></param>
    private static void FireMessageListener(string messageName, MessageBase messageBase)
    {
        if (messageListenersDic.ContainsKey(messageName))
        {
            messageListenersDic[messageName].Invoke(messageBase);
        }
    }
    private static List<MessageBase> messageList;
    private static int messageCount = 0;

    public static void OnReceiveData()
    {
        if (byteArray.Length <= 2) return;
        int readIndex = byteArray.readIndex;
        byte[] bytes = byteArray.bytes;
        //将高位字节乘以256，将其值向左移动8位（一个字节的位数），再加低位字节的值，将两个字节合并成一个16位的值
        short bodyLength = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);
        if (byteArray.Length < bodyLength + 2) return;
        byteArray.readIndex += 2;
        int nameCount = 0;
        string protoName = MessageBase.DecodeName(byteArray.bytes, byteArray.readIndex, out nameCount);
        if (protoName == "")
        {
            Debug.Log("接收消息解析失败");
            return;
        }
        byteArray.readIndex += nameCount;
        
        int bodyCount = bodyLength - nameCount;
        MessageBase messageBase = MessageBase.Decode(protoName, byteArray.bytes, byteArray.readIndex, bodyCount);
        byteArray.readIndex += bodyCount;
        byteArray.MoveBytes();
        lock (messageList)
        {
            messageList.Add(messageBase);
        }

        messageCount++;
        if (byteArray.Length > 2)
        {
            OnReceiveData();
        }
    }

    public static void MessageUpdate()
    {
        if (messageCount == 0) return;
        for (int i = 0; i < messageCountMax; i++)
        {
            MessageBase messageBase = null;
            lock (messageList)
            {
                if (messageCount > 0)
                {
                    messageBase = messageList.First();
                    messageList.RemoveAt(0);
                    messageCount--;
                }
            }

            if (messageBase != null)
            {
                FireMessageListener(messageBase.protoName, messageBase);
            }
            else
            {
                break;
            }
        }
    }

    private static int messageCountMax = 10;

    public static void NetManagerUpdate()
    {
        MessageUpdate();
        PingUpdate();
    }
    
    /// <summary>
    /// 
    /// </summary>
    public static bool isUsePing = true;
    public static int pingInterval = 30;
    private static float lastPingTime = 0;
    private static float lastPongTime = 0;

    private static void PingUpdate()
    {
        if (!isUsePing) return;

        if (Time.time - lastPingTime > pingInterval)
        {
            MessagePing messagePing = new MessagePing();
            lastPingTime = Time.time;
            Send(messagePing);
        }

        if (Time.time - lastPongTime > pingInterval * 4)
        {
            Close();
        }
    }
    
    private static void OnMessagePong(MessageBase messageBase)
    {
        MessagePing messagePing = messageBase as MessagePing;
        lastPongTime = Time.time;
    }
}
