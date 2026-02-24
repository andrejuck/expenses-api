using Libs.Api.Models;
using Libs.Auth.Models;
using Transactions.Domain.DataContracts.Generics;
using Transactions.Domain.Models;

namespace Transactions.Domain.DataContracts;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User> FindByIdAsync(Guid id);
    Task<User> GetByEmailAsync(string email);
    Task<List<TResponse>> GetAllPagedAsync<TResponse>(UserSearchParam searchParam, PagedRequest request);
    Task<long> GetAllCountAsync(UserSearchParam searchParam);
}
