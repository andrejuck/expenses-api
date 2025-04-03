namespace Libs.Auth.Models
{
    public static class AuthMessages
    {
        public const string INVALID_EMAIL_PASSWORD = "Invalid email or password.";
        public const string INVALID_EMAIL_PASSWORD_LOGIN = "Invalid email or password. {login}";
        public const string INVALID_EMAIL_ALREADY_EXISTS = "Email {email} already exists.";
        public const string INVALID_CONFIRMATION_EMAIL_EXPIRED = "Confirmation email expired.";
        public const string SUCCESS_REGISTERED = "User {user} registered successfully.";
        public const string SUCCESS_LOGIN = "User {user} logged successfully.";
        public const string SUCCESS_CONFIRMED_EMAIL = "Email confirmed successfully.";
    }
}
