using Transactions.Api.Helpers;
using Transactions.Api.PresentationContracts.Expenses;
using FluentValidation;

namespace Transactions.Api.Validators;

public class ExpenseFormValidator : AbstractValidator<TransactionForm>
{
    public ExpenseFormValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(string.Format(Messages.BAD_REQUEST_EMPTY_FIELD, nameof(TransactionForm.Description)))
            .Length(5, 150)
            .WithMessage(string.Format(Messages.BAD_REQUEST_LENGTH_VALUE_BETWEEN, nameof(TransactionForm.Description), 5, 150));

        RuleFor(x => x.TotalPrice)
            .GreaterThan(0)
            .WithMessage(string.Format(Messages.BAD_REQUEST_LESSER_VALUE, nameof(TransactionForm.TotalPrice), 0))
            .NotEmpty()
            .WithMessage(string.Format(Messages.BAD_REQUEST_EMPTY_FIELD, nameof(TransactionForm.TotalPrice)));

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .WithMessage(string.Format(Messages.BAD_REQUEST_EMPTY_FIELD, nameof(TransactionForm.TransactionDate)));

        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage(string.Format(Messages.BAD_REQUEST_EMPTY_FIELD, nameof(TransactionForm.PaymentMethodId)));

        RuleFor(x => x.ExpenseCategories)
            .ForEach(x => x.MinimumLength(3))
            .WithMessage(string.Format(Messages.BAD_REQUEST_LENGTH_VALUE_HIGHER_THAN, nameof(TransactionForm.ExpenseCategories), 3));
    }
}