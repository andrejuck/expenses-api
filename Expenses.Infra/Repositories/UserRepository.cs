using Expenses.Domain.DataContracts;
using Libs.Api.Infra;
using Libs.Api.Models;
using Libs.Auth.Models;
using MongoDB.Driver;

namespace Expenses.Infra.Repositories
{

    public class UserRepository : BasePageableMongoRepository<User>, IUserRepository
    {
        private readonly DBContext _dbContext;

        public UserRepository(DBContext dbContext)
            : base(dbContext.Users)
        {
            _dbContext = dbContext;
        }

        private FilterDefinition<User> IdFilter(Guid id)
        {
            return _filterBuilder.Eq(a => a.Id, id);
        }

        public async Task UpdateAsync(User entity)
        {
            var filter = IdFilter(entity.Id);
            await base.UpdateAsync(entity, filter);
        }

        public Task<long> GetAllCountAsync(PagedRequest request)
        {
            var sortDefinition = _sortBuilder.Ascending(x => x.Email);
            return base.GetAllCountAsync(request, sortDefinition);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbContext.Users.Find(_filterBuilder.Eq(u => u.Email, email)).FirstOrDefaultAsync();
        }

        public async Task<User> FindByIdAsync(Guid id)
        {
            var filter = IdFilter(id);

            return await _dbContext.Users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetAllPagedAsync(PagedRequest request)
        {
            var sortDefinition = _sortBuilder.Ascending(x => x.Email);
            return await base.GetAllPagedAsync(request, sortDefinition);
        }

    }
}