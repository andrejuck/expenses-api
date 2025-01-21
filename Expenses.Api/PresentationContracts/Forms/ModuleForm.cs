using System.ComponentModel.DataAnnotations;

namespace Expenses.Api.PresentationContracts.Forms;

public class ModuleForm {
    
    [Required]
    public string Name { get; set; }
    [Required]
    public List<UserRole> AllowedRoles { get; set; }
}