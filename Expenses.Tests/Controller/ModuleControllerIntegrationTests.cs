using AutoMapper;
using Expenses.Api.Adapters;
using Expenses.Api.Controllers;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Expenses.Infra.Repositories;
using Expenses.Tests.Generics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Expenses.Tests.Controller;

[TestFixture]
public class ModuleControllerIntegrationTests : BaseIntegrationTest
{
    public IModuleRepository _moduleRepository;
    private IMapper _mapper;
    private ModuleController _controller;

    [SetUp]
    protected override void Setup()
    {
        base.Setup();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ModuleProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _moduleRepository = new ModuleRepository(_dbContext);

        _controller = new ModuleController(_moduleRepository, _mapper);
    }

    [TestCase("GeneralUser", 1)]
    [TestCase("Admin", 3)]
    public async Task Should_Fetch_Modules_By_Role(string role, int expectedCount)
    {
        MockDatabase();
        _controller.ControllerContext = AddRoleToControllerContext(role);

        var actionResult = (await _controller.FetchAllModulesByRoleAsync()).Result as OkObjectResult;
        var result = actionResult.Value as List<ModuleResponse>;

        Assert.That(result.Count == expectedCount);
    }

    [Test]
    public async Task Should_Initialize_Db_With_AdminModule()
    {
        var modules = await _dbContext.Modules.Find(Builders<Module>.Filter.Empty).ToListAsync();

        Assert.IsNotEmpty(modules);
        Assert.That(modules.Count == 1);
        Assert.That(modules.FirstOrDefault().Name == "Admin Module");
        Assert.That(modules.FirstOrDefault().AllowedRoles.All(x => x == UserRole.Admin));
    }

    [Test]
    public async Task Should_Create_New_Module()
    {
        var form = new ModuleForm()
        {
            Name = "TestModule",
            AllowedRoles = new List<UserRole>() { UserRole.GeneralUser }
        };

        var result = await _controller.CreateModule(form);

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

        var result = await _controller.CreateModule(form);

        Assert.IsInstanceOf<ConflictObjectResult>(result);
    }

    [Test]
    public async Task Should_Soft_Delete_Module()
    {
        MockDatabase();
        var moduleToDelete = FindByName("TestModAdmin").FirstOrDefault();

        var result = await _controller.DeleteModule(moduleToDelete.Id);

        var deletedModule = FindByName("TestModAdmin").FirstOrDefault();
        Assert.IsInstanceOf<AcceptedResult>(result);
        Assert.NotNull(deletedModule.DeletedAt);
    }

    [Test]
    public async Task Should_Not_Soft_Delete_Inexisting_Module()
    {
        var result = await _controller.DeleteModule(Guid.NewGuid());

        Assert.IsInstanceOf<NotFoundObjectResult>(result);
    }

    private List<Module> FindByName(string name)
    {
        return _dbContext.Modules.Find(Builders<Module>.Filter.Eq(x => x.Name, name)).ToList();
    }

    private void MockDatabase()
    {
        var modules = new List<Module>
        {
            new Module("TestModAdmin", UserRole.Admin),
            new Module("TestModuleAdmin2", UserRole.Admin),
            new Module("TestModuleGeneral", UserRole.GeneralUser)
        };

        _dbContext.Modules.InsertMany(modules);
    }
}