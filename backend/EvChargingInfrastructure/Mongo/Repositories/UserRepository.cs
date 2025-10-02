using Domain.Entities;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Mongo.Repositories
{
    public class UserRepository : Domain.Interfaces.IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IMongoContext ctx)
        {
            _users = ctx.Database.GetCollection<User>("Users");
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateAsync(string id, User user)
        {
            await _users.ReplaceOneAsync(u => u.Id == id, user);
        }

        public async Task DeleteAsync(string id)
        {
            await _users.DeleteOneAsync(u => u.Id == id);
        }
    }
}