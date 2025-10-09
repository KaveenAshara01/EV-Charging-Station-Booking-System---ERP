using EvChargingAPI.Models;
using EvChargingAPI.Repositories;

namespace EvChargingAPI.Services
{
    public class StationService : IStationService
    {
        private readonly IStationRepository _stationRepository;

        public StationService(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        public async Task<List<Station>> GetAllStationsAsync()
        {
            return await _stationRepository.GetAllAsync();
        }

        public async Task<Station?> GetStationByIdAsync(string id)
        {
            return await _stationRepository.GetByIdAsync(id);
        }

        public async Task CreateStationAsync(Station station)
        {
            await _stationRepository.CreateAsync(station);
        }

        public async Task UpdateStationAsync(string id, Station station)
        {
            await _stationRepository.UpdateAsync(id, station);
        }

        public async Task DeleteStationAsync(string id)
        {
            await _stationRepository.DeleteAsync(id);
        }



    }
}
