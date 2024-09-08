using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageRoomList : MessageBase
{
    public MessageRoomList()
    {
        protoName = "MessageRoomList";
    }

    public RoomData[] roomInfos;
}
