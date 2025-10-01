namespace Domain.Entities
{
    public class User
    {
        public string? Id { get; set; } // Could be NIC or system-generated
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; } // Backoffice, StationOperator
        public bool IsActive { get; set; }
    }
}