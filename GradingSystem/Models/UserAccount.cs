using System.ComponentModel.DataAnnotations;

namespace GradingSystem.Models
{
    public class UserAccount
    {
        [Key]
        public int Id { get; set; }

        // 🔹 Email (used for login)
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        // 🔹 Password (hashed)
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = null!;

        // 🔹 Role (Admin / Teacher / Student)
        [Required(ErrorMessage = "Role is required")]
        [StringLength(20)]
        public string Role { get; set; } = "User";

        // 🔹 Approval status flags
        public bool IsApproved { get; set; } = false; // approved by admin
        public bool IsPending { get; set; } = true;   // waiting for approval

        // 🔹 Optional: Timestamp tracking
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? ApprovedAt { get; set; }
    }
}
