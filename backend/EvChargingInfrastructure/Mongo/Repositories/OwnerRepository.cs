using Domain.Interfaces;
using Domain.Entities;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Infrastructure.Mongo.Repositories;

public class OwnerRepository : IOwnerRepository
{
    private readonly IMomgoCollection<Owner> _owners;
    public OwnerRepository(IMongoContext ctx)
    => _owners = ctx.Database.GetCollection<Owner>("Owners");

}

namespace Infrastructure.Mongo.Repositories
{
    public class OwnerRepository : IOwnerRepository
    {
        private readonly IMongoCollection<Owner> _owners;
        public OwnerRepository(IMongoContext ctx)
            => _owners = ctx.Database.GetCollection<Owner>("Owners");

        public Task<Owner?> FindByEmailAsync(string email)
            => _owners.Find(o => o.Email == email).FirstOrDefaultAsync();

        public Task<Owner?> FindByEmailAsync(string email)
        => _owners.Find(o => o.Email == email).FirstOrDefaultAsync();

        public Task InsertAsync(Owner owner) => _owners.InsertOneAsync(owner);
    }

    public interface IOwnerRepository
    {
        Task<Owner?> FindByEmailAsync(string email);
        Task InsertAsync(Owner owner);

        public Task InsertAsync(Owner owner) => _owners.InsertOneAsync(owner);

    }


}



