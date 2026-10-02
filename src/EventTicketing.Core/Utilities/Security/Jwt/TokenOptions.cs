namespace EventTicketing.Core.Utilities.Security.Jwt;

public class TokenOptions
{
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string SecurityKey { get; set; } = null!;
    public int AccessTokenExpirationMinutes { get; set; } = 60;
}
