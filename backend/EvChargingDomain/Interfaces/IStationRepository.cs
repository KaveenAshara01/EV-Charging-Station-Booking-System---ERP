using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IStationRepository
    {
        Task<bool> HasActiveBookingsAsync(string stationId);
        Task InsertAsync(Station s);
        Task UpdateActiveAsync(string stationId, bool isActive);
    }
}
