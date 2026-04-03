namespace Transactions.Domain.Models;

public abstract class BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public virtual void SetUpdatedAt()
    {
        UpdatedAt = DateTime.Now;
    }

    public virtual void SetDeleted()
    {
        DeletedAt = DateTime.Now;
        SetUpdatedAt();
    }

}