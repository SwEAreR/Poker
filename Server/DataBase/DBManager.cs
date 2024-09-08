using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MySqlConnector;
using Newtonsoft.Json;



public class DBManager
{
    public static MySqlConnection mySql;

    public static bool IsConnect(string db, string ip, int port, string userId, string password)
    {
        mySql = new MySqlConnection();
        string s = $"Database={db};Data Source={ip};port={port};User Id={userId};Password={password};";
        mySql.ConnectionString = s;
        try {
            mySql.Open();
            Console.WriteLine("数据库启动成功");
            return true;
        }
        catch (Exception e) {
            Console.WriteLine("数据库启动失败" + e.Message);
            return false;
        }
    }

    public static bool IsAccountExist(string id)
    {
        if (!IsSafeString(id)) return true;
        string s = $"SELECT * FROM account WHERE id='{id}';";
        try
        {
            MySqlCommand mySqlCommand = new MySqlCommand(s, mySql);
            MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
            var result = mySqlDataReader.HasRows;
            mySqlDataReader.Close();
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine("数据库IsAccountExist Fail：" + e);
            return true;
        }
    }

    public static bool IsSafeString(string str)
    {
        return !Regex.IsMatch(str, @"[-|;|,|/|[|\]|\{|\}|%|@|*|!]");
    }

    public static bool TryRegister(string id, string pw)
    {
        if (!IsSafeString(id)) return false;
        if (!IsSafeString(pw)) return false;
        if (IsAccountExist(id)) return false;
        pw = MD5Encrypt(pw);//MD5加密
        string s = $"insert into account set id='{id}',pw='{pw}';";
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, mySql);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("数据库TryRegister Fail：" + e);
            return false;
        }
    }

    public static bool TryCreatePlayer(string id)
    {
        if (!IsSafeString(id)) return false;
        PlayerData playerData = new PlayerData()
        {
            id = id,
            bean = 2000,
            isHost = false,
            isPrepare = false
        };
        string data = JsonConvert.SerializeObject(playerData);
        
        string s=$"insert into player set id='{id}',data='{data}';";
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, mySql);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("数据库TryCreatePlayer Fail：" + e);
            return false;
        }
    }

    public static bool CheckPassword(string id, string pw)
    {
        if (!IsSafeString(id)) return false;
        pw = MD5Encrypt(pw);
        string s=$"SELECT * FROM account WHERE id='{id}' and pw='{pw}';";
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, mySql);
            MySqlDataReader reader = cmd.ExecuteReader();
            bool result = reader.HasRows;
            reader.Close();
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine("数据库CheckPassword Fail：" + e);
            return false;
        }
    }

    public static PlayerData GetPlayerData(string id)
    {
        if (!IsSafeString(id)) return null;
        string s=$"SELECT * FROM player WHERE id='{id}';";
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, mySql);
            MySqlDataReader reader = cmd.ExecuteReader();
            bool result = reader.HasRows;
            if (!result)
            {
                reader.Close();
                return null;
            }

            reader.Read();
            string data = reader.GetString("data");
            PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(data);
            reader.Close();
            return playerData;
        }
        catch (Exception e)
        {
            Console.WriteLine("数据库GetPlayerData Fail：" + e);
            return null;
        }
    }

    public static bool UpdatePlayerData(string id, PlayerData playerData)
    {
        string data = JsonConvert.SerializeObject(playerData);
        string s = $"update player set data='{data}' where id='{id}';";
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, mySql);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("数据库UpdatePlayerData Fail：" + e);
            return false;
        }
    }

    public static string MD5Encrypt(string str)
    {
        MD5 md5 = MD5.Create();
        byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var b in bytes)
        {
            stringBuilder.Append(b.ToString("x2"));
        }

        return stringBuilder.ToString();
    }
}