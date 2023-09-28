namespace Framework.Shared.Dtos.AuthServer
{
    public class TokenOutput
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public long ExpiresIn { get; set; }
    }
}
