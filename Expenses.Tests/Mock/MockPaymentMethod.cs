using Expenses.Api.PresentationContracts;
using Expenses.Domain.Models;
using Expenses.Domain.Models.Enum;
using MongoDB.Driver;

namespace Expenses.Tests.Mock;

public static class MockPaymentMethod
{
    public static PaymentMethod CreatePaymentMethod(
        IMongoCollection<PaymentMethod> collection,
        UserResponse loggedUser,
        PaymentType paymentType = PaymentType.Cash
    )
    {
        var paymentMethod = new PaymentMethod("TestPayment", paymentType);
        paymentMethod.BindUser(loggedUser.Id);

        collection.InsertOne(paymentMethod);

        return paymentMethod;
    }
}