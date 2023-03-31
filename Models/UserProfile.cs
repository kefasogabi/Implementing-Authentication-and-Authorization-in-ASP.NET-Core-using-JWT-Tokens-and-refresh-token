using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtAuth.Models
{
    [Table("UserProfiles", Schema = "dbo")]
    public class UserProfile
    {
        [Key]
        [StringLength(50)]
        public string UserId { get; set; }
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordSalt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
