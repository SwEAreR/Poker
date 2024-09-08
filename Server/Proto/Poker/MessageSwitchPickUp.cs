public class MessageSwitchPickUp : MessageBase
{
    public MessageSwitchPickUp()
    {
        protoName = "MessageSwitchPickUp";
    }

    public string id;
    public bool isCall;
    public bool canPass;
}