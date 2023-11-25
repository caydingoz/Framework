using System.ComponentModel.DataAnnotations;

namespace Framework.Shared.Dtos.AuthServer.UserService
{
    public class EmailLoginInput
    {
        [Required(ErrorMessage = "Email is required")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
