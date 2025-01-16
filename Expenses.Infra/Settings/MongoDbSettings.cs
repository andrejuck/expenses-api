namespace Expenses.Infra.Settings;

public class MongoDbSettings {
    public string Uri { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string DbName { get; set; }
}