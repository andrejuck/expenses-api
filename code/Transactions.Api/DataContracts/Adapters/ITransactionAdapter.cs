using Transactions.Api.PresentationContracts.Expenses;
using Transactions.Domain.Models;
using Transactions.Domain.Models.PaymentMethod;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Api.DataContracts.Adapters;

public interface ITransactionAdapter
{
    Transaction ConvertToDomain(TransactionForm form, PaymentMethod paymentMethod);
    TransactionResponse ConvertToResponse(Transaction domain);
    TransactionForm ConvertToForm(Transaction domain);
}
