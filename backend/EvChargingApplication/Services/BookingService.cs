using Domain.Interfaces;
using Domain.Entities;

namespace Application.Services;

public interface IBookingService
{
    Task<string> CreateAsync(Booking b); // returns Id
    Task CancelAsync(string bookingId);
    Task<Booking?> GetAsync(string bookingId);
}

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repo;
    private readonly IStationRepository _stations;

    public BookingService(IBookingRepository repo, IStationRepository stations)
    {
        _repo = repo;
        _stations = stations;
    }

    public async Task<string> CreateAsync(Booking b)
    {
        // Rule 1: reservation within 7 days
        if (b.StartUtc > DateTime.UtcNow.AddDays(7))
            throw new InvalidOperationException("Reservation must be within 7 days from today.");

        // Rule 2: station overlap prevention
        if (await _repo.ExistsOverlapAsync(b.StationId, b.StartUtc, b.EndUtc))
            throw new InvalidOperationException("Overlapping booking exists for this station.");

        b.Id = Guid.NewGuid().ToString("N");
        b.Status = "Active";
        await _repo.InsertAsync(b);
        return b.Id;
    }

    public async Task CancelAsync(string bookingId)
    {
        var b = await _repo.GetAsync(bookingId) ?? throw new KeyNotFoundException("Booking not found.");

        // Rule 3: modify/cancel >= 12 hours before start
        if (b.StartUtc - DateTime.UtcNow < TimeSpan.FromHours(12))
            throw new InvalidOperationException("Cancellations must be made at least 12 hours before start.");

        b.Status = "Cancelled";
        await _repo.UpdateAsync(b);
    }

    public async Task<Booking?> GetAsync(string bookingId)
    {
        return await _repo.GetAsync(bookingId);
    }
}
