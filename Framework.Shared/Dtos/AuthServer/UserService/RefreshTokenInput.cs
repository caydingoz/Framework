using System.ComponentModel.DataAnnotations;

namespace Framework.Shared.Dtos.AuthServer.UserService
{
    public class RefreshTokenInput
    {
        [Required(ErrorMessage = "AccessToken is required")]
        public required string AccessToken { get; set; }
        [Required(ErrorMessage = "RefreshToken is required")]
        public required string RefreshToken { get; set; }
    }
}
