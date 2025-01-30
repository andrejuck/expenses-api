namespace Expenses.Domain.Exceptions;

internal static class DomainMessages
{
    internal const string EMPTY_FIELD = "{0} should be filled.";
    internal const string HIGHER_THAN = "{0} should be higher than {1}.";
    internal const string EXPENSE_INSTALLMENT_ALLOWED_ONLY_TO_CREDIT_CARD = "Expense installments is allowed to credit cards only.";
}