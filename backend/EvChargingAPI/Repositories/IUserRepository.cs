using EvChargingAPI.Models;
using System.Threading.Tasks;

namespace EvChargingAPI.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
    }
}
