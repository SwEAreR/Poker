using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageRoomData : MessageBase
{
    public MessageRoomData()
    {
        protoName = "MessageRoomData";
    }

    public int roomId;
    public PlayerData[] playerDatas;
}
