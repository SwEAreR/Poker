using System;

/// <summary>
/// 自定义传输数据类型
/// </summary>
public class ByteArray
{
    /// <summary>
    /// 默认容量
    /// </summary>
    private const int Default_Size = 1024;
    /// <summary>
    /// 初始大小
    /// </summary>
    private int initSize;
    /// <summary>
    /// 字节数组
    /// </summary>
    public byte[] bytes;
    /// <summary>
    /// 读索引
    /// </summary>
    public int readIndex;
    /// <summary>
    /// 写索引
    /// </summary>
    public int writeIndex;
    /// <summary>
    /// 容量
    /// </summary>
    private int capacity;
    /// <summary>
    /// 使用长度
    /// </summary>
    public int Length
    {
        get => writeIndex - readIndex;
    }
    /// <summary>
    /// 剩余
    /// </summary>
    public int Remain
    {
        get => capacity - writeIndex;
    }
    /// <summary>
    /// 设置数组
    /// </summary>
    /// <param name="size">数组大小</param>
    public ByteArray(int size = Default_Size)
    {
        bytes = new byte[size];
        initSize = size;
        capacity = size;
        readIndex = 0;
        writeIndex = 0;
    }
    /// <summary>
    /// 设置数组
    /// </summary>
    /// <param name="defaultBytesArray">数组</param>
    public ByteArray(byte[] defaultBytesArray)
    {
        bytes = defaultBytesArray;
        initSize = defaultBytesArray.Length;
        capacity = defaultBytesArray.Length;
        readIndex = 0;
        writeIndex = defaultBytesArray.Length;
    }
    /// <summary>
    /// 移动数据
    /// </summary>
    public void MoveBytes()
    {
        if (Length > 0)
        {
            Array.Copy(bytes, readIndex, bytes, 0, Length);
        }
        writeIndex = Length;
        readIndex = 0;
    }
    /// <summary>
    /// 设置数组容量
    /// </summary>
    /// <param name="newSize"></param>
    public void SetSize(int newSize)
    {
        if (newSize < initSize) return;
        initSize = newSize;
        capacity = newSize;
        byte[] newBytesArray = new byte[newSize];
        Array.Copy(bytes, readIndex, newBytesArray, 0, Length);
        bytes = newBytesArray;
        readIndex = 0;
        writeIndex = Length;
    }
}
