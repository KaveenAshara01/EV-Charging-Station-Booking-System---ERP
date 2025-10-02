using EvChargingAPI.Models;
using System.Threading.Tasks;

namespace EvChargingAPI.Repositories
{
    public interface IReservationRepository
    {
        Task<Reservation> GetReservationById(string id);
        Task UpdateReservation(Reservation reservation);
    }
}
