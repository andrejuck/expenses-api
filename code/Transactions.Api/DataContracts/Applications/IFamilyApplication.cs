using Libs.Api.Models;
using Transactions.Api.PresentationContracts.Families;
using Transactions.Domain.Models.Accounts;

namespace Transactions.Api.DataContracts.Applications;

public interface IFamilyApplication
{
    Task<FamilyResponse?> FetchFamilyByIdAsync(Guid id, Guid userId);
    Task<List<FamilyResponse>> FetchUserFamiliesAsync(Guid userId);
    Task<Guid?> CreateFamilyAsync(FamilyForm form, Guid userId, string userName);
    Task<Guid?> UpdateFamilyAsync(Guid familyId, FamilyForm form, Guid userId);
    Task DeleteByIdAsync(Guid id, Guid userId);
}