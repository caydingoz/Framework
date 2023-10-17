using System.ComponentModel.DataAnnotations;

namespace Framework.Shared.Dtos.AuthServer
{
    public class EmailSignInInput
    {
        [Required(ErrorMessage = "Email is required")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
