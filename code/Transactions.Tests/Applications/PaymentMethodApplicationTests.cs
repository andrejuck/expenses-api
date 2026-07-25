using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Transactions.Api.Adapters;
using Transactions.Api.Application;
using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models;
using Transactions.Domain.Models.Enum;
using Transactions.Domain.Models.PaymentMethod;
using Transactions.Tests.TestHelpers;

namespace Transactions.Tests.Applications;

/// <summary>
/// Covers Documentation/[Feature] Transactions Manager/BDD - Acceptance Criteria/AC - Payment Method Page.md.
/// </summary>
[TestFixture]
public class PaymentMethodApplicationTests
{
    private IPaymentMethodRepository _repository = null!;
    private IPaymentMethodAdapter _adapter = null!;
    private Libs.Api.ErrorHandling.IErrorService _errorService = null!;
    private PaymentMethodApplication _sut = null!;

    private readonly Guid _userId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IPaymentMethodRepository>();
        _adapter = new PaymentMethodAdapter();
        _errorService = ErrorServiceTestFactory.CreateStateful();

        _sut = new PaymentMethodApplication(
            _repository,
            _adapter,
            _errorService,
            Substitute.For<ILogger<PaymentMethodApplication>>());
    }

    private static PaymentMethodForm ValidForm() => new()
    {
        Name = "Cartão Nubank",
        PaymentType = PaymentType.CreditCard,
        IsActive = true,
        IsDefault = false
    };

    [Test]
    public async Task Given_a_unique_name_for_the_user_When_creating_a_payment_method_Then_it_is_created_and_bound_to_the_user()
    {
        _repository.FindByNameAsync("Cartão Nubank", _userId).Returns((PaymentMethod?)null);

        await _sut.CreateNewPaymentMethodAsync(_userId, ValidForm());

        await _repository.Received(1).AddAsync(Arg.Is<PaymentMethod>(p => p.UserId == _userId && p.Name == "Cartão Nubank"));
    }

    [Test]
    public async Task Given_a_name_already_registered_by_the_user_When_creating_a_payment_method_Then_creation_is_rejected()
    {
        _repository.FindByNameAsync("Cartão Nubank", _userId).Returns(new PaymentMethod("Cartão Nubank", PaymentType.CreditCard, false));

        await _sut.CreateNewPaymentMethodAsync(_userId, ValidForm());

        await _repository.DidNotReceive().AddAsync(Arg.Any<PaymentMethod>());
    }

    [Test]
    public async Task Given_the_current_users_registered_cards_When_fetching_by_user_Then_only_that_users_cards_are_returned()
    {
        var ownCards = new List<PaymentMethod> { new("Cartão Nubank", PaymentType.CreditCard, false) };
        _repository.FindAllByUserIdAsync(_userId).Returns(ownCards);

        var result = await _sut.FetchByUserAsync(_userId);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Cartão Nubank"));
    }

    [Test]
    public async Task Given_a_payment_method_that_does_not_exist_When_updating_Then_nothing_happens()
    {
        _repository.FindByIdAsync(Arg.Any<Guid>(), _userId).Returns((PaymentMethod?)null);
        var patch = new JsonPatchDocument<PaymentMethodForm>();

        await _sut.UpdatePaymentMethodAsync(Guid.NewGuid(), _userId, patch);

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<PaymentMethod>());
    }

    [Test]
    public async Task Given_an_own_payment_method_When_updating_its_name_Then_the_change_is_persisted()
    {
        var existing = new PaymentMethod("Cartão Antigo", PaymentType.CreditCard, false);
        _repository.FindByIdAsync(existing.Id, _userId).Returns(existing);

        var patch = new JsonPatchDocument<PaymentMethodForm>();
        patch.Replace(x => x.Name, "Cartão Renomeado");

        await _sut.UpdatePaymentMethodAsync(existing.Id, _userId, patch);

        Assert.That(existing.Name, Is.EqualTo("Cartão Renomeado"));
        await _repository.Received(1).UpdateAsync(existing);
    }
}
