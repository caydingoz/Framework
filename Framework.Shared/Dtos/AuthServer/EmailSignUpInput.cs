using System.ComponentModel.DataAnnotations;

namespace Framework.Shared.Dtos.AuthServer
{
    public class EmailSignUpInput
    {
        [Required(ErrorMessage = "PhoneNumber is required")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email is required")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
