using System.Net;
using AutoMapper;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.Helpers;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Libs.Api.ErrorHandling;
using Microsoft.AspNetCore.JsonPatch;

namespace Expenses.Api.Application;

public class ExpenseApplication : IExpenseApplication
{
    private readonly IExpenseRepository _repository;
    private readonly IPaymentMethodRepository _paymentRepo;
    private readonly IMapper _mapper;
    private readonly IErrorService _errorService;

    public ExpenseApplication(
        IExpenseRepository repository,
        IMapper mapper,
        IPaymentMethodRepository paymentRepo,
        IErrorService errorService)
    {
        _repository = repository;
        _mapper = mapper;
        _paymentRepo = paymentRepo;
        _errorService = errorService;
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