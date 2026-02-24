namespace Transactions.Domain.Models;

public abstract class BaseEntity
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.Now;
    }

    public virtual void SetUpdateAt()
    {
        UpdatedAt = DateTime.Now;
    }

    public virtual void SetDeleted()
    {
        DeletedAt = DateTime.Now;
        SetUpdateAt();
    }

}