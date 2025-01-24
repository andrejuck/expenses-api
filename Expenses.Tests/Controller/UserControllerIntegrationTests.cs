using Expenses.Api.PresentationContracts;
using Expenses.Tests.Generics;
using Expenses.Tests.Helpers;
using Libs.Api.Models;
using Libs.Auth.Models;
using MongoDB.Driver;

namespace Expenses.Tests.Controller;

[TestFixture]
public class UserControllerIntegrationTests : BaseIntegrationTest
{
    private UriBuilder BaseUriBuilder = new UriBuilder("http://localhost/api/user");

    [SetUp]
    protected override void Setup()
    {
        base.Setup();
        Authenticate();
        MockDatabase();
    }

    [Test]
    public async Task Should_Approve_UserRegistration()
    {
        var user = FindUsers().First();

        var result = await Client.PatchAsync(BaseUriBuilder.Path + "/approve/" + user.Id.ToString(), null);

        var updatedUser = FindUserById(user.Id);
        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(updatedUser.RegistrationStatus, Is.EqualTo(RegistrationStatus.Approved));
        Assert.That(updatedUser.EmailConfirmed, Is.True);
    }

    [Test]
    public async Task Should_Fetch_All_Users()
    {
        var request = new PagedRequest() { CurrentPage = 1, PageSize = 3};
        BaseUriBuilder.Query =  request.BuildQueryParams();

        var result = await Client.GetAsync(BaseUriBuilder.Uri);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<PagedResponse<UserResponse>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(content.Result.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task Should_Deny_UserRegistration()
    {
        var user = FindUsers().First();

        var result = await Client.PatchAsync(BaseUriBuilder.Path + "/deny/" + user.Id.ToString(), null);

        var updatedUser = FindUserById(user.Id);
        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(updatedUser.RegistrationStatus, Is.EqualTo(RegistrationStatus.Denied));
    }

    [Test]
    public async Task Should_Fecth_All_UserRegistration_Status_Names()
    {
        var result = await Client.GetAsync(BaseUriBuilder.Path + "/status");
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<List<string>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(content.Count, Is.EqualTo(4));
    }

    private void MockDatabase()
    {
        var users = new List<User> 
        {
            new User("test@test.mock", "test"),
            new User("test2@test.mock", "test2"),
        };

        Factory.DbContext.Users.InsertMany(users);
    }

    private List<User> FindUsers()
    {
        return Factory.DbContext.Users.Find(Builders<User>.Filter.Empty).ToList();
    }

    private User FindUserById(Guid id)
    {
        return Factory.DbContext.Users.Find(Builders<User>.Filter.Eq(x => x.Id, id)).FirstOrDefault();
    }
}