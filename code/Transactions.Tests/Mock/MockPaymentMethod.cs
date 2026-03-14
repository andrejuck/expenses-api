using Transactions.Api.PresentationContracts;
using Transactions.Domain.Models;
using Transactions.Domain.Models.Enum;
using MongoDB.Driver;

namespace Transactions.Tests.Mock;

public static class MockPaymentMethod
{
    public static PaymentMethod CreatePaymentMethod(
        IMongoCollection<PaymentMethod> collection,
        UserResponse loggedUser,
        PaymentType paymentType = PaymentType.Cash
    )
    {
        var paymentMethod = new PaymentMethod("TestPayment", paymentType, true);
        paymentMethod.BindUser(loggedUser.Id);

        collection.InsertOne(paymentMethod);

        return paymentMethod;
    }
}