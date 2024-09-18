using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.AuthService.Input
{
    public class EmailRegisterInput
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email is required")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public required string Title { get; set; }
    }
}
