using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Libs.Api.Infra;
using MongoDB.Driver;

namespace Expenses.Infra.Repositories;

public class ModuleRepository : BaseMongoRepository<Module>, IModuleRepository
{

    private readonly DBContext _dbContext;

    public ModuleRepository(DBContext dbContext)
        : base(dbContext.Modules)
    {
        _dbContext = dbContext;
    }

    private FilterDefinition<Module> IdFilter(Guid id)
    {
        return _filterBuilder.Eq(a => a.Id, id);
    }

    private FilterDefinition<Module> NotDeletedFilter() {
        return _filterBuilder.Eq(a => a.DeletedAt, null);
    }

    public async Task<Module> FindByIdAsync(Guid id)
    {
          var filters = _filterBuilder.And(NotDeletedFilter(), IdFilter(id));
        return await _dbContext.Modules.Find(IdFilter(id)).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Module entity)
    {
        var filter = IdFilter(entity.Id);
        await base.UpdateAsync(entity, filter);
    }

    public async Task<List<Module>> FindAllByRolesAsync(IEnumerable<string> roles)
    {
        var filters = _filterBuilder.And(NotDeletedFilter(), _filterBuilder.AnyIn("AllowedRoles", roles));

        return await _dbContext.Modules.Find(filters).ToListAsync();
    }

    public async Task<Module> FindByNameAsync(string name)
    {
        var filters = _filterBuilder.And(NotDeletedFilter(), _filterBuilder.Eq(x => x.Name, name));
        return await _dbContext.Modules.Find(filters).FirstOrDefaultAsync();
    }

}