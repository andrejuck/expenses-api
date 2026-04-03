namespace Transactions.Api.PresentationContracts.Families;

public class FamilyForm
{
    public required string FamilyName { get; set; }
    public required IEnumerable<string> Members { get; set; }
    public required IEnumerable<Guid> Accounts { get; set; }
}