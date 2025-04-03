using Libs.Auth.Models;

namespace Expenses.Domain.Models;

public class Module : BaseEntity
{
    public Module() { }
    public Module(string name, Guid createdBy, params UserRole[] accessRoles)
    {
        Name = name;
        CreatedBy = createdBy;
        AllowedRoles = accessRoles.ToList();
    }

    public string Name { get; private set; }
    public Guid CreatedBy { get; private set; }
    public List<UserRole> AllowedRoles { get; private set; }

    public virtual void BindUser(Guid id)
    {
        CreatedBy = id;
    }

}