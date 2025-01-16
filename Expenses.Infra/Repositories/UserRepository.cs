using Expenses.Domain.DataContracts;
using Libs.Api.Infra;
using Libs.Api.Models;
using Libs.Auth.Models;
using MongoDB.Driver;

namespace Expenses.Infra.Repositories {

    public class UserRepository : BaseMongoRepository<User>, IUserRepository {

        private readonly DBContext _dbContext;

        public UserRepository(DBContext dbContext)
            : base()
        {
            _dbContext = dbContext;
        }

        private FilterDefinition<User> IdFilter(Guid id)
        {
            return _filter.Eq(a => a.Id, id);
        }
        
        public async Task AddAsync(User entity)
        {
            await _dbContext.Users.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(User entity)
        {
            var filter = IdFilter(entity.Id);
            var update = PrepareToUpdate(entity);

            await _dbContext.Users.UpdateOneAsync(filter, update);
        }

        public async Task<User> GetByEmailAsync(string email) {
            return await _dbContext.Users.Find(_filter.Eq(u => u.Email, email)).FirstOrDefaultAsync();
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            var filter = IdFilter(id);

            return await _dbContext.Users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetAllPagedAsync(PagedRequest request)
        {
            SortDefinition<User> sortDefinition = _sort.Ascending(x => x.Email);
            IEnumerable<FilterDefinition<User>> dynamicFilters = null;
            var userQuery = DefineFilters(request, ref sortDefinition, ref dynamicFilters);

            return await userQuery
                    .Skip((request.CurrentPage - 1) * request.PageSize)
                    .Limit(request.PageSize)
                    .Sort(sortDefinition)
                    .ToListAsync();
        }
        public async Task<long> GetAllCountAsync(PagedRequest request)
        {
            SortDefinition<User> sortDefinition = _sort.Ascending(x => x.Email);
            IEnumerable<FilterDefinition<User>> dynamicFilters = null;
            var userQuery = DefineFilters(request, ref sortDefinition, ref dynamicFilters);

            return await userQuery.CountDocumentsAsync();

        }

        private IFindFluent<User, User> DefineFilters(PagedRequest request, ref SortDefinition<User> sortDefinition, ref IEnumerable<FilterDefinition<User>> dynamicFilters)
        {
            if (request.Filters != null)
            {
                dynamicFilters = request.Filters.Select(x => _filter.In(x.Key, x.Value));
            }

            if (request.IsSorted)
            {

                if (request.SortingOrder.order == SortOrder.Ascending)
                {
                    sortDefinition = _sort.Ascending(request.SortingOrder.key);
                }
                else
                {
                    sortDefinition = _sort.Descending(request.SortingOrder.key);
                }
            }

            var userQuery = _dbContext.Users.Find(_filter.Empty);

            if (dynamicFilters != null)
            {
                userQuery = _dbContext.Users.Find(_filter.And(dynamicFilters));
            }

            return userQuery;
        }

    }
}