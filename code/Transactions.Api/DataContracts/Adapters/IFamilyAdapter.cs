using Transactions.Api.PresentationContracts.Families;
using Transactions.Domain.Models.Families;

namespace Transactions.Api.DataContracts.Adapters;

public interface IFamilyAdapter
{
    FamilyResponse ConvertToResponse(Family entity);
    List<FamilyResponse> ConvertToResponse(IEnumerable<Family> families);
    Family ConvertToDomain(FamilyForm form, Guid userId);
    IEnumerable<FamilyMember> ConvertToDomain(IEnumerable<string> emails);
}