using EvChargingAPI.DTOs;
using EvChargingAPI.Models;
using System.Threading.Tasks;

namespace EvChargingAPI.Services
{
    public interface IUserService
    {
        Task<UserResponseDto> RegisterAsync(RegisterUserDto dto);
        Task<UserResponseDto> LoginAsync(LoginUserDto dto);
    }
}
