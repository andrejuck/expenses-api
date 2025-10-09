using Expenses.Domain.Models;
using FluentValidation;

namespace Expenses.Domain.Validators.Expense
{
    public class ExpenseSearchParamValidator : AbstractValidator<ExpenseSearchParam>
    {
        public ExpenseSearchParamValidator()
        {
            RuleFor(m => m.EndTransactionDate)
                .GreaterThanOrEqualTo(x => x.StartTransactionDate)
                    .When(m => m.StartTransactionDate.HasValue)
                    .WithMessage("EndTransactionDate can not be lesser than StartTransactionDate");
        }
    }
}
