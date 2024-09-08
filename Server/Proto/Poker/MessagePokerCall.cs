public class MessagePokerCall : MessageBase
{
    public MessagePokerCall()
    {
        protoName = "MessagePokerCall";
    }
    
    public bool call;
    
    public string id;
    public int result;//0叫，1抢，2重新洗，3不需要,4不叫,5不抢
}