using FluentValidation;
using Transactions.Domain.Models;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Domain.Validators.Expense
{
    public class ExpenseSearchParamValidator : AbstractValidator<TransactionSearchParam>
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
