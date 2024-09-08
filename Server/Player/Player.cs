



public class Player
{
    public string id = "";
    public ClientState playerState;
    public PlayerData playerData;
    public int roomId = -1;

    public Player(ClientState state)
    {
        playerState = state;
    }

    public void Send(MessageBase messageBase)
    {
        NetManager.SocketSend(playerState, messageBase);
    }
}