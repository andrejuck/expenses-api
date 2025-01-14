using Libs.Auth.Models;

namespace Expenses.Domain.DataContract;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User> GetByEmail(string email);
}
