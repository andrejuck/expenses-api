using System.ComponentModel.DataAnnotations;
using Expenses.Domain.Models.Enum;
using Newtonsoft.Json;

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