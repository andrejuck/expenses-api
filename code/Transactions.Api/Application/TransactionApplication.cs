using AutoMapper;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.Helpers;
using Transactions.Api.PresentationContracts.Expenses;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models;
using Libs.Api.Adapters;
using Libs.Api.ErrorHandling;
using Libs.Api.Models;
using Microsoft.AspNetCore.JsonPatch;
using MongoDB.Bson;
using System.Net;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Api.Application;

public class TransactionApplication : ITransactionApplication
{
    private readonly IExpenseRepository _repository;
    private readonly IPaymentMethodRepository _paymentRepo;
    private readonly IAccountApplication _accountApplication;
    private readonly IMapper _mapper;
    private readonly IErrorService _errorService;
    private readonly IPaginationAdapter _pageAdapter;
    private readonly ILogger<TransactionApplication> _logger;

    public TransactionApplication(
        IExpenseRepository repository,
        IMapper mapper,
        IPaymentMethodRepository paymentRepo,
        IAccountApplication accountApplication,
        IErrorService errorService,
        IPaginationAdapter pageAdapter,
        ILogger<TransactionApplication> logger 
        )
    {
        _repository = repository;
        _mapper = mapper;
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

        Account? account = null;
        if (form.AccountId is not null)
        {
            account = await _accountApplication.FindByIdAsync(form.AccountId.Value, userId);

            if (account is null) return;
        }

        //TODO - Adapter
        var entity = new Transaction(form.Location,
            form.Description,
            form.TotalPrice,
            form.TransactionDate,
            form.ExpenseCategories,
            paymentMethod,
            form.TransactionType,
            account?.Id,
            form.Installment);

        entity.BindUser(userId);

        await _repository.AddAsync(entity);
        _logger.LogInformation(Messages.LOG_CREATED_MESSAGE, nameof(Transaction), userId, entity.ToJson());
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

        var entityForm = _mapper.Map<TransactionForm>(expense);

        patchForm.ApplyTo(entityForm);
        var paymentMethod = await FindPaymentBydId(entityForm.PaymentMethodId, userId);
        if (paymentMethod == null) return;
        
        Account? account = null;
        if (entityForm.AccountId is not null)
        {
            account = await _accountApplication.FindByIdAsync(entityForm.AccountId.Value, userId);

            if (account is null) return;
        }

        expense.PrepareToUpdate(entityForm.Location,
            entityForm.Description,
            entityForm.TotalPrice,
            entityForm.TransactionDate,
            entityForm.ExpenseCategories,
            paymentMethod,
            account?.Id,
            entityForm.Installment);

        await _repository.UpdateAsync(expense);
        _logger.LogInformation(Messages.LOG_UPDATED_MESSAGE, nameof(Transaction), userId, expense.ToJson());
    }

    public async Task<TransactionResponse?> GetTransactionAsync(Guid id, Guid userId)
    {
        var expense = await FindByIdAsync(id, userId);
        if (expense is null) return null;

        var result = _mapper.Map<TransactionResponse>(expense);
        _logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(TransactionResponse), userId, result.ToJson());
        return result;
    }

    public async Task DeleteTransactionAsync(Guid id, Guid userId)
    {
        var expense = await FindByIdAsync(id, userId);
        if (expense is null) return;

        expense.SetDeletedAt();
        await _repository.UpdateAsync(expense);
        _logger.LogInformation(Messages.LOG_DELETED_MESSAGE, nameof(expense), userId, expense.ToJson());
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

        _logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(PaymentMethod), userId, paymentMethod.ToJson());
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

        _logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(Transaction), userId, expense.ToJson());
        return expense;
    }
}