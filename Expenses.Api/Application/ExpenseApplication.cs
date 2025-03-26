using System.Net;
using AutoMapper;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.Helpers;
using Expenses.Api.PresentationContracts.Expenses;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Libs.Api.Adapters;
using Libs.Api.ErrorHandling;
using Libs.Api.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Expenses.Api.Application;

public class ExpenseApplication : IExpenseApplication
{
    private readonly IExpenseRepository _repository;
    private readonly IPaymentMethodRepository _paymentRepo;
    private readonly IMapper _mapper;
    private readonly IErrorService _errorService;
    private readonly IPaginationAdapter _pageAdapter;

    public ExpenseApplication(
        IExpenseRepository repository,
        IMapper mapper,
        IPaymentMethodRepository paymentRepo,
        IErrorService errorService,
        IPaginationAdapter pageAdapter)
    {
        _repository = repository;
        _mapper = mapper;
        _paymentRepo = paymentRepo;
        _errorService = errorService;
        _pageAdapter = pageAdapter;
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
    }

    public async Task<PagedResponse<ExpenseResponse>> GetPagedExpenseAsync(ExpenseSearchParam searchParams, PagedRequest pagedRequest, Guid userId)
    {
        var expensesResponse = await _repository.GetAllPagedAsync<ExpenseResponse>(searchParams, pagedRequest, userId);
        var total = await _repository.GetAllCountAsync(searchParams, userId);
        var pagedResponse = _pageAdapter.ConvertToResponse(pagedRequest, total, expensesResponse);

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
        return await _repository.GetAllUserCategories(userId);
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
    }

    public async Task<ExpenseResponse> GetExpenseAsync(Guid id, Guid userId)
    {
        var expense = await FindByIdAsync(id, userId);
        if (expense is null) return null;

        return _mapper.Map<ExpenseResponse>(expense);
    }

    public async Task DeleteExpenseAsync(Guid id, Guid userId)
    {
        var expense = await FindByIdAsync(id, userId);
        if (expense is null) return;

        expense.SetDeleted();
        await _repository.UpdateAsync(expense);
    }

    public async Task<List<ExpenseFileResponse>> GetAllExpensesAsync(ExpenseSearchParam searchParams, Guid userId)
    {
        var result = await _repository.GetAllAsync<ExpenseFileResponse>(searchParams, userId);
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

        return expense;
    }  
}