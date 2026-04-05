using Transactions.Domain.DataContracts;
using Transactions.Domain.Models;
using Libs.Api.Infra;
using Libs.Api.Models;
using Libs.Auth.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Transactions.Infra.Repositories
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

        public async Task<User> UpdateAsync(User entity)
        {
            var filter = IdFilter(entity.Id);
            await base.UpdateAsync(entity, filter);
            return entity;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbContext.Users.Find(_filterBuilder.Eq(u => u.Email, email)).FirstOrDefaultAsync();
        }

        public async Task<User?> FindByIdAsync(Guid id)
        {
            var filter = IdFilter(id);

            return await _dbContext.Users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<TResponse>> GetAllPagedAsync<TResponse>(UserSearchParam searchParam, PagedRequest request)
        {
            var sortDefinition = _sortBuilder.Descending(x => x.CreatedAt);
            var aggregatedBson = new BsonDocument[] {
                BuildFilters(searchParam),
                BuildSorting(request, ref sortDefinition)
            };

            var pagedResult = await base.GetAllPagedAsync<TResponse>(request, aggregatedBson);

            return pagedResult;
        }

        public Task<long> GetAllCountAsync(UserSearchParam searchParams)
        {
            var filter = _filterBuilder.Empty;
            filter = DefineFilters(searchParams, filter);
            return base.GetAllCountAsync(filter);
        }

    }
}