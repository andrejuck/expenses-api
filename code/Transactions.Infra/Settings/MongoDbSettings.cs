namespace Transactions.Infra.Settings;

public class MongoDbSettings
{
    public string Uri { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string DbName { get; set; } = null!;
    public string? AdminPassword { get; set; }
    public string ConnectionString
    {
        get => Uri
            .Replace("__username__", Username)
            .Replace("__password__", Password)
            .Replace("__db__", DbName);
    }

    public bool IsDevelopment { get; set; } = false;
    
    
}