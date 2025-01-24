using System.Net;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.Models;
using Expenses.Tests.Generics;
using Expenses.Tests.Helpers;
using Libs.Auth.Models;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Expenses.Tests.Controller;

[TestFixture]
public class ModuleControllerIntegrationTests : BaseIntegrationTest
{
    private Uri BaseUri = new Uri("http://localhost/api/module/");

    [SetUp]
    protected override void Setup()
    {
        base.Setup();
        Authenticate();
    }

    [TestCase("GeneralUser", 2)]
    [TestCase("Admin", 5)]
    public async Task Should_Fetch_Modules_By_Role(string role, int expectedCount)
    {
        var user = new User("general", "general");
        MockUser(user.Email, "test", Enum.Parse<UserRole>(role));
        MockDatabase();
        Authenticate(user.Email);

        var result = await Client.GetAsync(BaseUri);
        var resultContent = result.Content.ReadAsStringAsync().Result.Deserialize<List<ModuleResponse>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resultContent.Count, Is.EqualTo(expectedCount));
    }

    [Test]
    public async Task Should_Initialize_Db_With_AdminModule()
    {
        var modules = await Factory.DbContext.Modules.Find(Builders<Module>.Filter.Empty).ToListAsync();

        Assert.IsNotEmpty(modules);
        Assert.That(modules.Any(x => x.Name.Equals("Admin Module")));
        Assert.That(modules.FirstOrDefault(x => x.Name.Equals("Admin Module")).AllowedRoles.All(x => x.Equals(UserRole.Admin)));
    }

    [Test]
    public async Task Should_Create_New_Module()
    {
        var form = new ModuleForm()
        {
            Name = "TestModule",
            AllowedRoles = new List<UserRole>() { UserRole.GeneralUser }
        };
        var content = form.BuildJsonContent();

        var result = await Client.PostAsync(BaseUri, content);

        Assert.That(result.IsSuccessStatusCode, Is.True);
        var createdModule = FindByName(form.Name).FirstOrDefault();
        Assert.NotNull(createdModule);
        Assert.That(form.AllowedRoles, Is.EqualTo(createdModule.AllowedRoles));
        Assert.That(createdModule.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public async Task Should_Not_Create_New_Module_With_Existing_Name()
    {
        MockDatabase();
        var form = new ModuleForm()
        {
            Name = "TestModAdmin",
            AllowedRoles = new List<UserRole>() { UserRole.GeneralUser }
        };
        var content = form.BuildJsonContent();

        var result = await Client.PostAsync(BaseUri, content);

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task Should_Soft_Delete_Module()
    {
        MockDatabase();
        var moduleToDelete = FindByName("TestModAdmin").FirstOrDefault();

        var result = await Client.DeleteAsync(new Uri(BaseUri, moduleToDelete.Id.ToString()));

        var deletedModule = FindByName("TestModAdmin").FirstOrDefault();
        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.NotNull(deletedModule.DeletedAt);
    }

    [Test]
    public async Task Should_Not_Soft_Delete_Inexisting_Module()
    {
        var result = await Client.DeleteAsync(new Uri(BaseUri, Guid.NewGuid().ToString()));

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private List<Module> FindByName(string name)
    {
        return Factory.DbContext.Modules.Find(Builders<Module>.Filter.Eq(x => x.Name, name)).ToList();
    }

    private void MockDatabase()
    {
        var modules = new List<Module>
        {
            new Module("TestModAdmin", UserRole.Admin),
            new Module("TestModuleAdmin2", UserRole.Admin),
            new Module("TestModuleGeneral", UserRole.GeneralUser)
        };

        Factory.DbContext.Modules.InsertMany(modules);
    }
}