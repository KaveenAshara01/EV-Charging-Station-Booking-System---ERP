using EvChargingAPI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace EvChargingAPI.Repositories
{
    public class StationRepository : IStationRepository
    {
        private readonly IMongoCollection<Station> _stations;

        // public StationRepository(IConfiguration configuration)
        // {
        //     var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
        //     var database = client.GetDatabase("ev_charging_db");
        //     _stations = database.GetCollection<Station>("Stations");
        // }
        public StationRepository(IMongoDatabase database)
        {
            
            _stations = database.GetCollection<Station>("Stations");
        }

          

        public async Task<List<Station>> GetAllAsync()
        {
            return await _stations.Find(_ => true).ToListAsync();
        }

        public async Task<Station?> GetByIdAsync(string id)
        {
            return await _stations.Find(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Station station)
        {
            await _stations.InsertOneAsync(station);
        }

        public async Task UpdateAsync(string id, Station station)
        {
            await _stations.ReplaceOneAsync(s => s.Id == id, station);
        }

        public async Task DeleteAsync(string id)
        {
            await _stations.DeleteOneAsync(s => s.Id == id);
        }
    }
}
