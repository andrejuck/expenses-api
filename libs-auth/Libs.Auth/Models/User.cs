
namespace Libs.Auth.Models
{
    public class User
    {
        public User(
            string email, 
            string username,
            string password)
        {
            Id = Guid.NewGuid();
            Email = email;
            Username = username;
            Password = password;
            Roles = new List<UserRole>() { UserRole.GeneralUser };
            RegistrationStatus = RegistrationStatus.WaitingApproval;
            EmailConfirmed = false;
            CreatedAt = DateTime.Now;
        }

        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public List<UserRole> Roles { get; private set; }
        public RegistrationStatus RegistrationStatus { get; private set; }
        public bool EmailConfirmed { get; private set; }
        public DateTime? LoggedAt { get; private set; }    
        public DateTime? UpdatedAt { get;private set; }
        public DateTime CreatedAt { get; private set; }

        public virtual void UpdateRegistrationStatus(RegistrationStatus status) {
            RegistrationStatus = status;
            SetUpdateAt();
        }

        public virtual void ConfirmEmail() {
            EmailConfirmed = true;
            SetUpdateAt();
        }

        public virtual void UpdateLoggedAt() {
            LoggedAt = DateTime.Now;
        }

        public virtual void SetNewPassword(string password) {
            Password = password;
            SetUpdateAt();
        }

        public virtual void UpdateUsername(string newUsername) {
            Username = newUsername;
            SetUpdateAt();
        }

        private void SetUpdateAt() {
            UpdatedAt = DateTime.Now;
        }

    }
}
