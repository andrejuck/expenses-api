using Libs.Api.ErrorHandling.Exceptions;
using NUnit.Framework;
using Transactions.Domain.Models;
using Transactions.Domain.Models.Enum;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Tests.Domain;

/// <summary>
/// Covers Documentation/[Feature] Transactions Manager/BDD - Acceptance Criteria/AC - Transaction Manager Form.md:
/// price must be positive, description is required and instalments are only allowed for credit card payments.
/// </summary>
[TestFixture]
public class TransactionTests
{
    private static PaymentMethod CreditCard() => new("Cartão", PaymentType.CreditCard, false);
    private static PaymentMethod Cash() => new("Dinheiro", PaymentType.Cash, false);

    [Test]
    public void Given_a_price_of_zero_or_less_When_creating_a_transaction_Then_a_domain_exception_is_thrown()
    {
        Assert.Throws<DomainException>(() =>
            new Transaction(null, "Mercado", 0, DateTime.Today, null, CreditCard(), TransactionType.Expense, null));
    }

    [Test]
    public void Given_a_blank_description_When_creating_a_transaction_Then_a_domain_exception_is_thrown()
    {
        Assert.Throws<DomainException>(() =>
            new Transaction(null, string.Empty, 100, DateTime.Today, null, CreditCard(), TransactionType.Expense, null));
    }

    [Test]
    public void Given_a_non_credit_card_payment_When_installments_are_informed_Then_a_domain_exception_is_thrown()
    {
        Assert.Throws<DomainException>(() =>
            new Transaction(null, "Mercado", 100, DateTime.Today, null, Cash(), TransactionType.Expense, null, 2));
    }

    [Test]
    public void Given_a_credit_card_payment_When_dividing_by_installments_Then_it_generates_the_expected_number_of_transactions_with_indexed_description()
    {
        var paymentMethod = CreditCard();
        var transaction = new Transaction(null, "Mercado", 100, DateTime.Today, null, paymentMethod, TransactionType.Expense, null, 2);

        var installments = transaction.DivideByInstallments(paymentMethod);

        Assert.That(installments, Has.Count.EqualTo(2));
        Assert.That(installments[0].Description, Is.EqualTo("Mercado 1|2"));
        Assert.That(installments[1].Description, Is.EqualTo("Mercado 2|2"));
    }

    [Test]
    public void Given_a_total_price_that_does_not_divide_evenly_When_dividing_by_installments_Then_the_remainder_is_added_to_the_last_installment()
    {
        var paymentMethod = CreditCard();
        var transaction = new Transaction(null, "Mercado", 100, DateTime.Today, null, paymentMethod, TransactionType.Expense, null, 3);

        var installments = transaction.DivideByInstallments(paymentMethod);

        Assert.That(installments.Sum(x => x.TotalPrice), Is.EqualTo(100));
        Assert.That(installments[0].TotalPrice, Is.EqualTo(installments[1].TotalPrice));
        Assert.That(installments[2].TotalPrice, Is.GreaterThanOrEqualTo(installments[0].TotalPrice));
    }

    [Test]
    public void Given_a_non_credit_card_payment_When_dividing_by_installments_Then_a_domain_exception_is_thrown()
    {
        var creditCardTransaction = new Transaction(null, "Mercado", 100, DateTime.Today, null, CreditCard(), TransactionType.Expense, null, 2);

        Assert.Throws<DomainException>(() => creditCardTransaction.DivideByInstallments(Cash()));
    }

    [Test]
    public void Given_no_installment_quantity_When_dividing_by_installments_Then_a_domain_exception_is_thrown()
    {
        var paymentMethod = CreditCard();
        var transaction = new Transaction(null, "Mercado", 100, DateTime.Today, null, paymentMethod, TransactionType.Expense, null);

        Assert.Throws<DomainException>(() => transaction.DivideByInstallments(paymentMethod));
    }

    [Test]
    public void Given_a_price_with_more_than_2_decimal_places_When_creating_a_transaction_Then_it_is_rounded_to_2_decimal_places()
    {
        var transaction = new Transaction(null, "Mercado", 10.567m, DateTime.Today, null, CreditCard(), TransactionType.Expense, null);

        Assert.That(transaction.TotalPrice, Is.EqualTo(10.57m));
    }
}
