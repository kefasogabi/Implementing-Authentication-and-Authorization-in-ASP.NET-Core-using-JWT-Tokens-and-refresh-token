namespace JwtAuth.Responses
{
    public class AuthenticatedResponse : Response
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
