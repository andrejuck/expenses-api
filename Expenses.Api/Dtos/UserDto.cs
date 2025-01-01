using System.Security.Claims;

namespace Expenses.Api.Dtos
{
    public class UserDto
    {
        public string Username { get; set; }
        public IEnumerable<string> Claims { get; set; }
    }
}
