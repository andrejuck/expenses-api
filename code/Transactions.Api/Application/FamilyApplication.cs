using System.Net;
using Libs.Api.ErrorHandling;
using Libs.Api.Models;
using MongoDB.Bson;
using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.Helpers;
using Transactions.Api.PresentationContracts.Families;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Exceptions;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Families;

namespace Transactions.Api.Application;

public class FamilyApplication(
    IFamilyAdapter adapter,
    IFamilyRepository repository,
    IAccountRepository accountRepository,
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

    public async Task<FamilyResponse> CreateFamilyAsync(FamilyForm form, Guid userId)
    {
        var entity = adapter.ConvertToDomain(form, userId);
        //TODO - Fetch account
        //TODO - Fetch user
        await repository.AddAsync(entity);

        logger.LogInformation(Messages.LOG_CREATED_MESSAGE, nameof(Family), userId, entity.ToJson());
        return adapter.ConvertToResponse(entity);
    }

    public async Task<FamilyResponse?> UpdateFamilyAsync(Guid familyId, FamilyForm form, Guid userId)
    {
        var existing = await FindByIdAsync(familyId, userId);
        if(existing is null) return null;
        
        if(!ValidateForUpdate(existing, userId)) return null;
        
        //TODO - adicionar a validaçao das contas
        existing.PrepareToUpdate(form.FamilyName,
            adapter.ConvertToDomain(form.Members),
            );
        await repository.UpdateAsync(existing);
        
        logger.LogInformation(Messages.LOG_UPDATED_MESSAGE, nameof(Account), userId, existing.ToJson());
        return adapter.ConvertToResponse(existing);
    }

    public async Task DeleteByIdAsync(Guid id, Guid userId)
    {
        throw new NotImplementedException();
    }
    
    private async Task<Family?> FindByIdAsync(Guid id, Guid userId)
    {
        var family = await repository.FindByIdAsync(id, userId);
        if (family is null)
        {
            errorService.AddError(
                nameof(FindByIdAsync),
                string.Format(Messages.NOT_FOUND_MESSAGE_PATTERN, nameof(Family), nameof(Family.Id), id),
                HttpStatusCode.NotFound
            );

            return null;
        }

        logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(Family), userId, family.ToJson());
        return family;
    }

    private bool ValidateForUpdate(Family existingEntity, Guid userId)
    {
        if (userId.Equals(existingEntity.OwnerUser)) return true;
        
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
            var account = await accountRepository.FetchByIdAsync(formAccountGuid);
            if (account is null)
            {
                errorService.AddError();
            }
        }
    }
}