
using System.Security.Cryptography;

public static class RoomManager
{
    private static int maxId = 0;
    public static Dictionary<int, Room> roomsDic = new Dictionary<int, Room>();
    
    public static Room GetRoom(int id)
    {
        return roomsDic.TryGetValue(id, out var room) ? room : null;
    }

    public static Room CreateRoom()
    {
        maxId++;
        Room newRoom = new Room()
        {
            roomId = maxId,
        };
        roomsDic.Add(maxId, newRoom);
        return newRoom;
    }

    public static void RemoveRoom(int id)
    {
        roomsDic.Remove(id);
    }
}