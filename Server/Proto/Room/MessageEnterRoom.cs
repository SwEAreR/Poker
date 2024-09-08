public class MessageEnterRoom : MessageBase
{
    public MessageEnterRoom()
    {
        protoName = "MessageEnterRoom";
    }

    public int roomId;
    public bool result;
}