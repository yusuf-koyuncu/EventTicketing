using Microsoft.AspNetCore.Identity;

namespace EventTicketing.Core.Utilities.Security.Hashing;

public class PasswordHashingHelper : IPasswordHashingHelper
{
    private readonly PasswordHasher<object> _hasher = new();
    private static readonly object HashingSubject = new();

    public string Hash(string password) => _hasher.HashPassword(HashingSubject, password);

    public bool Verify(string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(HashingSubject, passwordHash, password)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}
