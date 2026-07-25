using FluentValidation;
using Libs.Api.ErrorHandling.Exceptions;
using Transactions.Api.Helpers;
using Transactions.Api.PresentationContracts.Accounts;
using Transactions.Api.PresentationContracts.Families;
using Transactions.Domain.Exceptions;

namespace Transactions.Api.Validators;

public class FamilyFormValidator : AbstractValidator<FamilyForm>
{

    public FamilyFormValidator()
    {
        RuleFor(x => x.FamilyName)
            .NotEmpty()
            .WithMessage(string.Format(Messages.BAD_REQUEST_EMPTY_FIELD, nameof(FamilyForm.FamilyName)))
            .Length(3, 50)
            .WithMessage(string.Format(Messages.BAD_REQUEST_LENGTH_VALUE_BETWEEN, nameof(FamilyForm.FamilyName), 3, 50));

        RuleFor(x => x.Members.Count())
            .InclusiveBetween(1, 5)
            .WithMessage(DomainMessages.FAMILY_INVALID_MEMBERS_QUANTITY);
        
        RuleFor(x => x.Accounts.Count())
            .GreaterThan(0)
            .WithMessage(DomainMessages.FAMILY_INVALID_ACCOUNTS_QUANTITY);
    }
}