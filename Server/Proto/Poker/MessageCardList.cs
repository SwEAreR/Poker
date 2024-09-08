using System.Collections;
using System.Collections.Generic;

public class MessageCardList : MessageBase
{
    public MessageCardList()
    {
        protoName = "MessageCardList";
    }

    public CardData[] cardData = new CardData[17];
    public CardData[] headCardData = new CardData[3];
}
