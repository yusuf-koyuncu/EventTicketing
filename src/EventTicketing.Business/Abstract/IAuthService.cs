using EventTicketing.Domain.Dtos;

namespace EventTicketing.Business.Abstract;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<bool> UserExistsAsync(string email, CancellationToken ct = default);
}
