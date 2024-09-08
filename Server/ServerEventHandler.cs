


public static class ServerEventHandler
{
    public static void OnDisConnectHandler(ClientState clientState)
    {
        if (clientState.player != null)
        {
            int roomId = clientState.player.roomId;
            if (roomId >= 0)
            {
                Room room = RoomManager.GetRoom(roomId);
                room.TryRemovePlayer(clientState.player.id);
            }
            
            DBManager.UpdatePlayerData(clientState.player.id, clientState.player.playerData);
            PlayerManager.RemovePlayer(clientState.player.id);
        }
    }

    public static void OnOverTimeHandler()
    {
        CheckPingPong();
    }

    public static void CheckPingPong()
    {
        foreach (ClientState clientState in NetManager.clientStatesDic.Values)
        {
            if (NetManager.GetNowTimeStamp() - clientState.lastPingTime > NetManager.pingInterval * 4)
            {
                Console.WriteLine("心跳机制断开连接");
                NetManager.Close(clientState);
                return;
            }
        }
    }
}