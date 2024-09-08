using System.Net.Sockets;



public class ClientState
{
    public Socket clientSocket;
    public ByteArray byteArray = new ByteArray();
    public long lastPingTime = 0;
    public Player player;
}