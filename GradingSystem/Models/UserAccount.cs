using System.ComponentModel.DataAnnotations;

namespace GradingSystem.Models
{
    public class UserAccount
    {


        [Key]
        public int Id { get; set; } // Primary key

        [Required(ErrorMessage = "Username is required")]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "User"; // Example: "Admin" or "User"


        public bool IsApproved { get; set; } = false;
        public bool IsPending { get; set; } = true;
    }
}
