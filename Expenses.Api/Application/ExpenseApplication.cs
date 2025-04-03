using AutoMapper;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.Helpers;
using Expenses.Api.PresentationContracts.Expenses;
using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Libs.Api.Adapters;
using Libs.Api.ErrorHandling;
using Libs.Api.Models;
using Microsoft.AspNetCore.JsonPatch;
using MongoDB.Bson;
using System.Net;

namespace Expenses.Api.Application;

public class ExpenseApplication : IExpenseApplication
{
    private readonly IExpenseRepository _repository;
    private readonly IPaymentMethodRepository _paymentRepo;
    private readonly IMapper _mapper;
    private readonly IErrorService _errorService;
    private readonly IPaginationAdapter _pageAdapter;
    private readonly ILogger<ExpenseApplication> _logger;

    public ExpenseApplication(
        IExpenseRepository repository,
        IMapper mapper,
        IPaymentMethodRepository paymentRepo,
        IErrorService errorService,
        IPaginationAdapter pageAdapter,
        ILogger<ExpenseApplication> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _paymentRepo = paymentRepo;
        _errorService = errorService;
        _pageAdapter = pageAdapter;
        _logger = logger;
    }

    public async Task CreateNewExpenseAsync(Guid userId, ExpenseForm form)
    {
        var paymentMethod = await FindPaymentBydId(form.PaymentMethodId, userId);
        if (paymentMethod == null) return;

        var entity = new Expense(form.Location,
            form.Description,
            form.TotalPrice,
            form.TransactionDate,
            form.ExpenseCategories,
            paymentMethod,
            form.Installment);

        entity.BindUser(userId);

        await _repository.AddAsync(entity);
        _logger.LogInformation(Messages.LOG_CREATED_MESSAGE, nameof(Expense), userId, entity.ToJson());
    }

    public async Task<PagedResponse<ExpenseResponse>> GetPagedExpenseAsync(ExpenseSearchParam searchParams, PagedRequest pagedRequest, Guid userId)
    {
        var expensesResponse = await _repository.GetAllPagedAsync<ExpenseResponse>(searchParams, pagedRequest, userId);
        var total = await _repository.GetAllCountAsync(searchParams, userId);
        var pagedResponse = _pageAdapter.ConvertToResponse(pagedRequest, total, expensesResponse);

        _logger.LogInformation(Messages.LOG_GET_MULTIPLE_MESSAGE, pagedResponse.Result.Count(), nameof(ExpenseResponse), total, userId);
        return pagedResponse;
    }

    public async Task<PagedResponse<GroupedExpensesResponse>> GetGroupedPagedExpenseAsync(ExpenseSearchParam searchParams, PagedRequest pagedRequest, Guid userId)
    {
        var expensesResponse = await _repository.GetAllGroupedPagedAsync<GroupedExpensesResponse>(searchParams, pagedRequest, userId);
        var total = await _repository.GetAllCountAsync(searchParams, userId);
        var pagedResponse = _pageAdapter.ConvertToResponse(pagedRequest, total, expensesResponse);

        return pagedResponse;
    }

    public async Task<List<string>> GetUserCategoriesAsync(Guid userId)
    {
        var result = await _repository.GetAllUserCategories(userId);
        _logger.LogInformation(Messages.LOG_GET_MULTIPLE_MESSAGE, result.Count, nameof(ExpenseResponse), result.Count, userId);
        return result;
    }

    public async Task UpdateExpenseAsync(Guid id, Guid userId, JsonPatchDocument<ExpenseForm> patchForm)
    {
        var expense = await FindByIdAsync(id, userId);
        if (expense is null) return;

        var entityForm = _mapper.Map<ExpenseForm>(expense);

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
        _logger.LogInformation(Messages.LOG_UPDATED_MESSAGE, nameof(Expense), userId, expense.ToJson());
    }

    public async Task<ExpenseResponse> GetExpenseAsync(Guid id, Guid userId)
    {
        var expense = await FindByIdAsync(id, userId);
        if (expense is null) return null;

        var result = _mapper.Map<ExpenseResponse>(expense);
        _logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(ExpenseResponse), userId, result.ToJson());
        return result;
    }

    public async Task DeleteExpenseAsync(Guid id, Guid userId)
    {
        var expense = await FindByIdAsync(id, userId);
        if (expense is null) return;

        expense.SetDeleted();
        await _repository.UpdateAsync(expense);
        _logger.LogInformation(Messages.LOG_DELETED_MESSAGE, nameof(expense), userId, expense.ToJson());
    }

    public async Task<List<ExpenseFileResponse>> GetAllExpensesAsync(ExpenseSearchParam searchParams, Guid userId)
    {
        var result = await _repository.GetAllAsync<ExpenseFileResponse>(searchParams, userId);
        _logger.LogInformation(Messages.LOG_GET_MULTIPLE_MESSAGE, result.Count, nameof(ExpenseResponse), result.Count, userId);
        return result;
    }

    private async Task<PaymentMethod> FindPaymentBydId(Guid paymentId, Guid userId)
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

    private async Task<Expense> FindByIdAsync(Guid id, Guid userId)
    {
        var expense = await _repository.FindByIdAsync(id, userId);
        if (expense is null)
        {
            _errorService.AddError(
                nameof(GetExpenseAsync),
                string.Format(Messages.NOT_FOUND_MESSAGE_PATTERN, nameof(Expense), nameof(Expense.Id), id),
                HttpStatusCode.NotFound
            );

            return null;
        }

        _logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(Expense), userId, expense.ToJson());
        return expense;
    }
}