using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.PresentationContracts.Expenses;
using Transactions.Domain.Models;
using Transactions.Domain.Models.PaymentMethod;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Api.Adapters;

public class TransactionAdapter(IPaymentMethodAdapter paymentMethodAdapter) : ITransactionAdapter
{
    public Transaction ConvertToDomain(TransactionForm form, PaymentMethod paymentMethod) =>
        new(form.Location,
            form.Description,
            form.TotalPrice,
            form.TransactionDate,
            form.ExpenseCategories,
            paymentMethod,
            form.TransactionType,
            form.Installment);

    public TransactionResponse ConvertToResponse(Transaction domain) =>
        new()
        {
            Id = domain.Id,
            Location = domain.Location ?? string.Empty,
            Description = domain.Description,
            TotalPrice = domain.TotalPrice,
            TransactionDate = domain.TransactionDate,
            ExpenseCategories = domain.ExpenseCategories ?? [],
            PaymentMethod = paymentMethodAdapter.ConvertToResponse(domain.PaymentMethod),
            Installment = domain.Installment,
            UserId = domain.UserId
        };

    public TransactionForm ConvertToForm(Transaction domain) =>
        new()
        {
            Location = domain.Location,
            Description = domain.Description,
            TotalPrice = domain.TotalPrice,
            TransactionDate = domain.TransactionDate,
            ExpenseCategories = domain.ExpenseCategories,
            PaymentMethodId = domain.PaymentMethodId,
            Installment = domain.Installment,
            TransactionType = domain.TransactionType
        };
}
