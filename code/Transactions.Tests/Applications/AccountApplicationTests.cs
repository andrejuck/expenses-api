using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Transactions.Api.Adapters;
using Transactions.Api.Application;
using Transactions.Api.PresentationContracts.Accounts;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Enum;
using Transactions.Tests.TestHelpers;

namespace Transactions.Tests.Applications;

/// <summary>
/// Covers Documentation/[Feature] Family/BDD - Acceptance Criteria/AC - Accounts Form.md
/// and AC - Accounts Manager Page.md.
/// </summary>
[TestFixture]
public class AccountApplicationTests
{
    private IAccountRepository _repository = null!;
    private IFamilyRepository _familyRepository = null!;
    private IExpenseRepository _expenseRepository = null!;
    private Libs.Api.ErrorHandling.IErrorService _errorService = null!;
    private AccountApplication _sut = null!;

    private readonly Guid _userId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IAccountRepository>();
        _familyRepository = Substitute.For<IFamilyRepository>();
        _expenseRepository = Substitute.For<IExpenseRepository>();
        _errorService = ErrorServiceTestFactory.CreateStateful();

        _sut = new AccountApplication(
            new AccountAdapter(),
            _repository,
            _familyRepository,
            _expenseRepository,
            _errorService,
            Substitute.For<ILogger<AccountApplication>>());
    }

    private static AccountForm ValidForm() => new()
    {
        AccountName = "Conta Pessoal",
        AccountType = AccountType.Personal,
        InitialBalance = 100
    };

    [Test]
    public async Task Given_a_unique_account_name_When_creating_an_account_Then_it_is_created()
    {
        _repository.FetchByNameAsync(Arg.Any<string>(), _userId).Returns((Account?)null);

        var result = await _sut.CreateAccountAsync(ValidForm(), _userId);

        Assert.That(result, Is.Not.Null);
        await _repository.Received(1).AddAsync(Arg.Is<Account>(a => a.Name == "Conta Pessoal"));
    }

    [Test]
    public async Task Given_an_account_name_already_used_by_the_user_When_creating_an_account_Then_creation_is_rejected()
    {
        _repository.FetchByNameAsync("Conta Pessoal", _userId).Returns(new Account("Conta Pessoal", AccountType.Personal, 0));

        var result = await _sut.CreateAccountAsync(ValidForm(), _userId);

        Assert.That(result, Is.Null);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Account>());
    }

    [Test]
    public async Task Given_an_update_form_with_initial_balance_filled_When_updating_an_account_Then_it_is_rejected()
    {
        var existing = new Account("Conta Pessoal", AccountType.Personal, 0);
        _repository.FetchByNameAsync(Arg.Any<string>(), existing.Id, _userId).Returns((Account?)null);
        _repository.FindByIdAsync(existing.Id, _userId).Returns(existing);

        var form = ValidForm();
        form.InitialBalance = 500;

        var result = await _sut.UpdateAccountAsync(existing.Id, form, _userId);

        Assert.That(result, Is.Null);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Account>());
    }

    [Test]
    public async Task Given_a_name_used_by_another_account_When_updating_an_account_Then_it_is_rejected()
    {
        var existing = new Account("Conta Pessoal", AccountType.Personal, 0);
        var conflicting = new Account("Conta Nova", AccountType.Personal, 0);
        _repository.FetchByNameAsync("Conta Nova", existing.Id, _userId).Returns(conflicting);

        var form = ValidForm();
        form.AccountName = "Conta Nova";
        form.InitialBalance = null;

        var result = await _sut.UpdateAccountAsync(existing.Id, form, _userId);

        Assert.That(result, Is.Null);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Account>());
    }

    [Test]
    public async Task Given_a_valid_update_form_without_initial_balance_When_updating_an_account_Then_it_is_updated()
    {
        var existing = new Account("Conta Pessoal", AccountType.Personal, 0);
        _repository.FetchByNameAsync(Arg.Any<string>(), existing.Id, _userId).Returns((Account?)null);
        _repository.FindByIdAsync(existing.Id, _userId).Returns(existing);

        var form = ValidForm();
        form.AccountName = "Conta Renomeada";
        form.InitialBalance = null;

        var result = await _sut.UpdateAccountAsync(existing.Id, form, _userId);

        Assert.That(result, Is.Not.Null);
        Assert.That(existing.Name, Is.EqualTo("Conta Renomeada"));
        await _repository.Received(1).UpdateAsync(existing);
    }

    [Test]
    public async Task Given_an_account_linked_to_an_expense_When_deleting_Then_deletion_is_rejected()
    {
        var existing = new Account("Conta Pessoal", AccountType.Personal, 0);
        _repository.FindByIdAsync(existing.Id, _userId).Returns(existing);
        _expenseRepository.GetAnyWithinAccountAsync(existing.Id, _userId).Returns(true);

        await _sut.DeleteByIdAsync(existing.Id, _userId);

        Assert.That(existing.DeletedAt, Is.Null);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Account>());
    }

    [Test]
    public async Task Given_an_account_with_no_linked_expenses_When_deleting_Then_it_is_marked_as_deleted()
    {
        var existing = new Account("Conta Pessoal", AccountType.Personal, 0);
        _repository.FindByIdAsync(existing.Id, _userId).Returns(existing);
        _expenseRepository.GetAnyWithinAccountAsync(existing.Id, _userId).Returns(false);

        await _sut.DeleteByIdAsync(existing.Id, _userId);

        Assert.That(existing.DeletedAt, Is.Not.Null);
        await _repository.Received(1).UpdateAsync(existing);
    }

    [Test]
    public async Task Given_personal_and_family_shared_accounts_When_fetching_all_accounts_Then_both_are_returned()
    {
        var personal = new Account("Conta Pessoal", AccountType.Personal, 0);
        var shared = new Account("Conta Compartilhada", AccountType.Shared, 0);

        _repository.GetAllUserAccountsAsync(_userId).Returns([personal]);
        _familyRepository.GetAllUserFamilyAccountsAsync(_userId).Returns([shared]);

        var result = (await _sut.FetchAllAccountsAsync(_userId)).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Select(a => a.Name), Is.EquivalentTo(new[] { "Conta Pessoal", "Conta Compartilhada" }));
    }
}
