using System.Net;
using Libs.Api.Adapters;
using Libs.Api.ErrorHandling;
using Libs.Api.Models;
using MongoDB.Bson;
using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.Helpers;
using Transactions.Api.PresentationContracts;
using Transactions.Api.PresentationContracts.Accounts;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models.Accounts;

namespace Transactions.Api.Application;

public class AccountApplication(
    IAccountAdapter adapter,
    IAccountRepository repository,
    IErrorService errorService,
    ILogger<AccountApplication> logger,
    IPaginationAdapter pageAdapter) : IAccountApplication
{
    public async Task<AccountResponse> CreateAccountAsync(AccountForm form, Guid userId)
    {
        var entity = adapter.ConvertToDomain(form, userId);
        await repository.AddAsync(entity);

        logger.LogInformation(Messages.LOG_CREATED_MESSAGE, nameof(Account), userId, entity.ToJson());
        return adapter.ConvertToResponse(entity);
    }

    public async Task<AccountResponse?> UpdateAccountAsync(Guid accountId, AccountForm form, Guid userId)
    {   
        if(!ValidateForUpdate(form)) return null;
        
        var existing = await FindByIdAsync(accountId, userId);
        if(existing is null) return null;
        
        existing.PrepareToUpdate(form.AccountName, form.AccountType, existing.FamilyId);
        await repository.UpdateAsync(existing);
        
        return adapter.ConvertToResponse(existing);
    }

    public async Task<PagedResponse<AccountResponse>> FetchPagedAccountsAsync(AccountSearchParam searchParams,
        PagedRequest pagedRequest,
        Guid userId)
    {
        var result = await repository.GetAllPagedAsync<AccountResponse>(searchParams, pagedRequest, userId);
        var total = await repository.GetAllCountAsync(searchParams, userId);
        var pagedResponse = pageAdapter.ConvertToResponse(pagedRequest, total, result);

        logger.LogInformation(Messages.LOG_GET_MULTIPLE_MESSAGE, pagedResponse.Result.Count(), nameof(AccountResponse),
            total, userId);
        return pagedResponse;
    }

    public async Task<AccountResponse?> FetchAccountByIdAsync(Guid id, Guid userId)
    {
        var entity = await FindByIdAsync(id, userId);
        return entity is null ? null : adapter.ConvertToResponse(entity);
    }

    public async Task DeleteByIdAsync(Guid id, Guid userId)
    {
        var entity = await FindByIdAsync(id, userId);
        if (entity is null) return;
        
        entity.SetDeleted();
        await repository.UpdateAsync(entity);
        logger.LogInformation(Messages.LOG_DELETED_MESSAGE, nameof(Account), userId, entity.ToJson());
    }

    private async Task<Account?> FindByIdAsync(Guid id, Guid userId)
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

    private bool ValidateForUpdate(AccountForm form)
    {
        if (form.InitialBalance is null) return true;
        
        errorService.AddError(nameof(UpdateAccountAsync),
            string.Format(Messages.BAD_REQUEST_FILLED_MUST_BE_EMPTY, nameof(AccountForm.InitialBalance)),
            HttpStatusCode.BadRequest);
            
        return false;

    }
}