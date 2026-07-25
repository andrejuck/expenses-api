using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Transactions.Api.Adapters;
using Transactions.Api.Application;
using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.PresentationContracts.Families;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Dtos;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Enum;
using Transactions.Domain.Models.Families;
using Transactions.Tests.TestHelpers;
using Libs.Auth.Models;

namespace Transactions.Tests.Applications;

/// <summary>
/// Covers Documentation/[Feature] Family/BDD - Acceptance Criteria/AC - Family Form.md
/// and AC - Family Page.md.
/// </summary>
[TestFixture]
public class FamilyApplicationTests
{
    private IFamilyRepository _repository = null!;
    private IAccountApplication _accountApplication = null!;
    private IUserRepository _userRepository = null!;
    private Libs.Api.ErrorHandling.IErrorService _errorService = null!;
    private IFamilyAdapter _adapter = null!;
    private FamilyApplication _sut = null!;

    private readonly Guid _ownerId = Guid.NewGuid();
    private const string OwnerName = "André";

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IFamilyRepository>();
        _accountApplication = Substitute.For<IAccountApplication>();
        _userRepository = Substitute.For<IUserRepository>();
        _errorService = ErrorServiceTestFactory.CreateStateful();
        _adapter = new FamilyAdapter();

        _sut = new FamilyApplication(
            _adapter,
            _repository,
            _accountApplication,
            _userRepository,
            _errorService,
            Substitute.For<ILogger<FamilyApplication>>());
    }

    private static Account SharedAccount() => new("Conta Compartilhada", AccountType.Shared, 100);
    private static Account PersonalAccount() => new("Conta Pessoal", AccountType.Personal, 100);

    [Test]
    public async Task Given_valid_name_members_and_shared_account_When_creating_family_Then_family_is_created_and_bound_to_accounts()
    {
        var sharedAccount = SharedAccount();
        _accountApplication.FetchAccountByIdAsync(sharedAccount.Id).Returns(sharedAccount);
        _userRepository.GetByEmailAsync("member@test.com").Returns(new User("member@test.com", "Member Name"));

        var form = new FamilyForm
        {
            FamilyName = "My Family",
            Members = ["member@test.com"],
            Accounts = [sharedAccount.Id]
        };

        var result = await _sut.CreateFamilyAsync(form, _ownerId, OwnerName);

        Assert.That(result, Is.Not.Null);
        await _repository.Received(1).AddAsync(Arg.Is<Family>(f => f.Name == "My Family"));
        await _accountApplication.Received(1).BindFamilyToAccountAsync(result!.Value, Arg.Any<IEnumerable<Guid>>());
    }

    [Test]
    public async Task Given_an_account_of_type_Personal_in_the_accounts_list_When_creating_family_Then_creation_is_rejected()
    {
        var personalAccount = PersonalAccount();
        _accountApplication.FetchAccountByIdAsync(personalAccount.Id).Returns(personalAccount);

        var form = new FamilyForm
        {
            FamilyName = "My Family",
            Members = ["member@test.com"],
            Accounts = [personalAccount.Id]
        };

        var result = await _sut.CreateFamilyAsync(form, _ownerId, OwnerName);

        Assert.That(result, Is.Null);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Family>());
    }

    [Test]
    public async Task Given_an_account_id_that_does_not_exist_When_creating_family_Then_creation_is_rejected()
    {
        var missingAccountId = Guid.NewGuid();
        _accountApplication.FetchAccountByIdAsync(missingAccountId).Returns((Account?)null);

        var form = new FamilyForm
        {
            FamilyName = "My Family",
            Members = ["member@test.com"],
            Accounts = [missingAccountId]
        };

        var result = await _sut.CreateFamilyAsync(form, _ownerId, OwnerName);

        Assert.That(result, Is.Null);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Family>());
    }

    [Test]
    public async Task Given_a_mix_of_known_and_unknown_member_emails_When_creating_family_Then_only_the_known_members_are_bound()
    {
        var sharedAccount = SharedAccount();
        var knownMember = new User("member@test.com", "Member Name");
        _accountApplication.FetchAccountByIdAsync(sharedAccount.Id).Returns(sharedAccount);
        _userRepository.GetByEmailAsync("member@test.com").Returns(knownMember);
        _userRepository.GetByEmailAsync("unknown@test.com").Returns((User?)null);

        var form = new FamilyForm
        {
            FamilyName = "My Family",
            Members = ["member@test.com", "unknown@test.com"],
            Accounts = [sharedAccount.Id]
        };
        

        var result = await _sut.CreateFamilyAsync(form, _ownerId, OwnerName);

        Assert.That(result, Is.Not.Null);
        await _repository.Received(1).AddAsync(Arg.Is<Family>(f =>
            f.Members.Count() == 1 && f.Members.Single() == knownMember.Id));
    }

    [Test]
    public async Task Given_a_family_not_owned_by_the_current_user_When_updating_Then_update_is_rejected()
    {
        var otherOwnerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var sharedAccount = SharedAccount();
        var family = new Family("My Family", otherOwnerId, "Other Owner", [Guid.NewGuid()], [sharedAccount.Id]);

        _repository.FindByIdAsync(family.Id, currentUserId).Returns(family);

        var form = new FamilyForm
        {
            FamilyName = "New Name",
            Members = ["member@test.com"],
            Accounts = [sharedAccount.Id]
        };

        var result = await _sut.UpdateFamilyAsync(family.Id, form, currentUserId);

        Assert.That(result, Is.Null);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Family>());
    }

    [Test]
    public async Task Given_the_owner_user_When_updating_family_Then_family_is_updated_and_rebound_to_accounts()
    {
        var sharedAccount = SharedAccount();
        var family = new Family("My Family", _ownerId, OwnerName, [Guid.NewGuid()], [sharedAccount.Id]);

        _repository.FindByIdAsync(family.Id, _ownerId).Returns(family);
        _accountApplication.FetchAccountByIdAsync(sharedAccount.Id).Returns(sharedAccount);
        _userRepository.GetByEmailAsync("member@test.com").Returns(new User("member@test.com", "Member Name"));

        var form = new FamilyForm
        {
            FamilyName = "Renamed Family",
            Members = ["member@test.com"],
            Accounts = [sharedAccount.Id]
        };

        var result = await _sut.UpdateFamilyAsync(family.Id, form, _ownerId);

        Assert.That(result, Is.EqualTo(family.Id));
        Assert.That(family.Name, Is.EqualTo("Renamed Family"));
        await _repository.Received(1).UpdateAsync(family);
        await _accountApplication.Received(1).BindFamilyToAccountAsync(family.Id, family.Accounts);
    }

    [Test]
    public async Task Given_a_family_that_does_not_exist_When_deleting_Then_nothing_happens()
    {
        _repository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((Family?)null);

        await _sut.DeleteByIdAsync(Guid.NewGuid(), _ownerId);

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Family>());
    }

    [Test]
    public async Task Given_an_existing_family_When_deleting_Then_it_is_marked_as_deleted()
    {
        var family = new Family("My Family", _ownerId, OwnerName, [Guid.NewGuid()], [Guid.NewGuid()]);
        _repository.FindByIdAsync(family.Id, _ownerId).Returns(family);

        await _sut.DeleteByIdAsync(family.Id, _ownerId);

        Assert.That(family.DeletedAt, Is.Not.Null);
        await _repository.Received(1).UpdateAsync(family);
    }

    [Test]
    public async Task Given_a_user_with_families_When_fetching_user_families_Then_the_page_lists_name_owner_members_and_accounts()
    {
        var dto = new FamilyDto
        {
            Id = Guid.NewGuid(),
            FamilyName = "My Family",
            OwnerUserName = OwnerName,
            Members = [],
            Accounts = []
        };
        _repository.FetchAllUserFamiliesAsync(_ownerId).Returns([dto]);

        var result = await _sut.FetchUserFamiliesAsync(_ownerId);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].FamilyName, Is.EqualTo("My Family"));
        Assert.That(result[0].OwnerName, Is.EqualTo(OwnerName));
    }
}
