

public class PlayerManager
{
    private static Dictionary<string, Player> playersDic = new Dictionary<string, Player>();

    public static bool IsOnline(string id)
    {
        return playersDic.ContainsKey(id);
    }

    public static Player GetPlayer(string id)
    {
        return playersDic.TryGetValue(id, out var player) ? player : null;
    }

    public static void AddPlayer(string id, Player newPlayer)
    {
        RemovePlayer(id);
        playersDic.Add(id, newPlayer);
    }

    public static void RemovePlayer(string id)
    {
            playersDic.Remove(id);
    }
}