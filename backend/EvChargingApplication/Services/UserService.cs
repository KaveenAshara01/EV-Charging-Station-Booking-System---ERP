using Domain.Entities;
using Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserService
    {
        private readonly Domain.Interfaces.IUserRepository _userRepository;

        public UserService(Domain.Interfaces.IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<Domain.Entities.User>> GetAllAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<Domain.Entities.User> GetByIdAsync(string id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task CreateAsync(Domain.Entities.User user)
        {
            await _userRepository.CreateAsync(user);
        }

        public async Task UpdateAsync(string id, Domain.Entities.User user)
        {
            await _userRepository.UpdateAsync(id, user);
        }

        public async Task DeleteAsync(string id)
        {
            await _userRepository.DeleteAsync(id);
        }
    }
}