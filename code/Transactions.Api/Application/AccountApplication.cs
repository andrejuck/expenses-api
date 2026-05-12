using System.Net;
using Libs.Api.ErrorHandling;
using MongoDB.Bson;
using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.Helpers;
using Transactions.Api.PresentationContracts.Accounts;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models.Accounts;

namespace Transactions.Api.Application;

public class AccountApplication(
    IAccountAdapter adapter,
    IAccountRepository repository,
    IFamilyRepository familyRepository,
    IExpenseRepository expenseRepository,
    IErrorService errorService,
    ILogger<AccountApplication> logger) : IAccountApplication
{
    public async Task<AccountResponse?> CreateAccountAsync(AccountForm form, Guid userId)
    {
        if (! await ValidateForCreation(form, userId)) return null;
        var entity = adapter.ConvertToDomain(form, userId);
        await repository.AddAsync(entity);

        logger.LogInformation(Messages.LOG_CREATED_MESSAGE, nameof(Account), userId, entity.ToJson());
        return adapter.ConvertToResponse(entity);
    }

    public async Task<AccountResponse?> UpdateAccountAsync(Guid accountId, AccountForm form, Guid userId)
    {   
        if(! await ValidateForUpdate(accountId, form, userId)) return null;
        
        var existing = await FindByIdAsync(accountId, userId);
        if(existing is null) return null;
        
        existing.PrepareToUpdate(form.AccountName, form.AccountType, existing.FamilyId);
        await repository.UpdateAsync(existing);
        
        logger.LogInformation(Messages.LOG_UPDATED_MESSAGE, nameof(Account), userId, existing.ToJson());
        return adapter.ConvertToResponse(existing);
    }

    public async Task BindFamilyToAccountAsync(Guid familyId, IEnumerable<Guid> accountsId)
    {
        foreach (var accountId in accountsId)
        {   
            await repository.BindFamilyToAccountAsync(accountId, familyId);
        }
    }

    public async Task<IEnumerable<AccountResponse>> FetchAllAccountsAsync(Guid userId)
    {

        var userAccounts = (await repository.GetAllUserAccountsAsync(userId)).ToList();
        userAccounts.AddRange(await familyRepository.GetAllUserFamilyAccountsAsync(userId));

        logger.LogInformation(Messages.LOG_GET_MULTIPLE_MESSAGE, userAccounts.Count, nameof(AccountResponse), userId);
        return adapter.ConvertToResponse(userAccounts);
    }

    public async Task<AccountResponse?> FetchAccountByIdAsync(Guid id, Guid userId)
    {
        var entity = await FindByIdAsync(id, userId);
        return entity is null ? null : adapter.ConvertToResponse(entity);
    }

    public async Task<Account?> FetchAccountByIdAsync(Guid id) =>
        await repository.FetchByIdAsync(id);

    public async Task DeleteByIdAsync(Guid id, Guid userId)
    {
        var entity = await FindByIdAsync(id, userId);
        if (entity is null) return;

        if (!await ValidateForDeletionAsync(entity.Id, userId)) return;
        
        entity.PrepareToDelete();
        await repository.UpdateAsync(entity);
        logger.LogInformation(Messages.LOG_DELETED_MESSAGE, nameof(Account), userId, entity.ToJson());
    }

    public async Task<Account?> FindByIdAsync(Guid id, Guid userId)
    {
        var account = await repository.FindByIdAsync(id, userId);
        if (account is null)
        {
            errorService.AddError(
                nameof(FindByIdAsync),
                string.Format(Messages.NOT_FOUND_MESSAGE_PATTERN, nameof(Account), nameof(Account.Id), id),
                HttpStatusCode.NotFound
            );

            return null;
        }

        logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(Account), userId, account.ToJson());
        return account;
    }

    public async Task<IEnumerable<RecentAccountResponse>> FetchRecentAccountsAsync(Guid userId, int limit) =>
        adapter.ConvertToRecentAccountResponse(await repository.FetchRecentAccountsSortedByUpdateDateAsync(userId, limit));

    private async Task<bool> ValidateForUpdate(Guid accountId, AccountForm form,  Guid userId)
    {
        var isValid = ValidateInitialBalance(form);
        isValid = await ValidateNameAsync(accountId, form.AccountName, userId);
            
        return isValid;
    }
    
    private async Task<bool> ValidateForCreation(AccountForm form, Guid userId)
    {
        var isValid = await ValidateNameAsync(null, form.AccountName, userId); 
        return isValid;
    }

    private async Task<bool> ValidateNameAsync(Guid? accountId, string accountName, Guid userId)
    {
        var existing = await repository.FetchByNameAsync(accountName, userId);
        if (accountId is not null) existing = await repository.FetchByNameAsync(accountName, accountId.Value, userId);
        
        if (existing is null) return true; 
        
        errorService.AddError(nameof(UpdateAccountAsync),
            string.Format(
                Messages.CONFLICT_MESSAGE_PATTERN, nameof(Account), nameof(AccountForm.AccountName), existing.Name
            ),
            HttpStatusCode.Conflict);

        return false;
    }

    private bool ValidateInitialBalance(AccountForm form)
    {
        if (form.InitialBalance is null) return true;
        
        errorService.AddError(nameof(UpdateAccountAsync),
            string.Format(Messages.BAD_REQUEST_FILLED_MUST_BE_EMPTY, nameof(AccountForm.InitialBalance)),
            HttpStatusCode.BadRequest);
        
        return false;
    }
    
    private async Task<bool> ValidateForDeletionAsync(Guid accountId, Guid userId)
    {
        var hasExpenses = await expenseRepository.GetAnyWithinAccountAsync(accountId, userId);
        if (!hasExpenses) return true;
        
        errorService.AddError(nameof(ValidateForDeletionAsync),
            string.Format(Messages.BAD_REQUEST_DELETION_NOT_ALLOWED, nameof(Account), "Account linked to an expense"),
            HttpStatusCode.BadRequest);
        
        return false;
    }
}