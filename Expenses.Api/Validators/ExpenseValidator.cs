using Expenses.Api.Helpers;
using Expenses.Api.PresentationContracts.Expenses;
using Expenses.Domain.Models.Enum;
using FluentValidation;

namespace Expenses.Api.Validators;

public class ExpenseValidator : AbstractValidator<ExpenseForm>
{
    public ExpenseValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(string.Format(Messages.BAD_REQUEST_EMPTY_FIELD, nameof(ExpenseForm.Description)))
            .Length(5, 150)
            .WithMessage(string.Format(Messages.BAD_REQUEST_LENGTH_VALUE_BETWEEN, nameof(ExpenseForm.Description), 5, 150));

        RuleFor(x => x.TotalPrice)
            .GreaterThan(0)
            .WithMessage(string.Format(Messages.BAD_REQUEST_LESSER_VALUE, nameof(ExpenseForm.TotalPrice), 0))
            .NotEmpty()
            .WithMessage(string.Format(Messages.BAD_REQUEST_EMPTY_FIELD, nameof(ExpenseForm.TotalPrice)));

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .WithMessage(string.Format(Messages.BAD_REQUEST_EMPTY_FIELD, nameof(ExpenseForm.TransactionDate)));

        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage(string.Format(Messages.BAD_REQUEST_EMPTY_FIELD, nameof(ExpenseForm.PaymentMethodId)));
        
        RuleFor(x => x.ExpenseCategories)
            .ForEach(x => x.MinimumLength(3))
            .WithMessage(string.Format(Messages.BAD_REQUEST_LENGTH_VALUE_HIGHER_THAN, nameof(ExpenseForm.ExpenseCategories), 3));
    }
}