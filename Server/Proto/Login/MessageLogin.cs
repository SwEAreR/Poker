



public class MessageLogin : MessageBase
{
    public MessageLogin()
    {
        protoName = "MessageLogin";
    }
    
    public string id = "";
    public string pw = "";

    public bool result = false;
}