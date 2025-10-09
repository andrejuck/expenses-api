using Expenses.Domain.DataContracts.Generics;
using Expenses.Domain.Models;
using Libs.Api.Models;
using Libs.Auth.Models;

namespace Expenses.Domain.DataContracts;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User> FindByIdAsync(Guid id);
    Task<User> GetByEmailAsync(string email);
    Task<List<TResponse>> GetAllPagedAsync<TResponse>(UserSearchParam searchParam, PagedRequest request);
    Task<long> GetAllCountAsync(UserSearchParam searchParam);
}
