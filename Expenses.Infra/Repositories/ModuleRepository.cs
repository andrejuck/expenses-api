using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Libs.Api.Infra;
using MongoDB.Driver;

namespace Expenses.Infra.Repositories;

public class ModuleRepository : BaseMongoRepository<Module>, IModuleRepository
{

    private readonly DBContext _dbContext;

    public ModuleRepository(DBContext dbContext)
    {
        _dbContext = dbContext;
    }

    private FilterDefinition<Module> IdFilter(Guid id)
    {
        return _filter.Eq(a => a.Id, id);
    }

    private FilterDefinition<Module> NotDeletedFilter() {
        return _filter.Eq(a => a.DeletedAt, null);
    }

    public async Task AddAsync(Module entity)
    {
        await _dbContext.Modules.InsertOneAsync(entity);
    }

    public async Task<Module> FindByIdAsync(Guid id)
    {
          var filters = _filter.And(NotDeletedFilter(), IdFilter(id));
        return await _dbContext.Modules.Find(IdFilter(id)).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Module entity)
    {
        var filter = IdFilter(entity.Id);
        var update = PrepareToUpdate(entity);

        await _dbContext.Modules.UpdateOneAsync(filter, update);
    }

    public async Task<List<Module>> FindAllByRolesAsync(IEnumerable<string> roles)
    {
        var filters = _filter.And(NotDeletedFilter(), _filter.AnyIn("AllowedRoles", roles));

        return await _dbContext.Modules.Find(filters).ToListAsync();
    }

    public async Task<Module> FindByNameAsync(string name)
    {
        var filters = _filter.And(NotDeletedFilter(), _filter.Eq(x => x.Name, name));
        return await _dbContext.Modules.Find(filters).FirstOrDefaultAsync();
    }

}