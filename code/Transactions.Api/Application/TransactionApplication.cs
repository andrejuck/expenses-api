using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.Helpers;
using Transactions.Api.PresentationContracts.Expenses;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models;
using Libs.Api.Adapters;
using Libs.Api.ErrorHandling;
using Libs.Api.Models;
using Microsoft.AspNetCore.JsonPatch;
using System.Net;
using System.Text.Json;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.PaymentMethod;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Api.Application;

public class TransactionApplication : ITransactionApplication
{
    private readonly IExpenseRepository _repository;
    private readonly IPaymentMethodRepository _paymentRepo;
    private readonly IAccountApplication _accountApplication;
    private readonly ITransactionAdapter _adapter;
    private readonly IErrorService _errorService;
    private readonly IPaginationAdapter _pageAdapter;
    private readonly ILogger<TransactionApplication> _logger;

    public TransactionApplication(
        IExpenseRepository repository,
        ITransactionAdapter adapter,
        IPaymentMethodRepository paymentRepo,
        IAccountApplication accountApplication,
        IErrorService errorService,
        IPaginationAdapter pageAdapter,
        ILogger<TransactionApplication> logger
        )
    {
        _repository = repository;
        _adapter = adapter;
        _paymentRepo = paymentRepo;
        _accountApplication = accountApplication;
        _errorService = errorService;
        _pageAdapter = pageAdapter;
        _logger = logger;
    }

    public async Task CreateNewTransactionAsync(Guid userId, TransactionForm form)
    {
        var paymentMethod = await FindPaymentBydId(form.PaymentMethodId, userId);
        if (paymentMethod == null) return;

        var entity = _adapter.ConvertToDomain(form, paymentMethod);
        entity.BindUser(userId);

        await _repository.AddAsync(entity);
        _logger.LogInformation(Messages.LOG_CREATED_MESSAGE, nameof(Transaction), userId, JsonSerializer.Serialize(entity));
    }

    public async Task<PagedResponse<TransactionResponse>> GetPagedTransactionAsync(TransactionSearchParam searchParams, PagedRequest pagedRequest, Guid userId)
    {
        var expensesResponse = await _repository.GetAllPagedAsync<TransactionResponse>(searchParams, pagedRequest, userId);
        var total = await _repository.GetAllCountAsync(searchParams, userId);
        var pagedResponse = _pageAdapter.ConvertToResponse(pagedRequest, total, expensesResponse);

        _logger.LogInformation(Messages.LOG_GET_PAGED_MULTIPLE_MESSAGE, pagedResponse.Result.Count(), nameof(TransactionResponse), total, userId);
        return pagedResponse;
    }

    public async Task<PagedResponse<GroupedExpensesResponse>> GetGroupedPagedExpenseAsync(TransactionSearchParam searchParams, PagedRequest pagedRequest, Guid userId)
    {
        var expensesResponse = await _repository.GetAllGroupedPagedAsync<GroupedExpensesResponse>(searchParams, pagedRequest, userId);
        var total = await _repository.GetAllCountAsync(searchParams, userId);
        var pagedResponse = _pageAdapter.ConvertToResponse(pagedRequest, total, expensesResponse);

        return pagedResponse;
    }

    public async Task<List<string>> GetUserCategoriesAsync(Guid userId)
    {
        var result = await _repository.GetAllUserCategories(userId);
        _logger.LogInformation(Messages.LOG_GET_PAGED_MULTIPLE_MESSAGE, result.Count, nameof(TransactionResponse), result.Count, userId);
        return result;
    }

    public async Task UpdateTransactionAsync(Guid id, Guid userId, JsonPatchDocument<TransactionForm> patchForm)
    {
        var expense = await FindByIdAsync(id, userId);
        if (expense is null) return;

        var entityForm = _adapter.ConvertToForm(expense);

        patchForm.ApplyTo(entityForm);
        var paymentMethod = await FindPaymentBydId(entityForm.PaymentMethodId, userId);
        if (paymentMethod == null) return;

        expense.PrepareToUpdate(entityForm.Location,
            entityForm.Description,
            entityForm.TotalPrice,
            entityForm.TransactionDate,
            entityForm.ExpenseCategories,
            paymentMethod,
            entityForm.Installment);

        await _repository.UpdateAsync(expense);
        _logger.LogInformation(Messages.LOG_UPDATED_MESSAGE, nameof(Transaction), userId, JsonSerializer.Serialize(expense));
    }

    public async Task<TransactionResponse?> GetTransactionAsync(Guid id, Guid userId)
    {
        var expense = await FindByIdAsync(id, userId);
        if (expense is null) return null;

        var result = _adapter.ConvertToResponse(expense);
        _logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(TransactionResponse), userId, JsonSerializer.Serialize(result));
        return result;
    }

    public async Task DeleteTransactionAsync(Guid id, Guid userId)
    {
        var expense = await FindByIdAsync(id, userId);
        if (expense is null) return;

        expense.SetDeletedAt();
        await _repository.UpdateAsync(expense);
        _logger.LogInformation(Messages.LOG_DELETED_MESSAGE, nameof(expense), userId, JsonSerializer.Serialize(expense));
    }

    public async Task<List<TransactionFileResponse>> GetAllExpensesAsync(TransactionSearchParam searchParams, Guid userId)
    {
        var result = await _repository.GetAllAsync<TransactionFileResponse>(searchParams, userId);
        _logger.LogInformation(Messages.LOG_GET_PAGED_MULTIPLE_MESSAGE, result.Count, nameof(TransactionResponse), result.Count, userId);
        return result;
    }

    private async Task<PaymentMethod?> FindPaymentBydId(Guid paymentId, Guid userId)
    {
        var paymentMethod = await _paymentRepo.FindByIdAsync(paymentId, userId);
        if (paymentMethod is null)
        {
            _errorService.AddError(
                nameof(FindPaymentBydId),
                string.Format(Messages.NOT_FOUND_MESSAGE_PATTERN, nameof(PaymentMethod), nameof(PaymentMethod.Id), paymentId),
                HttpStatusCode.NotFound
            );

            return null;
        }

        _logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(PaymentMethod), userId, JsonSerializer.Serialize(paymentMethod));
        return paymentMethod;
    }

    private async Task<Transaction?> FindByIdAsync(Guid id, Guid userId)
    {
        var expense = await _repository.FindByIdAsync(id, userId);
        if (expense is null)
        {
            _errorService.AddError(
                nameof(GetTransactionAsync),
                string.Format(Messages.NOT_FOUND_MESSAGE_PATTERN, nameof(Transaction), nameof(Transaction.Id), id),
                HttpStatusCode.NotFound
            );

            return null;
        }

        _logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(Transaction), userId, JsonSerializer.Serialize(expense));
        return expense;
    }
}
