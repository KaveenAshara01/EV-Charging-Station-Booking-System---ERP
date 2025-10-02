namespace Domain.Entities
{
    public class Station
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public bool IsActive { get; set; }
        // Add other properties as needed
    }
}
