using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessagePlayerData : MessageBase
{
    public MessagePlayerData()
    {
        protoName = "MessagePlayerData";
    }

    public string id;
    public int bean;
}
