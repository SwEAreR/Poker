public class MainClass
{
    public static void Main(string[] args)
    {
        string ip = "127.0.0.1";
        int dbPort = 3306;
        int port = 8888;
        string db = "DouDiZhu";
        string userid = "root";
        string pw = "ZJHzjh05091229";
        if (!DBManager.IsConnect(db, ip, dbPort, userid, pw)) return;
        string id = "Zung123";
        NetManager.SocketConnect(ip, port);
    }
}