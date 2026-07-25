using System.Net;
using System.Text.Json;
using Libs.Api.ErrorHandling;
using Libs.Auth.Models;
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
        var entity = await repository.FindAndProjectByIdAsync(id);
        return entity is null ? null : adapter.ConvertToResponse(entity);
    }

    public async Task<List<FamilyResponse>> FetchUserFamiliesAsync(Guid userId)
    {
        var families = await repository.FetchAllUserFamiliesAsync(userId);
        return adapter.ConvertToResponse(families);
    }

    public async Task<Guid?> CreateFamilyAsync(FamilyForm form, Guid userId, string userName)
    {
        var membersId = new List<Guid>();
        if (!await ValidateAccountsAsync(form.Accounts)) return null;
        
        await FetchFamilyMembersAsync(form.Members, membersId);
        
        var entity = adapter.ConvertToDomain(form, userId, userName, form.Accounts, membersId);
        await repository.AddAsync(entity);
        await accountApplication.BindFamilyToAccountAsync(entity.Id, form.Accounts);
        
        logger.LogInformation(Messages.LOG_CREATED_MESSAGE, nameof(Family), userId, JsonSerializer.Serialize(entity));
        return entity.Id;
    }

    public async Task<Guid?> UpdateFamilyAsync(Guid familyId, FamilyForm form, Guid userId)
    {
        var existing = await FindByIdAsync(familyId, userId);
        var existingMembers = new List<Guid>();
        if(existing is null) return null;
        
        if(!ValidateForUpdate(existing, userId)) return null;
        if(!await ValidateAccountsAsync(form.Accounts)) return null;
        
        await FetchFamilyMembersAsync(form.Members, existingMembers);
        
        existing.PrepareToUpdate(form.FamilyName,
            existingMembers,
            form.Accounts
            );
        
        await repository.UpdateAsync(existing);
        await accountApplication.BindFamilyToAccountAsync(existing.Id, existing.Accounts);
        
        logger.LogInformation(Messages.LOG_UPDATED_MESSAGE, nameof(Account), userId, JsonSerializer.Serialize(existing));
        return existing.Id;
    }

    public async Task DeleteByIdAsync(Guid id, Guid userId)
    {
        var entity = await FindByIdAsync(id, userId);
        if (entity is null) return;
        
        entity.SetDeletedAt();
        await repository.UpdateAsync(entity);
        logger.LogInformation(Messages.LOG_DELETED_MESSAGE, nameof(Family), userId, JsonSerializer.Serialize(entity));
    }
    
    private async Task<Family?> FindByIdAsync(Guid id, Guid userId)
    {
        var family = await repository.FindByIdAsync(id, userId);
        if (family is null)
        {
            errorService.AddError(
                nameof(FindByIdAsync),
                string.Format(Messages.NOT_FOUND_MESSAGE_PATTERN, nameof(Family), "Id", id),
                HttpStatusCode.NotFound
            );
            return null;
        }

        logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(Family), userId, JsonSerializer.Serialize(family));
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
    
    private async Task<bool> ValidateAccountsAsync(IEnumerable<Guid> accountIds)
    {
        foreach (var formAccountGuid in accountIds)
        {
            var account = await accountApplication.FetchAccountByIdAsync(formAccountGuid);
            if (account is null)
            {
                errorService.AddError(
                    nameof(FindByIdAsync),
                    string.Format(Messages.NOT_FOUND_MESSAGE_PATTERN, nameof(Account), "Id", formAccountGuid),
                    HttpStatusCode.NotFound
                );
                continue;
            }

            if (account.AccountType.Equals(AccountType.Personal))
            {
                errorService.AddError(nameof(ValidateAccountsAsync),
                    DomainMessages.FAMILY_INVALID_ACCOUNTS_TYPE,
                    HttpStatusCode.BadRequest);
            }
        }
        
        return !errorService.HasErrors;
    }
    
    private async Task FetchFamilyMembersAsync(IEnumerable<string> formMembers, List<Guid> membersIds)
    {
        foreach (var formMember in formMembers)
        {
            var user = await  userRepository.GetByEmailAsync(formMember);
            if (user is null)
            {
                logger.LogWarning(Messages.NOT_FOUND_MESSAGE_PATTERN, nameof(User), nameof(User.Email), formMember);
                continue;
            }
            
            membersIds.Add(user.Id);
        }
    }

    private bool ValidateOwnerUser(Family existingEntity, Guid userId) => userId.Equals(existingEntity.OwnerUserId);
}