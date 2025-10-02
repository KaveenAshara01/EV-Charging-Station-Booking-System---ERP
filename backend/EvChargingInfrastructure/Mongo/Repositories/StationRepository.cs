using Domain.Interfaces;
using Domain.Entities;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Infrastructure.Mongo.Repositories
{
    public class StationRepository : IStationRepository
    {
        private readonly IMongoCollection<Station> _stations;
        private readonly IMongoCollection<Booking> _bookings;

        public StationRepository(IMongoContext ctx)
        {
            _stations = ctx.Database.GetCollection<Station>("Stations");
            _bookings = ctx.Database.GetCollection<Booking>("Bookings");
        }

        public async Task<bool> HasActiveBookingsAsync(string stationId)
            => await _bookings.Find(b => b.StationId == stationId && b.Status == "Active").AnyAsync();

        public Task InsertAsync(Station s) => _stations.InsertOneAsync(s);

        public Task UpdateActiveAsync(string stationId, bool isActive)
            => _stations.UpdateOneAsync(s => s.Id == stationId,
                                        Builders<Station>.Update.Set(x => x.IsActive, isActive));
    }

    public interface IStationRepository
    {
        Task<bool> HasActiveBookingsAsync(string stationId);
        Task InsertAsync(Station s);
        Task UpdateActiveAsync(string stationId, bool isActive);    
    }
}