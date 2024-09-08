using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageKick : MessageBase
{
    public MessageKick()
    {
        protoName = "MessageLogin";
    }
    
    public bool isKick = false;
}
