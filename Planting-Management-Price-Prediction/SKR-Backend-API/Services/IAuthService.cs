using SKR_Backend_API.DTOs;

namespace SKR_Backend_API.Services;

public interface IAuthService
{
    Task<AuthResponseDto> SignUpAsync(SignUpDto signUpDto);
    Task<AuthResponseDto> SignInAsync(SignInDto signInDto);
}

