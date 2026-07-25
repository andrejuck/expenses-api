using System.ComponentModel.DataAnnotations;

namespace Transactions.Api.PresentationContracts.Forms;

public class ModuleForm
{

    [Required]
    public string Name { get; set; }
    [Required]
    public List<string> AllowedRoles { get; set; }
}