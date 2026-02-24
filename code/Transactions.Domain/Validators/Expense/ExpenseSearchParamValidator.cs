using FluentValidation;
using Transactions.Domain.Models;

namespace Transactions.Domain.Validators.Expense
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
