using System.Diagnostics;
using Libs.Auth.Cryptography;

namespace Libs.Auth.Models
{
    public class User
    {
        public User(
            string email,
            string username)
        {
            Id = Guid.NewGuid();
            Email = email;
            Username = username;
            Roles = new List<UserRole>() { UserRole.GeneralUser };
            RegistrationStatus = RegistrationStatus.WaitingApproval;
            EmailConfirmed = false;
            CreatedAt = DateTime.Now;
        }

        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public string Salt { get; private set; }
        public List<UserRole> Roles { get; private set; }
        public RegistrationStatus RegistrationStatus { get; private set; }
        public bool EmailConfirmed { get; private set; }
        public DateTime? LoggedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        public virtual void UpdateRegistrationStatus(RegistrationStatus status)
        {
            RegistrationStatus = status;
            SetUpdateAt();
        }

        public virtual void ConfirmEmail()
        {
            EmailConfirmed = true;
            SetUpdateAt();
        }

        public virtual void UpdateLoggedAt()
        {
            LoggedAt = DateTime.Now;
        }

        public virtual void SetPassword(string password)
        {
            CryptPassword(password);
        }

        public virtual void UpdateUsername(string newUsername)
        {
            Username = newUsername;
            SetUpdateAt();
        }

        public virtual void SetDeleted()
        {
            DeletedAt = DateTime.Now;
            SetUpdateAt();
        }

        public bool VerifyPassword(string password)
        {
            var salt = Convert.FromBase64String(Salt);

            return Password == HashingHelper.HashPassword(password, salt);
        }

        public void AddRoles(params UserRole[] roles)
        {
            Roles.AddRange(roles);
        }

        private void SetUpdateAt()
        {
            UpdatedAt = DateTime.Now;
        }

        private void CryptPassword(string password)
        {
            var salt = HashingHelper.GenerateSalt();
            var pass = HashingHelper.HashPassword(password, salt);

            Password = pass;
            Salt = Convert.ToBase64String(salt);
            Debug.WriteLine(string.Format("{0} cript: {1}", salt, Salt));
        }

    }
}
