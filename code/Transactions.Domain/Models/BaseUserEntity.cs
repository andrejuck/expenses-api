namespace Transactions.Domain.Models;

public abstract class BaseUserEntity : BaseEntity
{
    public Guid UserId { get; private set; }

    public virtual void BindUser(Guid id)
    {
        UserId = id;
    }
}