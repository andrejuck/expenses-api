using Libs.Api.Models;
using Libs.Auth.Models;

namespace Expenses.Domain.DataContracts;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllPagedAsync(PagedRequest request);
    Task<long> GetAllCountAsync(PagedRequest request);
}
