using MongoDB.Driver;
using EvChargingAPI.Models;
using System.Threading.Tasks;

namespace EvChargingAPI.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly IMongoCollection<Reservation> _reservations;

        public ReservationRepository(IMongoDatabase database)
        {
            _reservations = database.GetCollection<Reservation>("Reservations");
        }

        public async Task<Reservation> GetReservationById(string id)
        {
            return await _reservations.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateReservation(Reservation reservation)
        {
            await _reservations.ReplaceOneAsync(r => r.Id == reservation.Id, reservation);
        }
    }
}
