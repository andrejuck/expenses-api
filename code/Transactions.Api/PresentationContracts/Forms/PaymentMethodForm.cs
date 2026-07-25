using Transactions.Domain.Models.Enum;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Transactions.Api.PresentationContracts.Forms;

public class PaymentMethodForm
{
    [Required]
    public string Name { get; set; }
    [Required]
    public PaymentType PaymentType { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}