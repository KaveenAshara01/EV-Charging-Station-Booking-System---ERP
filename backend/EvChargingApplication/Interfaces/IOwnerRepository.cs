using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOwnerRepository
    {
        Task<Owner?> FindByEmailAsync(string email);
        Task InsertAsync(Owner owner);
    }
}
