using AutoMapper;
using Expenses.Api.Adapters;
using Expenses.Api.Controllers;
using Expenses.Api.PresentationContracts;
using Expenses.Domain.DataContracts;
using Expenses.Infra.Repositories;
using Expenses.Tests.Generics;
using Libs.Api.Adapters;
using Libs.Api.Models;
using Libs.Auth.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Expenses.Tests.Controller;

[TestFixture]
public class UserControllerIntegrationTests : BaseIntegrationTest
{
    private IUserRepository _repository;
    private IPaginationAdapter _pageAdapter;
    private IMapper _mapper;
    private UserController _controller;

    [SetUp]
    protected override void Setup()
    {
        base.Setup();
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
        _pageAdapter = new PaginationAdapter();
        _repository = new UserRepository(_dbContext);
        _controller = new UserController(_repository, _pageAdapter, _mapper);

        MockDatabase();
    }

    [Test]
    public async Task Should_Approve_UserRegistration()
    {
        var user = FindUsers().First();

        var result = await _controller.ApproveUserRegistration(user.Id);

        var updatedUser = FindUserById(user.Id);
        Assert.IsInstanceOf<AcceptedResult>(result);
        Assert.That(updatedUser.RegistrationStatus, Is.EqualTo(RegistrationStatus.Approved));
        Assert.That(updatedUser.EmailConfirmed, Is.True);
    }

    [Test]
    public async Task Should_Fetch_All_Users()
    {
        var request = new PagedRequest() { CurrentPage = 1, PageSize = 2};

        var result = await _controller.GetPaginatedUserList(request);
        
        Assert.IsInstanceOf<OkObjectResult>(result.Result);
        var resultObj = result.Result as OkObjectResult;
        Assert.IsInstanceOf<PagedResponse<UserResponse>>(resultObj.Value);
        var responseObj = resultObj.Value as PagedResponse<UserResponse>;
        Assert.That(responseObj.Result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Should_Deny_UserRegistration()
    {
        var user = FindUsers().First();

        var result = await _controller.DenyUserRegistration(user.Id);

        var updatedUser = FindUserById(user.Id);
        Assert.IsInstanceOf<AcceptedResult>(result);
        Assert.That(updatedUser.RegistrationStatus, Is.EqualTo(RegistrationStatus.Denied));
    }

    [Test]
    public void Should_Fecth_All_UserRegistration_Status_Names()
    {
        var result = _controller.GetAllRegistrationStatus();

        Assert.IsInstanceOf<OkObjectResult>(result.Result);
        var responseObj = result.Result as OkObjectResult;
        Assert.That((responseObj.Value as List<string>).Count, Is.EqualTo(4));
    }

    private void MockDatabase()
    {
        var users = new List<User> 
        {
            new User("test@test.mock", "test"),
            new User("test2@test.mock", "test2"),
        };

        _dbContext.Users.InsertMany(users);
    }

    private List<User> FindUsers()
    {
        return _dbContext.Users.Find(Builders<User>.Filter.Empty).ToList();
    }

    private User FindUserById(Guid id)
    {
        return _dbContext.Users.Find(Builders<User>.Filter.Eq(x => x.Id, id)).FirstOrDefault();
    }
}