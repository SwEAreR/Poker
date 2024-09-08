using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MessageBase
{
    public string protoName = "";

    /// <summary>
    /// 编码
    /// </summary>
    /// <param name="messageBase"></param>
    /// <returns></returns>
    public static byte[] Encode(MessageBase messageBase)
    {
        string str = JsonUtility.ToJson(messageBase);
        return Encoding.UTF8.GetBytes(str);
    }
    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="protoName">协议名</param>
    /// <param name="bytes">字节数组</param>
    /// <param name="offset">偏移</param>
    /// <param name="count">数目</param>
    /// <returns></returns>
    public static MessageBase Decode(string protoName, byte[] bytes, int offset, int count)
    {
        string str = Encoding.UTF8.GetString(bytes, offset, count);
        return JsonUtility.FromJson(str, Type.GetType(protoName)) as MessageBase;
    }
    /// <summary>
    /// 编码协议名，第一二个字节为特殊
    /// </summary>
    /// <param name="messageBase"></param>
    /// <returns></returns>
    public static byte[] EncodeName(MessageBase messageBase)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(messageBase.protoName);
        int length = nameBytes.Length;
        byte[] bytes = new byte[length + 2];
        //将length转为两个字节，以表明协议名的长度
        bytes[0] = (byte)(length % 256);
        bytes[1] = (byte)(length / 256);
        Array.Copy(nameBytes, 0, bytes, 2, length);
        return bytes;
    }

    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        if (offset + 2 > bytes.Length) return "";
        //获取协议名的长度
        short length = (short)(bytes[offset + 1] * 256 + bytes[offset]);
        if (length <= 0) return "";
        count = length + 2;
        return Encoding.UTF8.GetString(bytes, offset + 2, length);
    }
}
