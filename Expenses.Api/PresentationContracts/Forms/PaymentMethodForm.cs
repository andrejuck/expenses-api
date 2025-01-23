using System.ComponentModel.DataAnnotations;
using Expenses.Domain.Models.Enum;

namespace Expenses.Api.PresentationContracts.Forms;

public class PaymentMethodForm
{
    [Required]
    public string Name { get; set; }
    [Required]
    public PaymentType PaymentType { get; set; }
    public bool IsActive { get; set; }
}