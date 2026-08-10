using Transactions.Domain.Models.Enum;

namespace Transactions.Domain.Models.PaymentMethod;

public class PaymentMethod : BaseUserEntity
{
    public PaymentMethod(string name,
        PaymentType paymentType,
        Guid? accountId,
        bool isDefault = false)
    {
        Name = name;
        PaymentType = paymentType;
        AccountId = accountId;
        IsActive = true;
        IsDefault = isDefault;
    }

    public string Name { get; private set; }
    public PaymentType PaymentType { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; }
    public Guid? AccountId { get; private set; }

    public void PrepareToUpdate(string name, 
        PaymentType paymentType, 
        bool isActive,
        Guid? accountId)
    {
        Name = name;
        PaymentType = paymentType;
        IsActive = isActive;
        AccountId = accountId;

        SetUpdatedAt();
    }
}