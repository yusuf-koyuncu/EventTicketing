using EventTicketing.Business.Abstract;
using EventTicketing.Core.DataAccess;
using EventTicketing.Core.Utilities.Security;
using EventTicketing.DataAccess.Abstract;
using EventTicketing.Domain.Dtos;
using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Enums;
using EventTicketing.Domain.Exceptions;

namespace EventTicketing.Business.Concrete;

public class AuthManager : IAuthService
{
    private readonly IUserDal _userDal;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashingHelper _passwordHashing;
    private readonly ITokenHelper _tokenHelper;

    public AuthManager(
        IUserDal userDal,
        IUnitOfWork unitOfWork,
        IPasswordHashingHelper passwordHashing,
        ITokenHelper tokenHelper)
    {
        _userDal = userDal;
        _unitOfWork = unitOfWork;
        _passwordHashing = passwordHashing;
        _tokenHelper = tokenHelper;
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequest request, CancellationToken ct = default)
    {
        var email = Normalise(request.Email);

        if (await _userDal.EmailExistsAsync(email, ct))
            throw new BusinessRuleException($"{request.Email} is already registered.");

        var user = new AppUser
        {
            Email = email,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHashing.Hash(request.Password),
            Role = UserRole.Customer
        };

        await _userDal.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return CreateResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userDal.GetByEmailAsync(Normalise(request.Email), ct);

        if (user is null || !_passwordHashing.Verify(user.PasswordHash, request.Password))
            throw new InvalidCredentialsException("Email or password is incorrect.");

        return CreateResponse(user);
    }

    public Task<bool> UserExistsAsync(string email, CancellationToken ct = default) =>
        _userDal.EmailExistsAsync(Normalise(email), ct);

    private AuthResponseDto CreateResponse(AppUser user)
    {
        var accessToken = _tokenHelper.CreateToken(
            user.Id, user.Email, user.FullName, user.Role.ToString());

        return new AuthResponseDto(
            accessToken.Token,
            accessToken.ExpirationUtc,
            user.Id,
            user.Email,
            user.FullName,
            user.Role);
    }

    private static string Normalise(string email) => email.Trim().ToLowerInvariant();
}
