using Transactions.Api.PresentationContracts;
using Transactions.Tests.Helpers;
using Libs.Auth.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Transactions.Tests.Generics;

public abstract class BaseIntegrationTest
{
    protected CustomWebApplicationFactory<Program> Factory { get; private set; }
    protected HttpClient Client { get; private set; }
    protected UserResponse AdminUser { get; private set; }

    protected virtual void Setup()
    {
        Factory = new CustomWebApplicationFactory<Program>();
        Client = Factory.CreateClient();
        MockUser("testadmin", "test", UserRole.Admin);
    }

    [TearDown]
    protected virtual void Dispose()
    {
        Factory.Dispose();
        Client.Dispose();
    }

    protected virtual void Authenticate(string email = "testadmin", string password = "test")
    {
        var content = new { email, password }.BuildJsonContent();
        var stringResult = Client.PostAsync("/api/auth/login", content).Result.Content.ReadAsStringAsync().Result;
        var result = stringResult.Deserialize<UserResponse>();
        AdminUser = result;

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
    }

    protected virtual void MockUser(string email, string password, params UserRole[] roles)
    {
        var user = new User(email, email);
        user.SetPassword(password);
        user.AddRoles(roles);

        Factory.DbContext.Users.InsertOne(user);
    }
}