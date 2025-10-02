using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStationRepository
    {
        Task<bool> HasActiveBookingsAsync(string stationId);
        Task InsertAsync(Station s);
        Task UpdateActiveAsync(string stationId, bool isActive);
    }

    public interface IBookingRepository
    {
        Task InsertAsync(Booking b);
        Task<bool> ExistsOverlapAsync(string stationId, DateTime startUtc, DateTime endUtc);
        Task<Booking?> GetAsync(string id);
        Task UpdateAsync(Booking b);
    }

}
