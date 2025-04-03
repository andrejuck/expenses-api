using Expenses.Domain.Models.Enum;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Expenses.Api.PresentationContracts.Forms;

public class PaymentMethodForm
{
    [Required]
    [JsonProperty("name")]
    public string Name { get; set; }
    [Required]
    [JsonProperty("paymentType")]
    public PaymentType PaymentType { get; set; }
    [JsonProperty("isActive")]
    public bool IsActive { get; set; }
}