namespace EventTicketing.Core.Utilities.Security;

public interface ITokenHelper
{
    AccessToken CreateToken(int userId, string email, string fullName, string role);
}
