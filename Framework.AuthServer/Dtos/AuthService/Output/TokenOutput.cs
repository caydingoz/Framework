﻿namespace Framework.AuthServer.Dtos.AuthService.Output
{
    public class TokenOutput
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public long ExpiresIn { get; set; }
    }
}
