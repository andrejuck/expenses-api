using AutoMapper;
using Libs.Api.Adapters;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Transactions.Api.Application;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.PresentationContracts.Expenses;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Enum;
using Transactions.Domain.Models.Transaction;
using Transactions.Tests.TestHelpers;

namespace Transactions.Tests.Applications;

/// <summary>
/// Covers Documentation/[Feature] Transactions Manager/BDD - Acceptance Criteria/AC - Transaction Manager Form.md
/// and AC - Transaction Manager Page.md.
/// </summary>
[TestFixture]
public class TransactionApplicationTests
{
    private IExpenseRepository _repository = null!;
    private IPaymentMethodRepository _paymentRepository = null!;
    private IAccountApplication _accountApplication = null!;
    private Libs.Api.ErrorHandling.IErrorService _errorService = null!;
    private TransactionApplication _sut = null!;

    private readonly Guid _userId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IExpenseRepository>();
        _paymentRepository = Substitute.For<IPaymentMethodRepository>();
        _accountApplication = Substitute.For<IAccountApplication>();
        _errorService = ErrorServiceTestFactory.CreateStateful();

        _sut = new TransactionApplication(
            _repository,
            Substitute.For<IMapper>(),
            _paymentRepository,
            _accountApplication,
            _errorService,
            Substitute.For<IPaginationAdapter>(),
            Substitute.For<ILogger<TransactionApplication>>());
    }

    private static PaymentMethod CreditCard() => new("Cartão", PaymentType.CreditCard, false);

    private static TransactionForm ValidForm(Guid paymentMethodId, Guid? accountId = null) => new()
    {
        Description = "Mercado do mês",
        TotalPrice = 150,
        TransactionDate = DateTime.Today,
        PaymentMethodId = paymentMethodId,
        TransactionType = TransactionType.Expense,
        AccountId = accountId
    };

    [Test]
    public async Task Given_a_payment_method_that_does_not_exist_When_creating_a_transaction_Then_it_is_not_created()
    {
        var paymentMethodId = Guid.NewGuid();
        _paymentRepository.FindByIdAsync(paymentMethodId, _userId).Returns((PaymentMethod?)null);

        await _sut.CreateNewTransactionAsync(_userId, ValidForm(paymentMethodId));

        await _repository.DidNotReceive().AddAsync(Arg.Any<Transaction>());
    }

    [Test]
    public async Task Given_an_account_id_that_does_not_exist_When_creating_a_transaction_Then_it_is_not_created()
    {
        var paymentMethod = CreditCard();
        var accountId = Guid.NewGuid();
        _paymentRepository.FindByIdAsync(paymentMethod.Id, _userId).Returns(paymentMethod);
        _accountApplication.FindByIdAsync(accountId, _userId).Returns((Account?)null);

        await _sut.CreateNewTransactionAsync(_userId, ValidForm(paymentMethod.Id, accountId));

        await _repository.DidNotReceive().AddAsync(Arg.Any<Transaction>());
    }

    [Test]
    public async Task Given_a_valid_form_When_creating_a_transaction_Then_it_is_persisted_and_bound_to_the_user()
    {
        var paymentMethod = CreditCard();
        _paymentRepository.FindByIdAsync(paymentMethod.Id, _userId).Returns(paymentMethod);

        await _sut.CreateNewTransactionAsync(_userId, ValidForm(paymentMethod.Id));

        await _repository.Received(1).AddAsync(Arg.Is<Transaction>(t =>
            t.Description == "Mercado do mês" && t.UserId == _userId));
    }

    [Test]
    public async Task Given_a_transaction_that_does_not_exist_When_deleting_Then_nothing_happens()
    {
        _repository.FindByIdAsync(Arg.Any<Guid>(), _userId).Returns((Transaction?)null);

        await _sut.DeleteTransactionAsync(Guid.NewGuid(), _userId);

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Transaction>());
    }

    [Test]
    public async Task Given_an_existing_transaction_When_deleting_Then_it_is_marked_as_deleted()
    {
        var paymentMethod = CreditCard();
        var transaction = new Transaction(null, "Mercado do mês", 150, DateTime.Today, null, paymentMethod, TransactionType.Expense, null);
        _repository.FindByIdAsync(transaction.Id, _userId).Returns(transaction);

        await _sut.DeleteTransactionAsync(transaction.Id, _userId);

        Assert.That(transaction.DeletedAt, Is.Not.Null);
        await _repository.Received(1).UpdateAsync(transaction);
    }

    [Test]
    public async Task Given_a_transaction_id_that_does_not_exist_When_fetching_it_Then_null_is_returned()
    {
        _repository.FindByIdAsync(Arg.Any<Guid>(), _userId).Returns((Transaction?)null);

        var result = await _sut.GetTransactionAsync(Guid.NewGuid(), _userId);

        Assert.That(result, Is.Null);
    }
}
