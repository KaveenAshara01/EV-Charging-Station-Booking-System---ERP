using EvChargingAPI.Models;

namespace EvChargingAPI.Repositories
{
    public interface IStationRepository
    {
        Task<List<Station>> GetAllAsync();
        Task<Station?> GetByIdAsync(string id);
        Task CreateAsync(Station station);
        Task UpdateAsync(string id, Station station);
        Task DeleteAsync(string id);
    }
}
