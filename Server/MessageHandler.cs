
public class MessageHandler
{
    public static void MessagePing(ClientState clientState, MessageBase messageBase)
    {
        clientState.lastPingTime = NetManager.GetNowTimeStamp();

        MessagePong messagePong = new MessagePong();
        NetManager.SocketSend(clientState, messagePong);
        Console.WriteLine("Message Ping");
    }

    public static void MessageRegister(ClientState clientState, MessageBase messageBase)
    {
        MessageRegister messageRegister = messageBase as MessageRegister;
        if (DBManager.TryRegister(messageRegister.id, messageRegister.pw))
        {
            DBManager.TryCreatePlayer(messageRegister.id);
            messageRegister.result = true;
        }
        NetManager.SocketSend(clientState, messageRegister);
    }

    public static void MessageLogin(ClientState clientState, MessageBase messageBase)
    {
        MessageLogin messageLogin = messageBase as MessageLogin;
        if (!DBManager.CheckPassword(messageLogin.id, messageLogin.pw))
        {
            messageLogin.result = false;
            NetManager.SocketSend(clientState, messageLogin);
            return;
        }

        if (clientState.player != null)
        {
            messageLogin.result = false;
            NetManager.SocketSend(clientState, messageLogin);
            return;
        }

        if (PlayerManager.IsOnline(messageLogin.id))
        {
            Player onlinePlayer = PlayerManager.GetPlayer(messageLogin.id);
            MessageKick messageKick = new MessageKick();
            messageKick.isKick = true;
            onlinePlayer.Send(messageKick);
            NetManager.Close(onlinePlayer.playerState);
        }

        PlayerData playerData = DBManager.GetPlayerData(messageLogin.id);
        if (playerData == null)
        {
            messageLogin.result = false;
            NetManager.SocketSend(clientState, messageLogin);
            return;
        }

        Player newPlayer = new Player(clientState)
        {
            id = messageLogin.id,
            playerData = playerData
        };
        PlayerManager.AddPlayer(messageLogin.id, newPlayer);
        clientState.player = newPlayer;
        messageLogin.result = true;
        newPlayer.Send(messageLogin);
    }

    #region Room

    public static void MessageCreateRoom(ClientState clientState, MessageBase messageBase)
    {
        MessageCreateRoom msg = messageBase as MessageCreateRoom;
        Player player = clientState.player;
        if (player == null)
        {
            msg.result = false;
            player.Send(msg);
            return;
        }
      
        if (player.roomId >= 0)
        {
            msg.result = false;
            player.Send(msg);
            return;
        }
        
        Room room = RoomManager.CreateRoom();
        msg.result = room.TryAddPlayer(player.id);
        player.Send(msg);
        
        // if (msg.result)
        // {
        //     MessageRoomData(clientState, new MessageRoomData()
        //     {
        //         roomId = room.roomId,
        //         playerDatas = new[]
        //         {
        //             new PlayerData()
        //             {
        //                 bean = clientState.player.playerData.bean,
        //                 id = clientState.player.playerData.id,
        //                 isHost = clientState.player.playerData.isHost,
        //                 isPrepare = clientState.player.playerData.isPrepare
        //             }
        //         }
        //     });
        // }
    }
    
    public static void MessageEnterRoom(ClientState clientState, MessageBase messageBase)
    {
        MessageEnterRoom msg = messageBase as MessageEnterRoom;
        Player player = clientState.player;
        if (player == null)
        {
            msg.result = false;
            player.Send(msg);
            return;
        }
        
        if (player.roomId >= 0)
        {
            msg.result = false;
            player.Send(msg);
            return;
        }
        
        Room room = RoomManager.GetRoom(msg.roomId);
        if (room == null)
        {
            msg.result = false;
            player.Send(msg);
            return;
        }
        
        msg.result = room.TryAddPlayer(player.id);
        player.Send(msg);
        
        if (msg.result)
        {
            int index = 0;
            MessageRoomData roomData = new MessageRoomData();
            PlayerData[] playerData = new PlayerData[room.players.Count];
            foreach (string playerId in room.players)
            {
                Player p = PlayerManager.GetPlayer(playerId);
                playerData[index] = new PlayerData()
                {
                    bean = p.playerData.bean,
                    id = p.playerData.id,
                    isHost = p.playerData.isHost,
                    isPrepare = p.playerData.isPrepare
                };
            }
        
            roomData.playerDatas = playerData;
            roomData.roomId = room.roomId;
            
            foreach (string playerId in room.players)
            {
                Player p = PlayerManager.GetPlayer(playerId);
                MessageRoomData(p.playerState, roomData);
            }
        }
    }
    
    public static void MessageExitRoom(ClientState clientState, MessageBase messageBase)
    {
        MessageExitRoom msg = messageBase as MessageExitRoom;
        Player player = clientState.player;
        if (player == null)
        {
            msg.result = false;
            player.Send(msg);
            return;
        }
        
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            msg.result = false;
            player.Send(msg);
            return;
        }

        msg.result = room.TryRemovePlayer(player.id);
        player.Send(msg);
        
        if (msg.result && room.players.Count > 0)
        {
            int index = 0;
            MessageRoomData roomData = new MessageRoomData();
            PlayerData[] playerData = new PlayerData[room.players.Count];
            foreach (string playerId in room.players)
            {
                Player p = PlayerManager.GetPlayer(playerId);
                playerData[index] = new PlayerData()
                {
                    bean = p.playerData.bean,
                    id = p.playerData.id,
                    isHost = p.playerData.isHost,
                    isPrepare = p.playerData.isPrepare
                };
            }
        
            roomData.playerDatas = playerData;
            roomData.roomId = room.roomId;
            
            foreach (string playerId in room.players)
            {
                Player p = PlayerManager.GetPlayer(playerId);
                MessageRoomData(p.playerState, roomData);
            }
        }
    }
    
    public static void MessagePlayerData(ClientState clientState, MessageBase messageBase)
    {
        MessagePlayerData msg = messageBase as MessagePlayerData;
        Player player = clientState.player;
        if (player == null) return;
        msg.bean = player.playerData.bean;
        msg.id = player.id;
        player.Send(msg);
    }
    
    public static void MessagePlayerPrepare(ClientState clientState, MessageBase messageBase)
    {
        MessagePlayerPrepare msg = messageBase as MessagePlayerPrepare;
        Player player = clientState.player;
        if (player == null) return;
        player.playerData.isPrepare = !player.playerData.isPrepare;
        player.Send(msg);

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null) return;
        
        int index = 0;
        MessageRoomData roomData = new MessageRoomData();
        PlayerData[] playerData = new PlayerData[room.players.Count];
        foreach (string playerId in room.players)
        {
            Player p = PlayerManager.GetPlayer(playerId);
            playerData[index] = new PlayerData()
            {
                bean = p.playerData.bean,
                id = p.playerData.id,
                isHost = p.playerData.isHost,
                isPrepare = p.playerData.isPrepare
            };
        }
        
        roomData.playerDatas = playerData;
        roomData.roomId = room.roomId;
            
        foreach (string playerId in room.players)
        {
            Player p = PlayerManager.GetPlayer(playerId);
            MessageRoomData(p.playerState, roomData);
        }
    }
    
    public static void MessageRoomData(ClientState clientState, MessageBase messageBase)
    {
        MessageRoomData msg = messageBase as MessageRoomData;
        Player player = clientState.player;
        if (player == null) return;
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null) return;
        int count = room.players.Count;
        PlayerData[] data = new PlayerData[count];
        int index = 0;
        foreach (string playerId in room.players)
        {
            Player p = PlayerManager.GetPlayer(playerId);
            data[index] = new PlayerData()
            {
                bean = p.playerData.bean,
                id = p.playerData.id,
                isHost = p.playerData.isHost,
                isPrepare = p.playerData.isPrepare
            };
            index++;
        }

        msg.playerDatas = data;
        msg.roomId = room.roomId;
        player.Send(msg);
    }
    
    public static void MessageRoomList(ClientState clientState, MessageBase messageBase)
    {
        MessageRoomList msg = messageBase as MessageRoomList;
        Player player = clientState.player;
        if (player == null) return;
        
        int count = RoomManager.roomsDic.Count;
        RoomData[] data = new RoomData[count];
        int index = 0;
        foreach (Room room in RoomManager.roomsDic.Values)
        {
            data[index] = new RoomData()
            {
                id = room.roomId,
                count = room.players.Count,
                isPrepare = room.state == Room.State.Prepare
            };
            index++;
        }

        msg.roomInfos = data;
        player.Send(msg);
    }
    
    public static void MessageStartPoker(ClientState clientState, MessageBase messageBase)
    {
        MessageStartPoker msg = messageBase as MessageStartPoker;
        Player player = clientState.player;
        if (player == null) return;
        Room room = RoomManager.GetRoom(player.roomId);
        bool flag = true;
        foreach (var playerId in room.players)
        {
            flag &= PlayerManager.GetPlayer(playerId).playerData.isPrepare;
        }

        if (player.playerData.isHost && room.players.Count == 3 && flag)
        {
            msg.result = true;
            
            room.state = Room.State.Start;
        }
        else
        {
            msg.result = false;
        }

        foreach (string playerId in room.players)
        {
            Player roomPlayer = PlayerManager.GetPlayer(playerId);
            roomPlayer.Send(msg);
        }
        
        if (msg.result)
        {
            room.StartPoker();
        }
    }

    #endregion

    #region Poker

    public static void MessageCardList(ClientState clientState, MessageBase messageBase)
    {
        MessageCardList msg = messageBase as MessageCardList;
        Player player = clientState.player;
        if (player == null) return;

        if (!player.playerData.isHost) return;
        
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null) return;
        
        List<CardData> headCardData = new List<CardData>();
        room.playerCards.TryGetValue("", out var headCards);
        foreach (var card in headCards)
        {
            headCardData.Add(new CardData()
            {
                cardRank = (int)card.cardRank,
                cardSuit = (int)card.cardSuit
            });
        }
        
        foreach (string playerId in room.players)
        {
            Player roomPlayer = PlayerManager.GetPlayer(playerId);
            if (room.playerCards.TryGetValue(roomPlayer.id, out var cards))
            {
                List<CardData> cardData = new List<CardData>();
                foreach (var card in cards)
                {
                    cardData.Add(new CardData()
                    {
                        cardRank = (int)card.cardRank,
                        cardSuit = (int)card.cardSuit
                    });
                }
                msg.cardData = cardData.ToArray();
                msg.headCardData = headCardData.ToArray();
            }
            roomPlayer.Send(msg);
        }
    }

    public static void MessageGetStartPlayer(ClientState clientState, MessageBase messageBase)
    {
        MessageGetStartPlayer msg = messageBase as MessageGetStartPlayer;
        Player player = clientState.player;
        if (player == null) return;

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null) return;

        msg.id = room.players[room.startPlayerIndex];

        room.SendAllPlayers(msg);
    }

    public static void MessageRoomPlayer(ClientState clientState, MessageBase messageBase)
    {
        MessageRoomPlayer msg = messageBase as MessageRoomPlayer;
        Player player = clientState.player;
        if (player == null) return;

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null) return;

        for (int i = 0; i < room.players.Count; i++)
        {
            if (room.players[i] == player.id)
            {
                msg.Myself = player.id;
                msg.MyselfBean = PlayerManager.GetPlayer(msg.Myself).playerData.bean;
                msg.RHO = room.players[i + 1 >= room.players.Count ? 0 : i + 1];
                msg.RHOBean = PlayerManager.GetPlayer(msg.RHO).playerData.bean;
                msg.LHO = room.players[i - 1 < 0 ? ^1 : i - 1];
                msg.LHOBean = PlayerManager.GetPlayer(msg.LHO).playerData.bean;
                break;
            }
        }
        player.Send(msg);
    }

    public static void MessagePokerCall(ClientState clientState, MessageBase messageBase)
    {
        MessagePokerCall msg = messageBase as MessagePokerCall;
        Player player = clientState.player;
        if (player == null) return;

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null) return;
        
        msg.id = player.id;
        //1、没人叫过；2、有人叫过，我抢；3、我叫过，再抢
        if (msg.call)
        {
            if (room.callId == "")
            {
                room.playersCall.Add(player.id, msg.call);
                room.callId = player.id;
                msg.result = 0;
            }
            else
            {
                if (room.playersCall.ContainsKey(player.id))
                {
                    msg.result = 3;
                }
                else
                {
                    room.playersCall.Add(player.id, msg.call);
                    msg.result = 1;
                }
            }
            var list = room.playersCall.Where(it => it.Value == false).ToList();
            if (list.Count == 2 && room.callId != "")
            {
                msg.result = 3;
                msg.id = room.callId;
            }
        }
        else//1、不叫；2、不抢；3、叫过，不抢；4、都不叫，重新洗
        {
            if (room.playersCall.ContainsKey(player.id))
            {
                msg.result = 5;
            }
            else
            {
                room.playersCall.Add(player.id, msg.call);

                if (room.callId == "")
                {
                    msg.result = 4;
                }
                else
                {
                    msg.result = 5;
                }
                
                var list = room.playersCall.Where(it => it.Value == false).ToList();
                if (list.Count == 3)
                {
                    msg.result = 2;
                }
                else if (list.Count == 2 && room.callId != "")
                {
                    msg.result = 3;
                    msg.id = room.callId;
                }
            }
        }

        if (msg.result == 3)
        {
            room.landLordId = player.id;
            room.pokerTurn = true;
        }
        
        room.SendAllPlayers(msg);
    }
    
    public static void MessageSwitchPickUp(ClientState clientState, MessageBase messageBase)
    {
        MessageSwitchPickUp msg = messageBase as MessageSwitchPickUp;
        Player player = clientState.player;
        if (player == null) return;

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null) return;

        var index = room.players.IndexOf(player.id);
        if (room.pokerTurn)
        {
            room.pokerTurn = false;
            index--;
        }
        room.turnIndex = index == room.players.Count() - 1
            ? 0
            : index + 1;
        msg.id = room.players[room.turnIndex];
        msg.isCall = room.callId != "";
        msg.canPass = room.prePlay || room.prePrePlay;
        room.SendAllPlayers(msg);
    }

    public static void MessagePokerRestart(ClientState clientState, MessageBase messageBase)
    {
        MessagePokerRestart msg = messageBase as MessagePokerRestart;
        Player player = clientState.player;
        if (player == null) return;
        
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null) return;
        
        CardManager.Shuffle();
        room.cards = CardManager.cards;
        room.StartPoker();
        
        room.SendAllPlayers(msg);
    }
    
    public static void MessageShowCards(ClientState clientState, MessageBase messageBase)
    {
        MessageShowCards msg = messageBase as MessageShowCards;
        Player player = clientState.player;
        if (player == null) return;
        
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null) return;

        List<Card> list = CardManager.DataToCard(msg.cards.ToList());
        if (list.Count != 0)
        {
            msg.cardType = (int)CardManager.GetCardType(list, out _);
            if (room.prePlay || room.prePrePlay)
            {
                msg.result = CardManager.Compare(room.preCards.ToArray(), list.ToArray());
            }
            else
            {
                msg.result = CardManager.GetCardType(list, out _) != CardManager.CardType.None;
            }
            
            if (msg.result)
            {
                room.RemovePlayCards(player.id, list);
                room.preCards = list;
                room.prePrePlay = room.prePlay;
                room.prePlay = true;
                msg.id = player.id;
            }
        }
        else
        {
            room.prePrePlay = room.prePlay;
            room.prePlay = false;
            msg.id = player.id;
            msg.result = true;
        }

        room.SendAllPlayers(msg);
    }
    
    public static void MessagePokerResult(ClientState clientState, MessageBase messageBase)
    {
        MessagePokerResult msg = messageBase as MessagePokerResult;
        Player player = clientState.player;
        if (player == null) return;
        
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null) return;

        if (player.id == room.landLordId)
        {
            foreach (var playerId in room.players)
            {
                PlayerData playerData = DBManager.GetPlayerData(playerId);
                if (playerId == player.id) playerData.bean += 200;
                else playerData.bean -= 100;
                DBManager.UpdatePlayerData(playerData.id, playerData);
            } 
        }
        else
        {
            foreach (var playerId in room.players)
            {
                PlayerData playerData = DBManager.GetPlayerData(playerId);
                if (playerId == room.landLordId) playerData.bean -= 200;
                else playerData.bean += 100;
                DBManager.UpdatePlayerData(playerData.id, playerData);
            } 
        }

        msg.id = player.id;
        room.SendAllPlayers(msg);
    }
    
    #endregion
}