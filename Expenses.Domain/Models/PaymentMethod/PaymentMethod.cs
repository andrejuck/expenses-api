using Expenses.Domain.Models.Enum;

namespace Expenses.Domain.Models;

public class PaymentMethod : BaseUserEntity
{
    public PaymentMethod(string name, PaymentType paymentType)
    {
        Name = name;
        PaymentType = paymentType;
        IsActive = true;
    }

    public string Name { get; private set; }
    public PaymentType PaymentType { get; private set; }
    public bool IsActive { get; private set; }

    public void PrepareToUpdate(string name, PaymentType paymentType, bool isActive)
    {
        Name = name;
        PaymentType = paymentType;
        IsActive = isActive;

        SetUpdateAt();
    }
}