using System.Net;
using AutoMapper;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.Helpers;
using Expenses.Api.PresentationContracts;
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
        if(paymentMethod == null) return;

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

    public async Task<PagedResponse<ExpenseResponse>> GetPagedExpense(PagedRequest pagedRequest, Guid userId)
    {
        var expenses = await _repository.GetAllPagedAsync(pagedRequest);
        var total = await _repository.GetAllCountAsync(pagedRequest);
        var expensesResponse = _mapper.Map<List<ExpenseResponse>>(expenses);
        var pagedResponse = _pageAdapter.ConvertToResponse(pagedRequest, total, expensesResponse);

        return pagedResponse;
    }

    public async Task UpdateExpenseAsync(Guid id, Guid userId, JsonPatchDocument<ExpenseForm> patchForm)
    {
        var expense = await _repository.FindByIdAsync(id, userId);
        if (expense is null)
        {
            _errorService.AddError(
                nameof(UpdateExpenseAsync),
                string.Format(Messages.NOT_FOUND_MESSAGE_PATTERN, nameof(Expense), nameof(Expense.Id), id),
                HttpStatusCode.NotFound
            );

            return;
        }

        var entityForm = _mapper.Map<ExpenseForm>(expense);

        patchForm.ApplyTo(entityForm);
        var paymentMethod = await FindPaymentBydId(entityForm.PaymentMethodId, userId);
        if(paymentMethod == null) return;

        expense.PrepareToUpdate(entityForm.Location,
            entityForm.Description,
            entityForm.TotalPrice,
            entityForm.TransactionDate,
            entityForm.ExpenseCategories,
            paymentMethod,
            entityForm.Installment);

        await _repository.UpdateAsync(expense);
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
}