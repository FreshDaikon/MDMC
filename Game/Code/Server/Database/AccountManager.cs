

using LiteDB;
using Godot;
using System.Collections.Generic;

public partial class AccountManager : Node
{
    public static AccountManager Instance;
    private string dbPath;
    private List<User> onlineUsers = new List<User>();

    public override void _Ready()
    {
        if(Instance != null)
        {
            Free();
        }
        Instance = this;
        string osPath = OS.GetExecutablePath();
        int exeIndex = osPath.LastIndexOf("/");
        if(exeIndex >= 0)
        {
            dbPath = osPath.Substring(0, exeIndex);
        }
        dbPath += "/Database/Database.db";
        base._Ready();
    }

    public bool TryCredentials(string username, string password)
    {
        
        using(var db = new LiteDatabase(dbPath))
        {
            var users = db.GetCollection<User>("Accounts");
            users.EnsureIndex(x => x.Username);
            var result = users.FindOne(x => x.Username == username && x.Password == password);
            if(result != null)
            {
                if(onlineUsers.Contains(result))
                {
                    return false;
                }
                else
                {
                    onlineUsers.Add(result);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
    public bool TryRegister(string username, string password)
    {
        using(var db = new LiteDatabase(dbPath))
        {
            var users = db.GetCollection<User>("Accounts");
            users.EnsureIndex(x => x.Username);
            var result = users.FindOne(x => x.Username == username);
            if(result != null)
            {
                // Sometimes its just no good.
                return false;
            }
            else
            {
                var user = new User(username, password);
                users.Insert(user);
                return true;
            }
        }
    }

}