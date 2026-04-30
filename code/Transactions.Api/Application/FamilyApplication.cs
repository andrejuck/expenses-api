using System.Net;
using Libs.Api.ErrorHandling;
using MongoDB.Bson;
using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.Helpers;
using Transactions.Api.PresentationContracts.Families;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Exceptions;
using Transactions.Domain.Models;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Enum;
using Transactions.Domain.Models.Families;

namespace Transactions.Api.Application;

public class FamilyApplication(
    IFamilyAdapter adapter,
    IFamilyRepository repository,
    IAccountApplication accountApplication,
    IUserRepository userRepository,
    IErrorService errorService,
    ILogger<FamilyApplication> logger) : IFamilyApplication
{
    
    public async Task<FamilyResponse?> FetchFamilyByIdAsync(Guid id, Guid userId)
    {
        var entity = await FindByIdAsync(id, userId);
        return entity is null ? null : adapter.ConvertToResponse(entity);
    }

    public async Task<List<FamilyResponse>> FetchUserFamiliesAsync(Guid userId)
    {
        var families = await repository.FetchAllUserFamiliesAsync(userId);
        return adapter.ConvertToResponse(families);
    }

    public async Task<FamilyResponse?> CreateFamilyAsync(FamilyForm form, Guid userId, string userName)
    {
        var existingAccounts = new List<Account>();
        var existingMembers = new List<FamilyMember>();
        if (!await ValidateAccountsAsync(form.Accounts, existingAccounts)) return null;
        
        await FetchFamilyMembersAsync(form.Members, existingMembers);
        
        var entity = adapter.ConvertToDomain(form, userId, userName, existingAccounts, existingMembers);
        
        await repository.AddAsync(entity);
        await accountApplication.BindFamilyToAccountAsync(entity.Id, existingAccounts);
        
        logger.LogInformation(Messages.LOG_CREATED_MESSAGE, nameof(Family), userId, entity.ToJson());
        return adapter.ConvertToResponse(entity);
    }

    public async Task<FamilyResponse?> UpdateFamilyAsync(Guid familyId, FamilyForm form, Guid userId)
    {
        var existing = await FindByIdAsync(familyId, userId);
        var existingAccounts = new List<Account>();
        var existingMembers = new List<FamilyMember>();
        if(existing is null) return null;
        
        if(!ValidateForUpdate(existing, userId)) return null;
        if(!await ValidateAccountsAsync(form.Accounts, existingAccounts)) return null;
        
        await FetchFamilyMembersAsync(form.Members, existingMembers);
        
        existing.PrepareToUpdate(form.FamilyName,
            existingMembers,
            adapter.ConvertToDomain(existingAccounts)
            );
        
        await repository.UpdateAsync(existing);
        await accountApplication.BindFamilyToAccountAsync(existing.Id, existingAccounts);
        
        logger.LogInformation(Messages.LOG_UPDATED_MESSAGE, nameof(Account), userId, existing.ToJson());
        return adapter.ConvertToResponse(existing);
    }

    public async Task DeleteByIdAsync(Guid id, Guid userId)
    {
        var entity = await FindByIdAsync(id, userId);
        if (entity is null) return;
        
        entity.SetDeletedAt();
        await repository.UpdateAsync(entity);
        logger.LogInformation(Messages.LOG_DELETED_MESSAGE, nameof(Family), userId, entity.ToJson());
    }
    
    private async Task<Family?> FindByIdAsync(Guid id, Guid userId)
    {
        var family = await repository.FindByIdAsync(id, userId);
        if (family is null)
        {
            ErrorIdNotFound<Family>(id);
            return null;
        }

        logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(Family), userId, family.ToJson());
        return family;
    }

    private bool ValidateForUpdate(Family existingEntity, Guid userId)
    {
        if (ValidateOwnerUser(existingEntity, userId)) return true;
        
        errorService.AddError(
            nameof(FindByIdAsync),
            DomainMessages.FAMILY_UPDATING_USER_INVALID,
            HttpStatusCode.BadRequest
        );
            
        return false;
    }
    
    private async Task<bool> ValidateAccountsAsync(IEnumerable<Guid> accountIds, List<Account> accounts)
    {
        foreach (var formAccountGuid in accountIds)
        {
            var account = await accountApplication.FetchAccountByIdAsync(formAccountGuid);
            if (account is null)
            {
                ErrorIdNotFound<Account>(formAccountGuid);
                continue;
            }

            if (account.AccountType.Equals(AccountType.Personal))
            {
                errorService.AddError(nameof(ValidateAccountsAsync),
                    DomainMessages.FAMILY_INVALID_ACCOUNTS_TYPE,
                    HttpStatusCode.BadRequest);
                
                continue;
            }
            
            accounts.Add(account);
        }
        
        return !errorService.HasErrors;
    }
    
    private async Task FetchFamilyMembersAsync(IEnumerable<string> formMembers, List<FamilyMember> existingMembers)
    {
        foreach (var formMember in formMembers)
        {
            var user = await  userRepository.GetByEmailAsync(formMember);
            if (user is not null)
            {
                existingMembers.Add(adapter.ConvertToDomain(user));
                continue;
            }
            
            existingMembers.Add(new  FamilyMember { Email = formMember });
        }
    }

    private bool ValidateOwnerUser(Family existingEntity, Guid userId) => userId.Equals(existingEntity.OwnerUserId);
    
    private void ErrorIdNotFound<T>(Guid id) where T : BaseEntity => 
        errorService.AddError(
            nameof(FindByIdAsync),
            string.Format(Messages.NOT_FOUND_MESSAGE_PATTERN, nameof(T), "Id", id),
            HttpStatusCode.NotFound
        );
}