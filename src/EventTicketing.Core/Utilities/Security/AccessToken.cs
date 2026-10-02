namespace EventTicketing.Core.Utilities.Security;

public class AccessToken
{
    public AccessToken(string token, DateTime expirationUtc)
    {
        Token = token;
        ExpirationUtc = expirationUtc;
    }

    public string Token { get; set; }
    public DateTime ExpirationUtc { get; set; }
}
