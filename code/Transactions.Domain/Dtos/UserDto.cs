namespace Transactions.Domain.Dtos
{
    public class UserDto
    {
        public required string Username { get; set; }
        public IEnumerable<string> Claims { get; set; } = [];
        public string? Email { get; set; }
    }
}
