using Libs.Api.ErrorHandling.Exceptions;
using NUnit.Framework;
using Transactions.Domain.Models.Families;

namespace Transactions.Tests.Domain;

/// <summary>
/// Covers the domain invariants behind Documentation/[Feature] Family/BDD - Acceptance Criteria/AC - Family Form.md:
/// a family must have between 1 and 5 members and at least one linked account.
/// </summary>
[TestFixture]
public class FamilyTests
{
    [Test]
    public void Given_no_members_When_creating_a_family_Then_a_domain_exception_is_thrown()
    {
        Assert.Throws<DomainException>(() =>
            new Family("My Family", Guid.NewGuid(), "Owner", [], [Guid.NewGuid()]));
    }

    [Test]
    public void Given_more_than_5_members_When_creating_a_family_Then_a_domain_exception_is_thrown()
    {
        var sixMembers = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid());

        Assert.Throws<DomainException>(() =>
            new Family("My Family", Guid.NewGuid(), "Owner", sixMembers, [Guid.NewGuid()]));
    }

    [Test]
    public void Given_no_accounts_When_creating_a_family_Then_a_domain_exception_is_thrown()
    {
        Assert.Throws<DomainException>(() =>
            new Family("My Family", Guid.NewGuid(), "Owner", [Guid.NewGuid()], []));
    }

    [Test]
    public void Given_between_1_and_5_members_and_at_least_1_account_When_creating_a_family_Then_it_is_created_successfully()
    {
        var family = new Family("My Family", Guid.NewGuid(), "Owner", [Guid.NewGuid()], [Guid.NewGuid()]);

        Assert.That(family.Name, Is.EqualTo("My Family"));
    }

    [Test]
    public void Given_an_existing_family_When_updating_with_no_members_Then_a_domain_exception_is_thrown()
    {
        var family = new Family("My Family", Guid.NewGuid(), "Owner", [Guid.NewGuid()], [Guid.NewGuid()]);

        Assert.Throws<DomainException>(() =>
            family.PrepareToUpdate("My Family", [], [Guid.NewGuid()]));
    }

    [Test]
    public void Given_an_existing_family_When_updating_with_valid_data_Then_name_members_and_accounts_are_replaced()
    {
        var family = new Family("My Family", Guid.NewGuid(), "Owner", [Guid.NewGuid()], [Guid.NewGuid()]);
        var newMembers = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var newAccounts = new[] { Guid.NewGuid() };

        family.PrepareToUpdate("Renamed Family", newMembers, newAccounts);

        Assert.That(family.Name, Is.EqualTo("Renamed Family"));
        Assert.That(family.Members, Is.EqualTo(newMembers));
        Assert.That(family.Accounts, Is.EqualTo(newAccounts));
    }
}
