using FluentValidation.TestHelper;
using NUnit.Framework;
using Transactions.Api.PresentationContracts.Accounts;
using Transactions.Api.Validators;
using Transactions.Domain.Models.Enum;

namespace Transactions.Tests.Validators;

/// <summary>
/// Covers Documentation/[Feature] Family/BDD - Acceptance Criteria/AC - Accounts Form.md field-level rules.
/// </summary>
[TestFixture]
public class AccountFormValidatorTests
{
    private AccountFormValidator _validator = null!;

    [SetUp]
    public void SetUp() => _validator = new AccountFormValidator();

    [Test]
    public void Given_a_blank_account_name_When_validating_Then_it_fails()
    {
        var form = new AccountForm { AccountName = string.Empty, AccountType = AccountType.Personal };

        var result = _validator.TestValidate(form);

        result.ShouldHaveValidationErrorFor(x => x.AccountName);
    }

    [Test]
    public void Given_an_account_name_shorter_than_3_characters_When_validating_Then_it_fails()
    {
        var form = new AccountForm { AccountName = "AB", AccountType = AccountType.Personal };

        var result = _validator.TestValidate(form);

        result.ShouldHaveValidationErrorFor(x => x.AccountName);
    }

    [Test]
    public void Given_a_valid_account_name_When_validating_Then_it_passes()
    {
        var form = new AccountForm { AccountName = "Conta Pessoal", AccountType = AccountType.Personal };

        var result = _validator.TestValidate(form);

        result.ShouldNotHaveValidationErrorFor(x => x.AccountName);
    }
}
