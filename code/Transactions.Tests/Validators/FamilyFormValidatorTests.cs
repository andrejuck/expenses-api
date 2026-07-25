using FluentValidation.TestHelper;
using NUnit.Framework;
using Transactions.Api.PresentationContracts.Families;
using Transactions.Api.Validators;

namespace Transactions.Tests.Validators;

/// <summary>
/// Covers Documentation/[Feature] Family/BDD - Acceptance Criteria/AC - Family Form.md field-level rules.
/// </summary>
[TestFixture]
public class FamilyFormValidatorTests
{
    private FamilyFormValidator _validator = null!;

    [SetUp]
    public void SetUp() => _validator = new FamilyFormValidator();

    private static FamilyForm ValidForm() => new()
    {
        FamilyName = "My Family",
        Members = ["member@test.com"],
        Accounts = [Guid.NewGuid()]
    };

    [Test]
    public void Given_a_blank_name_When_validating_Then_it_fails()
    {
        var form = ValidForm();
        form.FamilyName = string.Empty;

        var result = _validator.TestValidate(form);

        result.ShouldHaveValidationErrorFor(x => x.FamilyName);
    }

    [Test]
    public void Given_an_empty_members_list_When_validating_Then_it_fails()
    {
        var form = ValidForm();
        form.Members = [];

        var result = _validator.TestValidate(form);

        result.ShouldHaveValidationErrorFor(x => x.Members.Count());
    }

    [Test]
    public void Given_more_than_5_members_When_validating_Then_it_fails()
    {
        var form = ValidForm();
        form.Members = Enumerable.Range(0, 6).Select(i => $"member{i}@test.com");

        var result = _validator.TestValidate(form);

        result.ShouldHaveValidationErrorFor(x => x.Members.Count());
    }

    [Test]
    public void Given_no_accounts_When_validating_Then_it_fails()
    {
        var form = ValidForm();
        form.Accounts = [];

        var result = _validator.TestValidate(form);

        result.ShouldHaveValidationErrorFor(x => x.Accounts.Count());
    }

    [Test]
    public void Given_a_valid_form_When_validating_Then_it_passes()
    {
        var result = _validator.TestValidate(ValidForm());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
