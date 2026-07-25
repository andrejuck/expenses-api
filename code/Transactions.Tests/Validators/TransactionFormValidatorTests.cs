using FluentValidation.TestHelper;
using NUnit.Framework;
using Transactions.Api.PresentationContracts.Expenses;
using Transactions.Api.Validators;
using Transactions.Domain.Models.Enum;

namespace Transactions.Tests.Validators;

/// <summary>
/// Covers Documentation/[Feature] Transactions Manager/BDD - Acceptance Criteria/AC - Transaction Manager Form.md
/// field-level rules.
/// </summary>
[TestFixture]
public class TransactionFormValidatorTests
{
    private TransactionFormValidator _validator = null!;

    [SetUp]
    public void SetUp() => _validator = new TransactionFormValidator();

    private static TransactionForm BuildForm(
        string description = "Mercado do mês",
        decimal totalPrice = 100,
        Guid? paymentMethodId = null,
        List<string>? expenseCategories = null) => new()
    {
        Description = description,
        TotalPrice = totalPrice,
        TransactionDate = DateTime.Today,
        PaymentMethodId = paymentMethodId ?? Guid.NewGuid(),
        TransactionType = TransactionType.Expense,
        ExpenseCategories = expenseCategories
    };

    [Test]
    public void Given_a_blank_description_When_validating_Then_it_fails()
    {
        var result = _validator.TestValidate(BuildForm(description: string.Empty));

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Given_a_description_shorter_than_5_characters_When_validating_Then_it_fails()
    {
        var result = _validator.TestValidate(BuildForm(description: "Ab"));

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Given_a_negative_price_When_validating_Then_it_fails()
    {
        var result = _validator.TestValidate(BuildForm(totalPrice: -10));

        result.ShouldHaveValidationErrorFor(x => x.TotalPrice);
    }

    [Test]
    public void Given_a_price_of_zero_When_validating_Then_it_fails()
    {
        var result = _validator.TestValidate(BuildForm(totalPrice: 0));

        result.ShouldHaveValidationErrorFor(x => x.TotalPrice);
    }

    [Test]
    public void Given_an_empty_payment_method_id_When_validating_Then_it_fails()
    {
        var result = _validator.TestValidate(BuildForm(paymentMethodId: Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.PaymentMethodId);
    }

    [Test]
    public void Given_a_category_with_less_than_3_characters_When_validating_Then_it_fails()
    {
        var result = _validator.TestValidate(BuildForm(expenseCategories: ["ab"]));

        result.ShouldHaveValidationErrorFor("ExpenseCategories[0]");
    }

    [Test]
    public void Given_a_valid_form_When_validating_Then_it_passes()
    {
        var result = _validator.TestValidate(BuildForm());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
