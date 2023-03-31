using System.ComponentModel.DataAnnotations;

namespace JwtAuth.Requests
{
    public class NewUserRequest
    {
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
