using System;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IBookingRepository
    {
        Task InsertAsync(Booking b);
        Task<bool> ExistsOverlapAsync(string stationId, DateTime startUtc, DateTime endUtc);
        Task<Booking?> GetAsync(string id);
        Task UpdateAsync(Booking b);
    }
}
