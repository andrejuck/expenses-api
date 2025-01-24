using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.Models;
using Expenses.Domain.Models.Enum;
using Expenses.Tests.Generics;
using Expenses.Tests.Helpers;
using Microsoft.AspNetCore.JsonPatch;
using MongoDB.Bson.IO;
using MongoDB.Driver;

namespace Expenses.Tests.Controller;

[TestFixture]
public class PaymentMethodControllerIntegrationTests : BaseIntegrationTest
{
    private UriBuilder BaseUri = new UriBuilder("http://localhost/api/payment-method/");

    [SetUp]
    protected override void Setup()
    {
        base.Setup();
        Authenticate();
    }

    [Test]
    public async Task Should_Create_New_PaymentMethod()
    {
        var form = new PaymentMethodForm()
        {
            Name = "Cash",
            IsActive = true,
            PaymentType = PaymentType.Cash
        };
        var content = form.BuildJsonContent();

        var result = await Client.PostAsync(BaseUri.Uri, content);

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        var payment = FindByName(form.Name).FirstOrDefault();
        Assert.That(payment.IsActive, Is.EqualTo(form.IsActive));
        Assert.That(payment.PaymentType, Is.EqualTo(form.PaymentType));
        Assert.That(payment.UserId, Is.EqualTo(AdminUser.Id));
    }

    [Test]
    public async Task Should_Not_Create_New_PaymentMethod_With_Existing_Name()
    {
        MockDatabase();
        var form = new PaymentMethodForm()
        {
            Name = "Cash",
            IsActive = true,
            PaymentType = PaymentType.Cash
        };
        var content = form.BuildJsonContent();

        var result = await Client.PostAsync(BaseUri.Uri, content);

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var payment = FindByUserId(AdminUser.Id);
        Assert.That(payment.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Should_Update_Existing_PaymentMethod()
    {
        //Arrange
        var paymentMock = MockDatabase();
        var patch = new JsonPatchDocument<PaymentMethodForm>();
        patch.Replace(x => x.Name, "Cash2");
        var body = patch.Operations.BuildJsonContent("application/json-patch+json");

        //Act
        var result = await Client.PatchAsync(BaseUri + $"{paymentMock.Id}", body);

        //Assert
        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
        var updatedPayment = FindByName("Cash2").FirstOrDefault();
        Assert.That(updatedPayment.IsActive, Is.EqualTo(paymentMock.IsActive));
        Assert.That(updatedPayment.PaymentType, Is.EqualTo(paymentMock.PaymentType));
        Assert.That(updatedPayment.Name, Is.EqualTo("Cash2"));
    }

    [Test]
    public async Task Should_Not_Update_Existing_PaymentMethod_With_Invalid_UserId()
    {
        //Arrange
        var paymentMock = MockDatabase();
        var patch = new JsonPatchDocument<PaymentMethodForm>();
        patch.Replace(x => x.Name, "Cash2");
        var body = patch.Operations.BuildJsonContent("application/json-patch+json");
        MockUser("not_payment_user", "test");
        Authenticate("not_payment_user");

        //Act
        var result = await Client.PatchAsync(BaseUri + $"{paymentMock.Id}", body);

        //Assert
        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        //Asserting that registry did not changed
        var updatedPayment = FindByName(paymentMock.Name).FirstOrDefault();
        Assert.That(updatedPayment.IsActive, Is.EqualTo(paymentMock.IsActive));
        Assert.That(updatedPayment.PaymentType, Is.EqualTo(paymentMock.PaymentType));
        Assert.That(updatedPayment.Name, Is.EqualTo(paymentMock.Name));
    }

    [Test]
    public async Task Should_Fetch_Users_PaymentMethod()
    {
        //Arrange
        var paymentMock = MockDatabase();

        //Act
        var result = await Client.GetAsync(BaseUri + $"{paymentMock.Id}");

        //Assert
        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetchedContent = (await result.Content.ReadAsStringAsync()).Deserialize<PaymentMethodResponse>();
        Assert.That(fetchedContent.IsActive, Is.EqualTo(paymentMock.IsActive));
        Assert.That(fetchedContent.PaymentType, Is.EqualTo(paymentMock.PaymentType));
        Assert.That(fetchedContent.Name, Is.EqualTo(paymentMock.Name));
    }

    [Test]
    public async Task Should_Not_Fetch_Users_PaymentMethod()
    {
        //Arrange
        var paymentMock = MockDatabase();
        MockUser("not_payment_user", "test");
        Authenticate("not_payment_user");
        
        //Act
        var result = await Client.GetAsync(BaseUri + $"{paymentMock.Id}");

        //Assert
        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Should_Fetch_All_Users_PaymentMethod()
    {
        //Arrange
        MockDatabase();

        //Act
        var result = await Client.GetAsync(BaseUri.Uri);

        //Assert
        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetchedContent = (await result.Content.ReadAsStringAsync()).Deserialize<List<PaymentMethodResponse>>();
        Assert.That(fetchedContent.Count, Is.EqualTo(1));
    }

    private List<PaymentMethod> FindByName(string name)
    {
        return Factory.DbContext.PaymentMethods.Find(Builders<PaymentMethod>.Filter.Eq(x => x.Name, name)).ToList();
    }

    private List<PaymentMethod> FindByUserId(Guid userId)
    {
        return Factory.DbContext.PaymentMethods.Find(Builders<PaymentMethod>.Filter.Eq(x => x.UserId, userId)).ToList();
    }

    private PaymentMethod MockDatabase()
    {
        var entity = new PaymentMethod("Cash", PaymentType.Cash);
        entity.BindUser(AdminUser.Id);

        Factory.DbContext.PaymentMethods.InsertOne(entity);
        return FindByName(entity.Name).FirstOrDefault();
    }
}