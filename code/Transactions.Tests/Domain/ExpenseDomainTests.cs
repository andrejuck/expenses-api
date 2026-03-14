using Transactions.Domain.Models;
using Transactions.Domain.Models.Enum;
using Libs.Api.ErrorHandling.Exceptions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Tests.Domain;

[TestFixture]
public class ExpenseDomainTests
{

    [TestCase(3, 10.00)]
    [TestCase(5, 15.00)]
    [TestCase(3, 10.20)]
    [TestCase(1, 10.133)]
    public void Should_Divide_Expense_By_Installments(int installment, decimal totalPrice)
    {
        var paymentMethod = new PaymentMethod("credit card", PaymentType.CreditCard, true);
        var originalExpense = new Transaction(null, "Test description", totalPrice, DateTime.Now, null, paymentMethod, TransactionType.Expense, installment);

        var expenses = originalExpense.DivideByInstallments(paymentMethod);

        Assert.That(expenses.Count, Is.EqualTo(installment));
        Assert.That(expenses.Sum(x => x.TotalPrice), Is.EqualTo(Math.Round(totalPrice, 2)));
    }

    [Test]
    public void Should_Not_Divide_Expense_When_Installments_Equal_Zero()
    {
        var paymentMethod = new PaymentMethod("credit card", PaymentType.CreditCard, true);
        var originalExpense = new Transaction(null, "Test description", 10, DateTime.Now, null, paymentMethod, TransactionType.Expense);

        try
        {
            var expenses = originalExpense.DivideByInstallments(paymentMethod);
        }
        catch (DomainException ex)
        {
            Assert.That(ex.Message, Is.EqualTo("Number of Installments should be higher than zero."));
        }
    }

    [TestCase(PaymentType.DebitCard)]
    [TestCase(PaymentType.Cash)]
    public void Should_Not_Divide_Expense_When_NotCreditCard(PaymentType paymentType)
    {
        var paymentMethod = new PaymentMethod("credit card", paymentType, true);
        var originalExpense = new Transaction(null, "Test description", 10, DateTime.Now, null, paymentMethod, TransactionType.Expense);

        try
        {
            var expenses = originalExpense.DivideByInstallments(paymentMethod);
        }
        catch (DomainException ex)
        {
            Assert.That(ex.Message, Is.EqualTo("Expense installments is allowed to credit cards only."));
        }
    }

    [Test]
    public void Should_Validate_Expense_With_Incorret_PaymentType_For_Installment()
    {
        var paymentMethod = new PaymentMethod("credit card", PaymentType.Cash, true);

        try
        {
            var originalExpense = new Transaction(null, "Test description", 10, DateTime.Now, null, paymentMethod, TransactionType.Expense, 1);
        }
        catch (DomainException ex)
        {
            Assert.That(ex.Message, Is.EqualTo("Expense installments is allowed to credit cards only."));
        }
    }
}