using FluentValidation;
using Transactions.Api.Helpers;
using Transactions.Api.PresentationContracts.Accounts;
using Transactions.Api.PresentationContracts.Forms;

namespace Transactions.Api.Validators;

public class AccountFormValidator : AbstractValidator<AccountForm>
{

    public AccountFormValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .WithMessage(string.Format(Messages.BAD_REQUEST_EMPTY_FIELD, nameof(AccountForm.AccountName)))
            .Length(3, 50)
            .WithMessage(string.Format(Messages.BAD_REQUEST_LENGTH_VALUE_BETWEEN, nameof(AccountForm.AccountName), 3, 50));
    }
    
}