
using System;
namespace Domain.Entities;

public class Owner
{
    public string Id { get; set; } = default!;
    public string NIC { get; set; } = default!; // National Identity Card as PK
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; } = true;
}