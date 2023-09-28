namespace Framework.Shared.Dtos.AuthServer
{
    public class RefreshTokenInput
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
