namespace Transactions.Api.PresentationContracts.PaymentMethods;

public class PaymentMethodResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string PaymentType { get; set; }
    public bool IsActive { get; set; }
    public Guid UserId { get; set; }
}