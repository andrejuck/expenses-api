using Libs.Api.ErrorHandling.Exceptions;
using NUnit.Framework;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Enum;

namespace Transactions.Tests.Domain;

/// <summary>
/// Covers the domain invariant behind Documentation/[Feature] Family/Account.md:
/// a Personal account can never be bound to a Family.
/// </summary>
[TestFixture]
public class AccountTests
{
    [Test]
    public void Given_a_new_account_When_no_initial_balance_is_informed_Then_balance_defaults_to_zero()
    {
        var account = new Account("Conta Pessoal", AccountType.Personal, null);

        Assert.That(account.Balance, Is.EqualTo(0));
    }

    [Test]
    public void Given_a_personal_account_When_binding_it_to_a_family_Then_a_domain_exception_is_thrown()
    {
        var account = new Account("Conta Pessoal", AccountType.Personal, 100);

        Assert.Throws<DomainException>(() =>
            account.PrepareToUpdate(account.Name, AccountType.Personal, Guid.NewGuid()));
    }

    [Test]
    public void Given_a_shared_account_When_binding_it_to_a_family_Then_it_succeeds()
    {
        var account = new Account("Conta Compartilhada", AccountType.Shared, 100);
        var familyId = Guid.NewGuid();

        account.PrepareToUpdate(account.Name, AccountType.Shared, familyId);

        Assert.That(account.FamilyId, Is.EqualTo(familyId));
    }

    [Test]
    public void Given_an_account_When_deleting_it_Then_it_is_unbound_from_family_and_marked_as_deleted()
    {
        var account = new Account("Conta Compartilhada", AccountType.Shared, 100);
        account.PrepareToUpdate(account.Name, AccountType.Shared, Guid.NewGuid());

        account.PrepareToDelete();

        Assert.That(account.FamilyId, Is.Null);
        Assert.That(account.DeletedAt, Is.Not.Null);
    }
}
