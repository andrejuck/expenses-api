using Transactions.Domain.Models.Enum;

namespace Transactions.Domain.Models.PaymentMethod;

public class PaymentMethod : BaseUserEntity
{
    public PaymentMethod(string name, PaymentType paymentType, bool? isDefault)
    {
        Name = name;
        PaymentType = paymentType;
        IsActive = true;
        IsDefault = isDefault ?? false;
    }

    public string Name { get; private set; }
    public PaymentType PaymentType { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; }

    public void PrepareToUpdate(string name, PaymentType paymentType, bool isActive)
    {
        Name = name;
        PaymentType = paymentType;
        IsActive = isActive;

        SetUpdatedAt();
    }
}