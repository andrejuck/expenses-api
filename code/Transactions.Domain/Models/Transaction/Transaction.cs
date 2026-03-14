using Libs.Api.ErrorHandling.Exceptions;
using Transactions.Domain.Exceptions;
using Transactions.Domain.Models.Enum;

namespace Transactions.Domain.Models.Transaction;

public class Transaction : BaseUserEntity
{
    public Transaction(
        string? location,
        string description,
        decimal totalPrice,
        DateTime transactionDate,
        List<string>? expenseCategories,
        PaymentMethod paymentMethod,
        TransactionType transactionType,
        int? installment = null)
    {
        Location = location;
        Description = description;
        TotalPrice = Math.Round(totalPrice, 2);
        TransactionDate = transactionDate;
        ExpenseCategories = expenseCategories;
        PaymentMethodId = paymentMethod.Id;
        TransactionType = transactionType;
        Installment = installment;

        Validate(paymentMethod);
    }

    public string? Location { get; private set; }
    public string Description { get; private set; }
    public decimal TotalPrice { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public List<string>? ExpenseCategories { get; private set; }
    public Guid PaymentMethodId { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public int? Installment { get; private set; }
    public TransactionType TransactionType { get; private set; }

    public void PrepareToUpdate(string? location,
        string description,
        decimal totalPrice,
        DateTime transactionDate,
        List<string>? expenseCategories,
        PaymentMethod paymentMethod,
        int? installment = null)
    {
        Location = location;
        Description = description;
        TotalPrice = Math.Round(totalPrice, 2);
        TransactionDate = transactionDate;
        ExpenseCategories = expenseCategories;
        PaymentMethodId = paymentMethod.Id;
        Installment = installment;

        SetUpdateAt();
        Validate(paymentMethod);
    }

    public List<Transaction> DivideByInstallments(PaymentMethod paymentMethod)
    {
        if (!paymentMethod.PaymentType.Equals(PaymentType.CreditCard))
            throw new DomainException(DomainMessages.EXPENSE_INSTALLMENT_ALLOWED_ONLY_TO_CREDIT_CARD);

        if (Installment is 0 or null)
            throw new DomainException("Number of Installments should be higher than zero.");

        var unitPrice = TotalPrice / Installment;
        var expenses = new List<Transaction>();
        for (int i = 1; i <= Installment; i++)
        {
            var adaptedDescription = $"{Description} {i}|{Installment}";
            var expense = new Transaction(Location, adaptedDescription, unitPrice.Value, TransactionDate, ExpenseCategories, paymentMethod, TransactionType);
            expenses.Add(expense);
        }

        var diff = TotalPrice - expenses.Sum(x => x.TotalPrice);
        if (diff > 0)
        {
            expenses.Last().TotalPrice += diff;
        }

        return expenses;
    }

    private void Validate(PaymentMethod paymentMethod)
    {
        if (TotalPrice <= 0)
            throw new DomainException(string.Format(DomainMessages.HIGHER_THAN, nameof(TotalPrice), 0));
        if (string.IsNullOrEmpty(Description))
            throw new DomainException(string.Format(DomainMessages.EMPTY_FIELD, nameof(Description)));
        if (PaymentMethodId == Guid.Empty)
            throw new DomainException(string.Format(DomainMessages.EMPTY_FIELD, nameof(PaymentMethod)));
        if (paymentMethod.PaymentType != PaymentType.CreditCard && Installment != null)
            throw new DomainException(DomainMessages.EXPENSE_INSTALLMENT_ALLOWED_ONLY_TO_CREDIT_CARD);
    }
}