
public class Room
{
    public int roomId = -1;
    public int playerMax = 3;
    public List<string> players = new List<string>();
    public string hostId = "";

    public Dictionary<string, bool> playersCall = new Dictionary<string, bool>();
    public int startPlayerIndex = 0;

    public int turnIndex = -1;
    public bool pokerTurn = false;
    public List<Card> preCards = new List<Card>();
    public bool prePlay;
    public bool prePrePlay;
    public string landLordId = "";

    public enum State
    {
        Prepare,
        Start
    }
    
    public enum PickUpState
    {
        None,
        Call,
        GiveUp
    }
    
    public State state = State.Prepare;

    public List<Card> cards;
    
    public Dictionary<string, List<Card>> playerCards = new Dictionary<string, List<Card>>();
    public string callId = "";
    
    public bool TryAddPlayer(string id)
    {
        Player player = PlayerManager.GetPlayer(id);
        if (player == null)
        {
            Console.WriteLine("Room.TryAddPlayer Fail，尝试添加空玩家");
            return false;
        }

        if (players.Count >= playerMax)
        {
            Console.WriteLine("Room.TryAddPlayer Fail，房间已满");
            return false;
        }
        
        if (state == State.Start)
        {
            Console.WriteLine("Room.TryAddPlayer Fail，房间已开始游戏");
            return false;
        }
        
        if (players.Contains(id))
        {
            Console.WriteLine("Room.TryAddPlayer Fail，玩家已在房间中");
            return false;
        }

        players.Add(id);
        player.roomId = this.roomId;
        if (hostId == "")
        {
            hostId = id;
            player.playerData.isHost = true;
        }
        return true;
    }

    public bool TryRemovePlayer(string id)
    {
        Player player = PlayerManager.GetPlayer(id);
        if (player == null)
        {
            Console.WriteLine("Room.TryRemovePlayer Fail，尝试移除空玩家");
            return false;
        }
        
        if (!players.Contains(id))
        {
            Console.WriteLine("Room.TryAddPlayer Fail，玩家不在房间中，无需移除");
            return false;
        }

        players.Remove(id);
        player.roomId = -1;
        player.playerData.isPrepare = false;
        if (player.playerData.isHost)
        {
            player.playerData.isHost = false;
            if (players.Count > 0)
            {
                hostId = players[0];
                Player newHost = PlayerManager.GetPlayer(players[0]);
                newHost.playerData.isHost = true;
            }
            else
            {
                hostId = "";
                RoomManager.RemoveRoom(roomId);
            }
        }
        return true;
    }

    public void StartPoker()
    {
        playersCall.Clear();
        playerCards.Clear();
        turnIndex = -1;
        pokerTurn = false;
        callId = "";
        preCards.Clear();

        CardManager.Shuffle();
        cards = CardManager.cards;

        for (int i = 0; i < 3; i++)
        {
            List<Card> c = new List<Card>();
            for (int j = i * 17; j < (i + 1) * 17; j++)
            {
                c.Add(cards[j]);
            }

            playerCards.Add(players[i], c);
            
            // int left = 0;
            // int right = 1;
            // playerCards[players[i]].Sort((a, b) => a.cardRank - b.cardRank);
            // for (left = 0; right < playerCards[players[i]].Count; left++,right++)
            // {
            //     if (playerCards[players[i]][left].cardRank == playerCards[players[i]][right].cardRank &&
            //         playerCards[players[i]][left].cardSuit == playerCards[players[i]][right].cardSuit)
            //     {
            //         Console.WriteLine("Server Give Same Card!");
            //     }
            // }
        }

        List<Card> headCards = new List<Card>();
        for (int i = 51; i < 54; i++)
        {
            headCards.Add(cards[i]);
        }
        playerCards.Add("", headCards);
    }

    public void SendAllPlayers(MessageBase messageBase)
    {
        foreach (string player in players)
        {
            PlayerManager.GetPlayer(player).Send(messageBase);
        }
    }

    public void RemovePlayCards(string id, List<Card> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            for (int j = playerCards[id].Count - 1; j >= 0; j--)
            {
                if (cards[i].Equals(playerCards[id][j]))
                {
                    playerCards[id].RemoveAt(j);
                }
            }
        }
    }
    
    public string GetPokerWinnerId()
    {
        foreach (var kv in playerCards)
        {
            if (kv.Value.Count == 0) return kv.Key;
        }

        return string.Empty;
    }
}