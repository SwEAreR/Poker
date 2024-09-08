using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageRegister : MessageBase
{
    public MessageRegister()
    {
        protoName = "MessageRegister";
    }

    public string id;
    public string pw;
    public bool result;
}
