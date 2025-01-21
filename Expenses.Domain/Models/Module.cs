namespace Expenses.Domain.Models;

public class Module : BaseEntity
{
    public Module() { }
    public Module(string name, params UserRole[] accessRoles)
    {
        Name = name;
        AllowedRoles = accessRoles.ToList();
    }

    public string Name { get; private set; }
    public List<UserRole> AllowedRoles { get; private set; }

}