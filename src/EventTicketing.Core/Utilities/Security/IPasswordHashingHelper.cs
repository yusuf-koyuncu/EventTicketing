namespace EventTicketing.Core.Utilities.Security;

public interface IPasswordHashingHelper
{
    string Hash(string password);

    bool Verify(string passwordHash, string password);
}
