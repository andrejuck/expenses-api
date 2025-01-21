using System.Security.Claims;
using AutoMapper;
using Expenses.Infra;
using Expenses.Infra.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NSubstitute;

namespace Expenses.Tests.Generics;

public abstract class BaseIntegrationTest
{
    protected MongoDbRunner _runner;
    protected DBContext _dbContext;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }

    protected virtual void Setup()
    {
        _runner = MongoDbRunner.Start();
        var mongoSettings = new MongoDbSettings() { DbName = "TestDB" };
        _dbContext = new DBContext(_runner.ConnectionString, mongoSettings);
    }

    [TearDown]
    protected virtual void Dispose()
    {
        _runner.Dispose();
    }

    protected virtual ControllerContext AddRoleToControllerContext(string role)
    {
        var httpContext = Substitute.For<HttpContext>();
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, role)
        }));

        httpContext.User.Returns(claimsPrincipal);
        return new ControllerContext
        {
            HttpContext = httpContext
        };
    }
}