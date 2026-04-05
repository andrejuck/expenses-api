namespace Transactions.Domain.Exceptions;

public static class DomainMessages
{
    internal const string EMPTY_FIELD = "{0} should be filled.";
    internal const string HIGHER_THAN = "{0} should be higher than {1}.";
    internal const string EXPENSE_INSTALLMENT_ALLOWED_ONLY_TO_CREDIT_CARD = "Expense installments is allowed to credit cards only.";

    internal const string ACCOUNT_FAMILYBIND_NOT_ALLOWED_FOR_PERSONAL_ACCOUNT =
        "It is not allowed to bind a Family to a personal account.";

    public const string FAMILY_UPDATING_USER_INVALID = "A family can only be updated or deleted by the Owner";
    public const string FAMILY_INVALID_MEMBERS_QUANTITY = "A family must have at least 1 member and a maximum of 5 members";
    public const string FAMILY_INVALID_ACCOUNTS_QUANTITY = "A family must have at least 1 shared account";
    public const string FAMILY_INVALID_ACCOUNTS_TYPE = "A family should not have personal accounts";
}