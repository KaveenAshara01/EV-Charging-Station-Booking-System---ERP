using EvChargingAPI.Models;

namespace EvChargingAPI.Services
{
    public interface IStationService
    {
        Task<List<Station>> GetAllStationsAsync();
        Task<Station?> GetStationByIdAsync(string id);
        Task CreateStationAsync(Station station);
        Task UpdateStationAsync(string id, Station station);
        Task DeleteStationAsync(string id);
    }
}
