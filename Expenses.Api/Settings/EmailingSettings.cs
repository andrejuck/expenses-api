namespace Expenses.Api.Settings; 

public class EmailingSettings {

    public bool IsEnabled { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public bool EnableSSL { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}