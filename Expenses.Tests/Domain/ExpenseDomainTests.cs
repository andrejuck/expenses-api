using Expenses.Domain.Exceptions;
using Expenses.Domain.Models;
using Expenses.Domain.Models.Enum;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Expenses.Tests.Domain;

[TestFixture]
public class ExpenseDomainTests
{

    [TestCase(3, 10.00)]
    [TestCase(5, 15.00)]
    [TestCase(3, 10.20)]
    [TestCase(1, 10.133)]
    public void Should_Divide_Expense_By_Installments(int installment, decimal totalPrice)
    {
        var paymentMethod = new PaymentMethod("credit card", PaymentType.CreditCard);
        var originalExpense = new Expense(null, "Test description", totalPrice, DateTime.Now, null, paymentMethod, installment);

        var expenses = originalExpense.DivideByInstallments(paymentMethod);

        Assert.That(expenses.Count, Is.EqualTo(installment));
        Assert.That(expenses.Sum(x => x.TotalPrice), Is.EqualTo(Math.Round(totalPrice, 2)));
    }

    [Test]
    public void Should_Not_Divide_Expense_When_Installments_Equal_Zero()
    {
        var paymentMethod = new PaymentMethod("credit card", PaymentType.CreditCard);
        var originalExpense = new Expense(null, "Test description", 10, DateTime.Now, null, paymentMethod);

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
        var paymentMethod = new PaymentMethod("credit card", paymentType);
        var originalExpense = new Expense(null, "Test description", 10, DateTime.Now, null, paymentMethod);

        try
        {
            var expenses = originalExpense.DivideByInstallments(paymentMethod);
        }
        catch (DomainException ex)
        {
            Assert.That(ex.Message, Is.EqualTo("To create installments, payment should be on credit card."));
        }
    }

    [Test]
    public void Should_Validate_Expense_With_Incorret_PaymentType_For_Installment()
    {
        var paymentMethod = new PaymentMethod("credit card", PaymentType.Cash);

        try
        {
            var originalExpense = new Expense(null, "Test description", 10, DateTime.Now, null, paymentMethod, 1);
        }
        catch (DomainException ex)
        {
            Assert.That(ex.Message, Is.EqualTo("Expense installments is allowed to credit cards only."));
        }
    }
}