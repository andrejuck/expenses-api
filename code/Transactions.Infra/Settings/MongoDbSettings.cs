namespace Transactions.Infra.Settings;

public class MongoDbSettings
{
    public string Uri { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string DbName { get; set; }

    public string ConnectionString
    {
        get => Uri
            .Replace("__username__", Username)
            .Replace("__password__", Password)
            .Replace("__db__", DbName);
    }
}