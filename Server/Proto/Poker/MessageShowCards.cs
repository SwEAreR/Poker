public class MessageShowCards : MessageBase
{
    public MessageShowCards()
    {
        protoName = "MessageShowCards";
    }

    public CardData[] cards;

    public string id;
    public int cardType;
    public bool result;
}