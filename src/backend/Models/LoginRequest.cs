namespace Prodigy.Backend.Models
{
    /// <summary>
    /// Login request model for username/password authentication
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's password
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
