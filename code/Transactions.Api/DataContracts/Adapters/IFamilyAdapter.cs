using Libs.Auth.Models;
using Transactions.Api.PresentationContracts.Families;
using Transactions.Domain.Dtos;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Families;

namespace Transactions.Api.DataContracts.Adapters;

public interface IFamilyAdapter
{
    FamilyResponse ConvertToResponse(FamilyDto dto);
    List<FamilyResponse> ConvertToResponse(IEnumerable<FamilyDto> dtoList);
    Family ConvertToDomain(FamilyForm form, Guid userId, string userName, IEnumerable<Guid> accounts, IEnumerable<Guid> members);
}