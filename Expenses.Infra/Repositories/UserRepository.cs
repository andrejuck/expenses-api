using Expenses.Domain.DataContract;
using Libs.Auth.Models;
using MongoDB.Driver;

namespace Expenses.Infra.Repositories {

    public class UserRepository : IUserRepository {

        private readonly DBContext _dbContext;
        private SortDefinitionBuilder<User> _sort = Builders<User>.Sort;
        private FilterDefinitionBuilder<User> _filter = Builders<User>.Filter;
        private UpdateDefinitionBuilder<User> _update = Builders<User>.Update;

        public UserRepository(DBContext dbContext)
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
            var update = _update.Set(a => a, entity);

            await _dbContext.Users.UpdateOneAsync(filter, update);
        }

        public async Task<User> GetByEmail(string email) {
            return await _dbContext.Users.Find(_filter.Eq(u => u.Email, email)).FirstOrDefaultAsync();
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            var filter = IdFilter(id);

            return await _dbContext.Users.Find(filter).FirstOrDefaultAsync();
        }
    }
}