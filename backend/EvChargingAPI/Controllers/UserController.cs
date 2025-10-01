using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Domain.Entities;
using Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IOwnerRepository _ownerRepo;
    public UserController(IOwnerRepository ownerRepo) => _ownerRepo = ownerRepo;

    // Create Backoffice or Operator user
    [HttpPost]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> CreateUser([FromBody] Owner user)
    {
        if (user.Roles == null || user.Roles.Length == 0)
            return BadRequest("Role is required (Backoffice or StationOperator)");
        await _ownerRepo.InsertAsync(user);
        return Ok(user);
    }

    // Update user
    [HttpPut("{id}")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] Owner user)
    {
        var existing = await _ownerRepo.FindByEmailAsync(id);
        if (existing == null)
            return NotFound();
        // Update properties
        existing.Name = user.Name;
        existing.Email = user.Email;
        existing.Phone = user.Phone;
        existing.Roles = user.Roles;
        await _ownerRepo.InsertAsync(existing); // or UpdateAsync
        return Ok(existing);
    }

    // Delete user
    [HttpDelete("{id}")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var existing = await _ownerRepo.FindByEmailAsync(id);
        if (existing == null)
            return NotFound();
        return Ok();
    }

    // Get all users (Backoffice only)
    [HttpGet]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> GetAllUsers()
    {
        // Implement logic to get all users
        // var users = await _ownerRepo.GetAllAsync();
        return Ok();
    }
}
