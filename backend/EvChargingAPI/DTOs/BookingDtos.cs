namespace Api.DTOs;

public class CreateBookingDto
{
    public string StationId { get; set; } = default!;
    public string OwnerId { get; set; } = default!;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
}

public class IdDto { public string Id { get; set; } = default!; }
