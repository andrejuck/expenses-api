using Expenses.Domain.Models.Enum;

namespace Expenses.Api.PresentationContracts;

public class PaymentMethodResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public PaymentType PaymentType { get; set; }
    public bool IsActive { get; set; }
}