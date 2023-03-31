namespace JwtAuth.Requests
{
    public class TokenModelRequest
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
