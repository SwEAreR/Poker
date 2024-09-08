public class MessageRoomData : MessageBase
{
    public MessageRoomData()
    {
        protoName = "MessageRoomData";
    }

    public int roomId;
    public PlayerData[] playerDatas;
}
